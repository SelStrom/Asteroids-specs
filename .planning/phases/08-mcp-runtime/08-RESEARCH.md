# Phase 08: MCP Runtime — Research

**Researched:** 2026-03-29
**Domain:** Unity Editor Bridge, C# Reflection, Runtime-to-Editor State Sharing, WebGL Compilation Guards
**Confidence:** HIGH

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**D-01: Семантика `isPlaying`**
`isPlaying = EditorApplication.isPlaying && Game.IsRunning`
Где `Game.IsRunning` — новое публичное свойство (`public bool IsRunning => _isRunning`).

**D-02: Стратегия обновления буфера**
`RuntimeBridgeProxy.UpdateState(score, wave, lives, isRunning)` вызывается каждый кадр
из `ApplicationEntry.Update()` — всегда актуально, оверхед минимален.
При выходе из Play Mode (`ApplicationEntry.OnDestroy` или `OnDisable`) — сброс в нули.

**D-03: Доступ McpUnityBridge к RuntimeBridgeProxy**
McpUnityBridge читает состояние через **reflection**:
```csharp
var type = Type.GetType("SelStrom.Asteroids.RuntimeBridgeProxy, Assembly-CSharp");
var score = (int)type.GetField("Score").GetValue(null);
```
Нет прямой зависимости пакета от игрового кода.
Если type == null — возвращать `isPlaying: false`, остальные поля 0.

**D-04: Расположение RuntimeBridgeProxy**
`Assets/Scripts/Application/RuntimeBridgeProxy.cs` — Runtime сборка (Assembly-CSharp).
Структура:
```csharp
namespace SelStrom.Asteroids
{
    public static class RuntimeBridgeProxy
    {
#if UNITY_EDITOR
        public static int Score;
        public static int Wave;
        public static int Lives;
        public static bool IsRunning;

        public static void UpdateState(int score, int wave, int lives, bool isRunning) { ... }
        public static void Reset() { ... }
#else
        // WebGL no-op заглушки
        public static void UpdateState(int score, int wave, int lives, bool isRunning) { }
        public static void Reset() { }
#endif
    }
}
```

**D-05: Endpoint в McpUnityBridge**
Добавить `case "/get_game_state":` в switch `HandleRequest`.
Метод `HandleGetGameState` использует reflection (D-03), возвращает:
```json
{ "success": true, "score": 0, "wave": 1, "lives": 3, "isPlaying": false }
```

**D-06: Game.IsRunning**
Добавить `public bool IsRunning => _isRunning;` в `Game.cs`.
`_isRunning` уже существует — нужно только публичное свойство.

**D-07: Обновление ApplicationEntry**
В `Update()`: `RuntimeBridgeProxy.UpdateState(_game?.Score ?? 0, _game?.Wave ?? 0, _game?.Lives ?? 0, _game?.IsRunning ?? false);`
В `OnDisable()` или `OnDestroy()`: `RuntimeBridgeProxy.Reset();`
`Game.Wave` — новое свойство `public int Wave => _waveNumber;`

### Claude's Discretion
_(не указано — все аспекты реализации детально описаны в Decisions)_

### Deferred Ideas (OUT OF SCOPE)
- get_entity_list, get_asteroid_positions — расширения MCP (backlog)
- MCP tool для restart/shoot — управление игрой через MCP (backlog)
</user_constraints>

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| MCP-07 | MCP tool `get_game_state` — возвращает JSON с `{ score, wave, lives, isPlaying }` (из Runtime bridge) | D-03, D-05: reflection в McpUnityBridge + endpoint в switch/case |
| MCP-11 | `Runtime/RuntimeBridgeProxy.cs` с `#if UNITY_EDITOR` guard — пишет состояние игры в статический буфер, доступный Editor bridge; в WebGL-билде компилируется в no-op заглушки | D-04: файл в Assembly-CSharp, условная компиляция |
</phase_requirements>

---

## Summary

Фаза реализует двустороннюю связь между Unity Runtime (игровой код, Assembly-CSharp) и MCP-сервером (TypeScript, stdio) через промежуточный HTTP-мост McpUnityBridge. Архитектура намеренно избегает прямой сборочной зависимости пакета `com.shtl.mcp-unity` от игрового кода: McpUnityBridge читает состояние через C# reflection по имени типа `SelStrom.Asteroids.RuntimeBridgeProxy, Assembly-CSharp`.

`RuntimeBridgeProxy` — статический класс в Runtime-сборке (Assembly-CSharp) с `#if UNITY_EDITOR` guard. В Editor-сборке он содержит поля-буферы и методы обновления; в WebGL-сборке компилируется в пустые заглушки, что полностью устраняет зависимость от Editor API в production-билде. `ApplicationEntry.Update()` обновляет буфер каждый кадр.

На стороне TypeScript MCP-сервера добавляется инструмент `get_game_state` с вызовом `/get_game_state` на bridge. На стороне C# добавляется `case "/get_game_state":` в существующий switch в `McpUnityBridge.HandleRequest`, метод `HandleGetGameState` делает reflection к прокси. Вся логика укладывается в 5 файлов: 1 новый (`RuntimeBridgeProxy.cs`) + 4 изменённых (`Game.cs`, `ApplicationEntry.cs`, `McpUnityBridge.cs`, `index.ts`).

**Primary recommendation:** Реализовывать в строгом порядке: RuntimeBridgeProxy → Game.IsRunning + Game.Wave → ApplicationEntry → McpUnityBridge → TypeScript. Каждый шаг компилируется до следующего.

---

## Standard Stack

### Core
| Компонент | Версия | Назначение | Почему |
|-----------|--------|-----------|--------|
| C# Reflection (`System.Reflection`) | .NET 4.x (Unity 2022.3) | Чтение статических полей RuntimeBridgeProxy из McpUnityBridge без сборочной зависимости | Единственный способ читать из Runtime в Editor без coupling |
| `#if UNITY_EDITOR` | встроен в Unity | Условная компиляция для WebGL-guard | Стандарт Unity для Editor-only кода в Runtime-сборках |
| `EditorApplication.isPlaying` | Unity Editor API | Проверка, находится ли Editor в Play Mode | Официальный API |
| `@modelcontextprotocol/sdk` | существующая версия в проекте | MCP TypeScript server | Уже используется в index.ts |
| `zod` | существующая версия в проекте | Валидация параметров MCP tools | Уже используется в index.ts |

### Поддерживающие
| Компонент | Назначение | Когда |
|-----------|-----------|-------|
| `Type.GetType(string)` | Поиск типа по полному имени включая сборку | При каждом `/get_game_state` запросе |
| `FieldInfo.GetValue(null)` | Чтение static поля через reflection | Для Score, Wave, Lives, IsRunning |

---

## Architecture Patterns

### Рекомендуемая структура файлов (изменения фазы)

```
Assets/Scripts/Application/
└── RuntimeBridgeProxy.cs        # НОВЫЙ — static буфер с #if UNITY_EDITOR

Assets/Scripts/Application/
├── Game.cs                       # +IsRunning property, +Wave property
└── ApplicationEntry.cs           # +UpdateState в Update(), +Reset() в OnDestroy()

Packages/com.shtl.mcp-unity/
├── Editor/McpUnityBridge.cs      # +case "/get_game_state", +HandleGetGameState()
└── Editor~/Server/src/index.ts   # +tool "get_game_state"
```

### Pattern 1: Static Buffer с #if UNITY_EDITOR
**Что:** Статический класс-прокси в Runtime-сборке, который в Editor-окружении хранит буфер состояния, а в WebGL компилируется в no-op.
**Когда использовать:** Любая ситуация, когда Runtime-код должен экспортировать состояние в Editor без прямой зависимости от Editor API.

```csharp
// Assets/Scripts/Application/RuntimeBridgeProxy.cs
// Источник: CONTEXT.md D-04, верифицировано паттерном из Unity docs conditional compilation

namespace SelStrom.Asteroids
{
    public static class RuntimeBridgeProxy
    {
#if UNITY_EDITOR
        public static int Score;
        public static int Wave;
        public static int Lives;
        public static bool IsRunning;

        public static void UpdateState(int score, int wave, int lives, bool isRunning)
        {
            Score = score;
            Wave = wave;
            Lives = lives;
            IsRunning = isRunning;
        }

        public static void Reset()
        {
            Score = 0;
            Wave = 0;
            Lives = 0;
            IsRunning = false;
        }
#else
        public static void UpdateState(int score, int wave, int lives, bool isRunning) { }
        public static void Reset() { }
#endif
    }
}
```

### Pattern 2: Reflection-based чтение из Editor Bridge
**Что:** McpUnityBridge читает статические поля RuntimeBridgeProxy через C# Reflection без прямой ссылки на сборку.
**Когда использовать:** Editor-код в пакете должен читать данные из игровой сборки, но не может иметь compile-time зависимость.

```csharp
// Packages/com.shtl.mcp-unity/Editor/McpUnityBridge.cs
// Источник: CONTEXT.md D-03, D-05

private static void HandleGetGameState(HttpListenerContext context)
{
    var type = System.Type.GetType("SelStrom.Asteroids.RuntimeBridgeProxy, Assembly-CSharp");

    if (type == null || !UnityEditor.EditorApplication.isPlaying)
    {
        SendJsonRaw(context, "{\"success\":true,\"score\":0,\"wave\":0,\"lives\":0,\"isPlaying\":false}");
        return;
    }

    var score = (int)type.GetField("Score").GetValue(null);
    var wave  = (int)type.GetField("Wave").GetValue(null);
    var lives = (int)type.GetField("Lives").GetValue(null);
    var isRunning = (bool)type.GetField("IsRunning").GetValue(null);
    var isPlaying = UnityEditor.EditorApplication.isPlaying && isRunning;

    var sb = new System.Text.StringBuilder();
    sb.Append("{\"success\":true,\"score\":");
    sb.Append(score);
    sb.Append(",\"wave\":");
    sb.Append(wave);
    sb.Append(",\"lives\":");
    sb.Append(lives);
    sb.Append(",\"isPlaying\":");
    sb.Append(isPlaying ? "true" : "false");
    sb.Append("}");

    SendJsonRaw(context, sb.ToString());
}
```

### Pattern 3: ApplicationEntry Update с null-guard
**Что:** Обновление буфера каждый кадр из MonoBehaviour.Update с защитой от null (_game может быть null до Start).

```csharp
// Assets/Scripts/Application/ApplicationEntry.cs
// Источник: CONTEXT.md D-07

private void Update()
{
    OnUpdate?.Invoke(Time.deltaTime);
    RuntimeBridgeProxy.UpdateState(
        _application?.Score ?? 0,
        _application?.Wave ?? 0,
        _application?.Lives ?? 0,
        _application?.IsRunning ?? false
    );
}

private void OnDestroy()
{
    _application?.Dispose();
    RuntimeBridgeProxy.Reset();
}
```

### Pattern 4: MCP TypeScript tool без параметров
**Что:** Добавление нового tool в index.ts по аналогии с существующими (play, stop, compile).

```typescript
// Packages/com.shtl.mcp-unity/Editor~/Server/src/index.ts
// Источник: существующий index.ts, аналогия с "play"/"stop"

server.tool(
  "get_game_state",
  "Get current game state: score, wave, lives, isPlaying. Returns { score, wave, lives, isPlaying }.",
  {},
  async () => {
    const text = await callBridge("/get_game_state");
    return { content: [{ type: "text", text }] };
  }
);
```

### Anti-Patterns to Avoid

- **Прямая ссылка asmdef:** НЕ добавлять `McpUnityBridge.asmdef` ссылку на `Asteroids` (игровую сборку) — нарушает изоляцию пакета. Reflection — единственный правильный путь.
- **Поля без #if UNITY_EDITOR:** Поля `Score`, `Wave`, `Lives`, `IsRunning` ДОЛЖНЫ быть под `#if UNITY_EDITOR`. Если оставить их в WebGL, Unity включит их в билд, что увеличит размер и может сломать WebGL из-за threading-зависимостей статических инициализаторов.
- **Reset только в OnDestroy:** OnDestroy вызывается после перезагрузки домена. Нужен также вызов в `OnDisable` или `OnApplicationQuit` для корректного сброса при выходе из Play Mode без перезагрузки домена.
- **Reflection без null-check:** `type.GetField("Score")` может вернуть null если поле переименовано. Всегда проверять `type != null` перед reflection.
- **JSON float вместо int:** Score, Wave, Lives — целые числа. Используй `(int)`, не `(float)` при reflection.

---

## Don't Hand-Roll

| Проблема | Не строить | Использовать | Почему |
|----------|-----------|--------------|--------|
| JSON-сериализация | Кастомный JSON-парсер | `StringBuilder` с ручным форматированием | Уже используется в McpUnityBridge — JsonUtility не поддерживает анонимные типы (D-02 из STATE.md Phase 02) |
| Межсборочная коммуникация | Интерфейсы/события между сборками | Reflection через `Assembly-CSharp` | Нет compile-time зависимости пакета от игры |
| Play Mode detection | Кастомный флаг | `EditorApplication.isPlaying` | Официальный Unity API |

---

## Common Pitfalls

### Pitfall 1: WebGL-сборка сломана из-за Editor API в Runtime
**Что идёт не так:** Если в RuntimeBridgeProxy включить `using UnityEditor;` или Editor-only API без `#if UNITY_EDITOR`, WebGL-билд падает с ошибкой компиляции — `UnityEditor` недоступен в runtime.
**Почему:** Директива `using UnityEditor` вне `#if UNITY_EDITOR` блока проходит через компилятор в WebGL-сборке, где сборка `UnityEditor.dll` отсутствует.
**Как избежать:** В `RuntimeBridgeProxy.cs` НЕ ставить `using UnityEditor;` на верхнем уровне файла. Все поля и методы, зависящие от Editor, — строго внутри `#if UNITY_EDITOR`.
**Признаки:** Ошибка `The type or namespace name 'EditorApplication' does not exist in the namespace 'UnityEditor'` при WebGL-билде.

### Pitfall 2: Reflection возвращает null для type
**Что идёт не так:** `Type.GetType("SelStrom.Asteroids.RuntimeBridgeProxy, Assembly-CSharp")` возвращает null в нескольких случаях: до первого Play Mode, после перезагрузки домена, если класс ещё не загружен.
**Почему:** Assembly-CSharp загружается при первом входе в Play Mode. В Edit Mode между перезагрузками тип недоступен.
**Как избежать:** Всегда проверять `type != null` перед любым reflection. При null — возвращать `{ isPlaying: false }` без ошибок (D-03 из CONTEXT.md).
**Признаки:** `NullReferenceException` в HandleGetGameState при вызове вне Play Mode.

### Pitfall 3: ApplicationEntry не имеет прямого доступа к Game
**Что идёт не так:** `ApplicationEntry` не хранит ссылку на `Game` напрямую — он работает через `Application`. Поля `Score`, `Wave`, `Lives`, `IsRunning` нужно пробрасывать через `Application`.
**Почему:** Паттерн проекта — `ApplicationEntry → Application → Game`. ApplicationEntry не знает о Game.
**Как избежать:** Добавить публичные свойства `Score`, `Wave`, `Lives`, `IsRunning` в класс `Application.cs`, которые делегируют к `_game`. Тогда в `ApplicationEntry.Update()` вызывать `_application?.Score`.
**Признаки:** Компиляционная ошибка `'ApplicationEntry' does not contain a definition for '_game'`.

### Pitfall 4: OnDestroy vs OnDisable для Reset
**Что идёт не так:** `OnDestroy` не вызывается при каждом выходе из Play Mode — вызывается только при уничтожении объекта. При Domain Reload Disabled (Editor Settings) поведение отличается.
**Почему:** Unity Domain Reload поведение зависит от настроек проекта. OnDisable вызывается надёжнее при Stop Play Mode.
**Как избежать:** Вызывать `RuntimeBridgeProxy.Reset()` в `OnDestroy()`. Это достаточно для стандартных настроек Unity 2022.3 LTS с Domain Reload включённым (по умолчанию).
**Признаки:** После Stop Play Mode `get_game_state` возвращает старые значения вместо нулей.

### Pitfall 5: Game.Wave vs _waveNumber
**Что идёт не так:** В `Game.cs` публичное свойство `WaveNumber` уже существует (`public int WaveNumber => _waveNumber`). CONTEXT.md D-07 говорит добавить `public int Wave => _waveNumber`.
**Почему:** ApplicationEntry обращается к Game через Application, и важно использовать правильное имя свойства.
**Как избежать:** Использовать уже существующее `WaveNumber` или добавить `Wave` как алиас. Либо выставить через `Application.Wave` делегирующее свойство. Проверить перед кодированием.
**Признаки:** Компиляционная ошибка при обращении к несуществующему `Game.Wave`.

### Pitfall 6: StringBuilder в HandleGetGameState должен использовать bool без кавычек
**Что идёт не так:** JSON `"isPlaying": "true"` (строка) vs `"isPlaying": true` (булево). Если передать строку, TypeScript-сторона получит строку вместо boolean.
**Почему:** `sb.Append(isPlaying)` для bool вызывает `.ToString()` → `"True"` с заглавной буквы, что невалидно в JSON.
**Как избежать:** `sb.Append(isPlaying ? "true" : "false")` — явная строка без кавычек.
**Признаки:** TypeScript парсер получает `"True"` вместо `true`.

---

## Code Examples

Полные верифицированные паттерны из анализа кодовой базы:

### Существующая структура switch в McpUnityBridge.HandleRequest (строки 146-188)
```csharp
// Packages/com.shtl.mcp-unity/Editor/McpUnityBridge.cs (существующий код)
switch (path)
{
    case "/compile":
        HandleCompile(context);
        break;
    case "/play":
        HandlePlay(context);
        break;
    // ... другие cases ...
    default:
        context.Response.StatusCode = 404;
        SendJsonRaw(context, "{\"success\":false,\"message\":\"Unknown endpoint\"}");
        break;
}
```
Новый `case "/get_game_state":` добавляется перед `default:`.

### Существующий паттерн SendJsonRaw в McpUnityBridge
```csharp
// Источник: McpUnityBridge.cs строка 645
private static void SendJsonRaw(HttpListenerContext context, string json)
{
    // ... существующая реализация ...
}
```
HandleGetGameState использует тот же `SendJsonRaw`.

### Существующие публичные свойства Game.cs (строки 49-52)
```csharp
// Assets/Scripts/Application/Game.cs — уже существует
public int Score => _model.Score;
public int Lives => _lives;
public int HighScore => _highScore;
public int WaveNumber => _waveNumber;
// _isRunning существует (строка 19), публичного IsRunning нет — нужно добавить
```

### Существующий ApplicationEntry.Update (строки 49-52)
```csharp
// Assets/Scripts/Application/ApplicationEntry.cs — расширяется
private void Update()
{
    OnUpdate?.Invoke(Time.deltaTime);
    // Добавить здесь: RuntimeBridgeProxy.UpdateState(...)
}
```

---

## Runtime State Inventory

> Фаза не является rename/refactor/migration — инвентаризация Runtime State не применяется.

---

## Environment Availability

| Зависимость | Нужна для | Доступна | Версия | Fallback |
|-------------|-----------|---------|--------|----------|
| Unity 2022.3 Editor | Компиляция C#, EditorApplication API | ✓ | 2022.3.x | — |
| Node.js | Пересборка TypeScript MCP-сервера | Проверить | — | Использовать существующий dist/ |
| Assembly-CSharp (runtime) | Reflection в Editor | ✓ | автоматически | — |

**Пересборка TypeScript:** Если Node.js доступен — после изменения `index.ts` нужен `npm run build` в директории `Editor~/Server/`. Если dist/ уже существует и структура сервера совпадает — можно пересобрать только при наличии Node.

---

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | Ручное тестирование в Unity Editor (нет авто-тестов для Editor bridge) |
| Config file | нет |
| Quick run command | MCP tool `get_game_state` через Claude Code |
| Full suite command | Play Mode + вызов `get_game_state` через MCP |

### Phase Requirements → Test Map
| Req ID | Поведение | Тип теста | Автоматическая команда | Файл существует? |
|--------|-----------|-----------|----------------------|-----------------|
| MCP-07 | `get_game_state` возвращает `{ score, wave, lives, isPlaying }` в Play Mode | Smoke/manual | MCP tool вызов в Play Mode | ❌ Wave 0 (manual) |
| MCP-11 | `RuntimeBridgeProxy.cs` компилируется без ошибок в WebGL-сборке | Build check | `compile` MCP tool после изменений | ✅ (compile tool существует) |

### Success Criteria (из CONTEXT.md)
1. MCP tool `get_game_state` возвращает `{ score, wave, lives, isPlaying }` во время Editor Play Mode
2. `RuntimeBridgeProxy.cs` компилируется без ошибок в WebGL-сборке (вся логика под `#if UNITY_EDITOR`)
3. При вызове `get_game_state` вне Play Mode возвращается `{ isPlaying: false }` без ошибок

### Sampling Rate
- **Per task commit:** `compile` MCP tool — проверить отсутствие ошибок компиляции
- **Per wave merge:** Enter Play Mode → вызвать `get_game_state` → проверить JSON
- **Phase gate:** Все 3 success criteria выполнены перед `/gsd:verify-work`

### Wave 0 Gaps
- [ ] Нет автоматизированных тестов для Editor bridge — тестирование только через MCP вручную
- [ ] TypeScript rebuild: `npm run build` в `Packages/com.shtl.mcp-unity/Editor~/Server/` после изменения index.ts

---

## State of the Art

| Старый подход | Текущий подход | Когда изменился | Влияние |
|---------------|---------------|----------------|---------|
| Прямая зависимость пакета на игровой код | Reflection через `Assembly-CSharp` | Решение в Phase 08 CONTEXT.md | Пакет не зависит от игрового кода — переносим |
| EditorApplication.delayCall из фонового потока | ConcurrentQueue + DrainQueue в главном потоке | Phase 02 quick fix (STATE.md) | Стабильность Thread dispatching |

---

## Open Questions

1. **Доступ к Game через Application**
   - Что знаем: ApplicationEntry работает с `Application`, не с `Game` напрямую
   - Неясно: Есть ли в `Application.cs` публичные свойства Score/Wave/Lives/IsRunning или нужно добавить
   - Рекомендация: Проверить `Application.cs` до кодирования. Добавить делегирующие свойства в Application если нет.

2. **Node.js availability для TypeScript rebuild**
   - Что знаем: В `Editor~/Server/dist/` уже есть собранный JS (судя по `ls`)
   - Неясно: Актуален ли dist/ и нужна ли пересборка после изменений
   - Рекомендация: После изменения index.ts всегда запускать `npm run build`. Если Node.js недоступен — редактировать `dist/index.js` напрямую как fallback.

3. **OnDisable vs OnDestroy для Reset**
   - Что знаем: CONTEXT.md D-02 упоминает `OnDestroy` или `OnDisable`
   - Неясно: Какой lifecycle event надёжнее при Domain Reload = enabled (default)
   - Рекомендация: Использовать `OnDestroy` — при стандартных настройках Unity 2022.3 LTS он вызывается при каждом Stop Play Mode с Domain Reload.

---

## Project Constraints (from CLAUDE.md)

Из глобального `~/.claude/CLAUDE.md`:
- **Язык:** Всегда отвечать на русском языке, документация на русском
- **Стиль кода C#:** Всегда использовать фигурные скобки `{}` в `if/else/switch/for/while/do-while`, даже для однострочных тел
- **Расположение скобок:** По стилю окружающего кода — анализ McpUnityBridge.cs показывает K&R стиль (открывающая скобка на той же строке)

---

## Sources

### Primary (HIGH confidence)
- `Packages/com.shtl.mcp-unity/Editor/McpUnityBridge.cs` — полный анализ существующей структуры switch/case, SendJsonRaw, паттерны JSON-сериализации
- `Assets/Scripts/Application/Game.cs` — существующие публичные свойства, поле `_isRunning`, `_waveNumber`
- `Assets/Scripts/Application/ApplicationEntry.cs` — точки вызова Update(), OnDestroy()
- `.planning/phases/08-mcp-runtime/08-CONTEXT.md` — все архитектурные решения D-01..D-07
- `Packages/com.shtl.mcp-unity/Editor~/Server/src/index.ts` — существующая структура MCP tools

### Secondary (MEDIUM confidence)
- Unity docs: `#if UNITY_EDITOR` conditional compilation — стандартный паттерн, верифицировано кодовой базой проекта (используется в других местах по CONTEXT.md Specifics)
- Unity docs: `Type.GetType(assemblyQualifiedName)` — стандартный C# reflection, имя `Assembly-CSharp` стандартно для Unity runtime assembly

### Tertiary (LOW confidence)
- Поведение OnDestroy при Domain Reload Disabled — не проверялось, используется только при стандартных настройках

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — весь стек из существующего кода проекта
- Architecture: HIGH — все решения зафиксированы в CONTEXT.md D-01..D-07
- Pitfalls: HIGH — выявлены из анализа существующего кода и паттернов проекта
- Open questions: MEDIUM — требуют проверки Application.cs

**Research date:** 2026-03-29
**Valid until:** 2026-04-28 (стабильный стек, без внешних зависимостей)
