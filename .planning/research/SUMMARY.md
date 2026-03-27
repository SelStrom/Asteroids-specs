# Research Summary — Asteroids WebGL + MCP Unity Package

**Дата:** 2026-03-27
**Источники:** unity-webgl-asteroids.md, mcp-unity-package.md

---

## 1. Ключевые технические решения (подтверждённые)

| Решение | Статус | Уверенность |
|---------|--------|-------------|
| Unity 2022.3 LTS + Built-in RP | Оптимальный выбор для WebGL 2D | HIGH |
| IL2CPP backend при WebGL-сборке | Обязателен, автоматически применяется Unity | HIGH |
| Корутины (`CoroutineResult<T>`) вместо `async Task` | Правильный паттерн для WebGL; уже используется в проекте | HIGH |
| SpriteAtlas V2 + прямые ссылки из ScriptableObject | Один draw call, работает без кода загрузки | HIGH |
| Kinematic Rigidbody + ручная физика | Уже реализовано; нужен `rb.MovePosition()` для синхронизации коллайдеров | HIGH |
| UGS Auth 3.6.0 + Leaderboards 2.3.3 | WebGL-совместимы, анонимный auth через localStorage | MEDIUM |
| MCP-сервер: TypeScript stdio + HTTP-мост к Unity Editor | Утверждённая архитектура; Unity API стабилен | HIGH (C#) / MEDIUM (MCP SDK) |

---

## 2. Критические WebGL-подводные камни

### Обязательно исправить до WebGL-деплоя

**Баг: UFO конфиг-подмена (EntitiesCatalog.cs:146, ВЫСОКИЙ приоритет)**
- Малый UFO получает `_configs.UfoBig` вместо `_configs.Ufo` — неверные очки и поведение.
- Исправить до любого публичного деплоя.

**Коллайдеры Kinematic + `AutoSyncTransforms: false` (СРЕДНИЙ приоритет)**
- При прямом изменении `Transform.position` коллайдеры синхронизируются только в FixedUpdate.
- Решение: использовать `Rigidbody2D.MovePosition(newPosition)` в `MoveSystem`.

**LINQ-аллокации в `Model.Update` и `ActionScheduler` (СРЕДНИЙ приоритет)**
- GC-спайки в браузере проявляются как фризы.
- Приоритет повышается для WebGL. Заменить `.Any()` / `.Where()` на цикл с ранним выходом там, где это вызывается каждый кадр.

### Настроить при первом WebGL-билде

**Компрессия и заголовки сервера**
- При Gzip/Brotli-компрессии сервер обязан отдавать `Content-Encoding` заголовок — без него браузер не декомпрессирует и Unity выдаёт `"Unable to parse Build/..."`.
- Рекомендация по хостингу: GitHub Pages / Itch.io → Disabled или Gzip; nginx с полным контролем → Brotli.

**WebGL Player Settings (чеклист)**
```
Memory Growth:         true
Initial Memory (MB):   64
Exception Support:     None (продакшн) / Explicitly Thrown Only (дебаг)
Strip Engine Code:     true
Managed Stripping:     High (UGS SDK включает встроенный link.xml)
```

### Уже безопасно в проекте

- IL2CPP: нет `Emit`, нет `System.Linq.Expressions.Compile` — безопасно.
- Threading: весь async-код через корутины — WebGL-совместимо.
- UGS SDK (`WaitUntil(() => task.IsCompleted)`) — работает в WebGL без реальных потоков.
- Sprite workflow — прямые ссылки на спрайты из SpriteAtlas; загрузка не требуется.

---

## 3. Архитектура MCP-пакета (утверждённый паттерн)

### Структура пакета

```
Packages/com.shtl.mcp-unity/
├── package.json                       # UPM manifest (unity: "2022.3")
├── Editor/                            # Только для Editor, не попадает в WebGL
│   ├── McpUnityBridge.cs              # [InitializeOnLoad] + HttpListener на порту 8765
│   └── McpUnity.Editor.asmdef        # includePlatforms: ["Editor"]
├── Runtime/                           # Компилируется во все платформы
│   ├── RuntimeBridgeProxy.cs          # Статический буфер; вся логика под #if UNITY_EDITOR
│   ├── RuntimeBridgeBootstrap.cs      # [RuntimeInitializeOnLoadMethod] — авто-спавн pusher'а
│   ├── GameStatePusher.cs             # MonoBehaviour, пишет состояние каждые 0.5с
│   └── McpUnity.Runtime.asmdef        # includePlatforms: [] (все платформы)
└── Editor~/                           # Тильда: игнорируется Unity Asset Pipeline
    └── Server/                        # TypeScript MCP-сервер
        ├── package.json               # type: "module", @modelcontextprotocol/sdk ^1.0.0
        ├── tsconfig.json              # target: ES2022, module: Node16
        └── src/
            ├── index.ts               # MCP Server + обработчики ListTools/CallTool
            └── bridge.ts              # callUnityBridge() с AbortController (таймаут 5с)
```

### Принципиальные решения

**Изоляция Editor-кода:** `HttpListener` (использует `System.Net`) недоступен в WebGL — весь bridge-код в `Editor/` с `asmdef includePlatforms: ["Editor"]`. Runtime-часть (`RuntimeBridgeProxy`) содержит `#if UNITY_EDITOR` на всей логике — в WebGL-сборке становится заглушками.

**Коммуникация Runtime ↔ Editor:** статический класс `RuntimeBridgeProxy` с lock-защищённым строковым буфером. `GameStatePusher` (MonoBehaviour) записывает состояние каждые 0.5с через `PushState()`. `McpUnityBridge` читает буфер через `GetLatestState()` при запросе `/game-state`.

**Main thread для Editor API:** `EditorApplication.isPlaying`, `AssetDatabase.*`, `EditorSceneManager.*` — только main thread. Паттерн: `EditorApplication.delayCall += () => action()` для fire-and-forget. Для команд с ожиданием результата (`/list-scenes`) — `ManualResetEventSlim` с таймаутом 10с.

**Порт:** 8765 (не конфликтует со стандартными сервисами; Unity Remote использует тот же порт, но одновременное использование маловероятно).

**Регистрация в Claude Code (`mcp.json`):**
```json
{
  "mcpServers": {
    "unity": {
      "command": "node",
      "args": ["Packages/com.shtl.mcp-unity/Editor~/Server/dist/index.js"],
      "cwd": "/path/to/unity/project"
    }
  }
}
```

### HTTP API (7 endpoint'ов)

| Endpoint | Описание | Thread |
|----------|----------|--------|
| `POST /play` | `EditorApplication.isPlaying = true` | delayCall |
| `POST /stop` | `EditorApplication.isPlaying = false` | delayCall |
| `POST /compile` | `AssetDatabase.Refresh()` | delayCall |
| `POST /list-scenes` | `AssetDatabase.FindAssets("t:Scene")` | delayCall + sync wait |
| `POST /open-scene` | `EditorSceneManager.OpenScene(path)` | delayCall |
| `POST /import-asset` | `AssetDatabase.ImportAsset(path)` | delayCall |
| `POST /game-state` | Читает буфер `RuntimeBridgeProxy` | Прямо (thread-safe) |

---

## 4. UGS — аутентификация и лидерборд

### Инициализация и Auth (один раз при старте)

```csharp
await UnityServices.InitializeAsync();  // обязательно первым
if (!AuthenticationService.Instance.IsSignedIn)
    await AuthenticationService.Instance.SignInAnonymouslyAsync();
```

**WebGL-специфика:** токен хранится в `localStorage` браузера — при повторном открытии сессия восстанавливается автоматически. В режиме incognito — новый аккаунт каждый раз.

### Лидерборд

- Отправка: `LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score, options)` — один раз в конце игры, rate limit не достигается.
- Получение Top-10: `GetScoresAsync(leaderboardId, new GetScoresOptions { Limit = 10 })`.
- CORS-проблем нет — UGS API настроен для browser-origin запросов.
- Лидерборд `"asteroids_highscores"` должен быть создан вручную в Unity Dashboard.

### Обработка ошибок в корутинах

Паттерн `WaitUntil(() => task.IsCompleted)` + проверка `task.IsFaulted` — уже реализован в проекте. Критично: `task.Exception` не пробрасывается автоматически при использовании через корутины.

### Требует верификации

- Точные rate limits (`AddPlayerScoreAsync`, `GetScoresAsync`) — проверить в dashboard.unity.com.
- Сигнатура `AddPlayerScoreAsync` в Auth SDK 3.6.0 (MEDIUM confidence — API стабилен с 2.x, но уточнить).

---

## 5. Зоны риска (требуют проверки при реализации)

| Риск | Область | Действие |
|------|---------|----------|
| `@modelcontextprotocol/sdk` версия и API | MCP TypeScript | Проверить текущую версию на npm перед `npm install` |
| UGS Auth SDK 3.6.0 сигнатуры | UGS | Сверить с официальной документацией при интеграции |
| `EditorApplication.delayCall` не тикает в фоне | MCP Bridge | Проверить поведение при Editor без фокуса; при необходимости использовать `EditorApplication.update` с очередью |
| `AssemblyReloadEvents` vs `AppDomain.DomainUnload` | MCP Bridge | Предпочесть `AssemblyReloadEvents.beforeAssemblyReload` как явный и поддерживаемый в Unity 2022 |
| WebGL-сборка с `Managed Stripping = High` | WebGL Build | Проверить, что UGS пакеты включают `link.xml`; если будут MissingMethodException — добавить кастомный `Assets/link.xml` |
| Порт 8765 занят Unity Remote | MCP Bridge | Вынести порт в конфигурируемую константу (`McpUnityBridgeConfig`) |
| Разработка в Editor vs WebGL Player Settings | WebGL Build | Держать Build Target = Standalone для итерации; переключать на WebGL только для финального билда |

---

## 6. Итоговая оценка готовности к реализации

**Игровая часть (WebGL Asteroids):** HIGH уверенность. Архитектура проекта совместима с WebGL без переписывания. Есть два критических исправления (UFO-баг, `MovePosition`) и одно среднеприоритетное (LINQ-аллокации).

**MCP-пакет:** HIGH уверенность по Unity C# части (`HttpListener`, `[InitializeOnLoad]`, `EditorApplication` API). MEDIUM по TypeScript MCP SDK — версия и API требуют верификации на npm перед реализацией.

**UGS:** MEDIUM уверенность. Базовый flow стабилен, конкретные rate limits и точные сигнатуры методов требуют проверки по официальной документации.
