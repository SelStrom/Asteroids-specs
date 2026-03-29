---
phase: 08-mcp-runtime
created: 2026-03-29
status: ready
---

# Phase 08: MCP Runtime — Context

## Domain

Добавить MCP tool `get_game_state`, который возвращает `{ score, wave, lives, isPlaying }` из живого Runtime.
`RuntimeBridgeProxy.cs` — статический буфер с `#if UNITY_EDITOR` guard: пишет состояние из Runtime,
читается Editor bridge. В WebGL-билде компилируется в no-op заглушки.

## Canonical refs

- `Packages/com.shtl.mcp-unity/Editor/McpUnityBridge.cs` — HTTP bridge, switch/case по path
- `Assets/Scripts/Application/Game.cs` — источник Score, Lives, _waveNumber, _isRunning
- `Assets/Scripts/Application/ApplicationEntry.cs` — Update loop, точка вызова RuntimeBridgeProxy
- `Assets/Editor/AsteroidsEditor.asmdef` — Editor asmdef проекта
- `.planning/REQUIREMENTS.md` — MCP-07, MCP-11

## Decisions

### D-01: Семантика `isPlaying`

`isPlaying = EditorApplication.isPlaying && Game.IsRunning`

Где `Game.IsRunning` — новое публичное свойство (`public bool IsRunning => _isRunning`).
Это отличает "Play Mode открыт" от "игра реально идёт" (не title screen, не game over).

### D-02: Стратегия обновления буфера

`RuntimeBridgeProxy.UpdateState(score, wave, lives, isRunning)` вызывается каждый кадр
из `ApplicationEntry.Update()` — всегда актуально, оверхед минимален.

При выходе из Play Mode (`ApplicationEntry.OnDestroy` или `OnDisable`) — сброс в нули.

### D-03: Доступ McpUnityBridge к RuntimeBridgeProxy

McpUnityBridge читает состояние через **reflection**:
```csharp
var type = Type.GetType("SelStrom.Asteroids.RuntimeBridgeProxy, Assembly-CSharp");
var score = (int)type.GetField("Score").GetValue(null);
// ...
```

Нет прямой зависимости пакета `com.shtl.mcp-unity` от игрового кода.
Если type == null (не в Play Mode или скрипт не найден) — возвращать `isPlaying: false`, остальные поля 0.

### D-04: Расположение RuntimeBridgeProxy

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

### D-05: Endpoint в McpUnityBridge

Добавить `case "/get_game_state":` в switch `HandleRequest`.
Метод `HandleGetGameState` использует reflection (D-03), возвращает:
```json
{ "success": true, "score": 0, "wave": 1, "lives": 3, "isPlaying": false }
```

### D-06: Game.IsRunning

Добавить `public bool IsRunning => _isRunning;` в `Game.cs`.
`_isRunning` уже существует — нужно только публичное свойство.

### D-07: Обновление ApplicationEntry

В `Update()`: `RuntimeBridgeProxy.UpdateState(_game?.Score ?? 0, _game?.Wave ?? 0, _game?.Lives ?? 0, _game?.IsRunning ?? false);`
В `OnDisable()` или `OnDestroy()`: `RuntimeBridgeProxy.Reset();`

`Game.Wave` — новое свойство `public int Wave => _waveNumber;`

## Specifics

- Пакет `com.shtl.mcp-unity` кастомный — модификация McpUnityBridge.cs допустима
- `#if UNITY_EDITOR` в Runtime файле — проверенный паттерн (используется в других местах проекта)
- Reflection через `Assembly-CSharp` — стандартное имя Unity runtime assembly
- WebGL guard критичен: `HttpListener`, `Thread` недоступны в WebGL — уже решено в McpUnityBridge через `[InitializeOnLoad]` (только Editor)

## Out of scope (deferred)

- get_entity_list, get_asteroid_positions — расширения MCP (backlog)
- MCP tool для restart/shoot — управление игрой через MCP (backlog)
