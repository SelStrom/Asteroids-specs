---
phase: 08-mcp-runtime
plan: 01
subsystem: mcp-runtime-bridge
tags: [mcp, runtime, reflection, editor, csharp]
dependency_graph:
  requires: []
  provides: [get_game_state-endpoint, RuntimeBridgeProxy-buffer]
  affects: [McpUnityBridge, ApplicationEntry, Game, Application]
tech_stack:
  added: []
  patterns: [static-buffer-with-preprocessor-guard, c-sharp-reflection, stringbuilder-json]
key_files:
  created:
    - Assets/Scripts/Application/RuntimeBridgeProxy.cs
  modified:
    - Assets/Scripts/Application/Game.cs
    - Assets/Scripts/Application/Application.cs
    - Assets/Scripts/Application/ApplicationEntry.cs
    - Packages/com.shtl.mcp-unity/Editor/McpUnityBridge.cs
decisions:
  - "#if UNITY_EDITOR guard в RuntimeBridgeProxy — WebGL-safe без using UnityEditor на верхнем уровне"
  - "Reflection через Assembly-CSharp — нет compile-time зависимости пакета от игрового кода"
  - "isPlaying = EditorApplication.isPlaying && IsRunning — различает Play Mode и активную игру"
  - "StringBuilder ручная JSON-сериализация — паттерн проекта (Phase 02 decision)"
metrics:
  duration_minutes: 15
  completed_date: "2026-03-29"
  tasks_completed: 4
  tasks_total: 4
  files_created: 1
  files_modified: 4
---

# Phase 08 Plan 01: RuntimeBridgeProxy + /get_game_state Endpoint Summary

**One-liner:** RuntimeBridgeProxy статический буфер (#if UNITY_EDITOR guard) + reflection-based /get_game_state endpoint в McpUnityBridge без compile-time зависимостей.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Создать RuntimeBridgeProxy.cs | 236924d | Assets/Scripts/Application/RuntimeBridgeProxy.cs |
| 2 | Добавить IsRunning/Wave в Game.cs, делегаты в Application.cs | c346d16 | Game.cs, Application.cs |
| 3 | Обновить ApplicationEntry.cs — UpdateState в Update, Reset в OnDestroy | 15b36bc | ApplicationEntry.cs |
| 4 | Добавить /get_game_state endpoint в McpUnityBridge.cs | c7fb953 | McpUnityBridge.cs |

## What Was Built

### RuntimeBridgeProxy.cs

Новый статический класс `SelStrom.Asteroids.RuntimeBridgeProxy` в `Assets/Scripts/Application/`:
- В `#if UNITY_EDITOR` — публичные поля `Score`, `Wave`, `Lives`, `IsRunning` + методы `UpdateState()` и `Reset()`
- В `#else` — no-op заглушки для WebGL — файл компилируется без Editor API
- Без `using UnityEditor;` на верхнем уровне — не сломает WebGL-билд

### Game.cs + Application.cs

- `Game.cs`: добавлены `public bool IsRunning => _isRunning;` и `public int Wave => _waveNumber;`
- `Application.cs`: делегирующие свойства `Score/Wave/Lives/IsRunning` через `_game?.X ?? 0` с null-guard

### ApplicationEntry.cs

- `Update()`: вызов `RuntimeBridgeProxy.UpdateState(...)` каждый кадр через `_application?.X ?? 0`
- `OnDestroy()`: вызов `RuntimeBridgeProxy.Reset()` после `_application?.Dispose()` — буфер обнуляется при выходе из Play Mode

### McpUnityBridge.cs

- Новый `case "/get_game_state":` в switch `HandleRequest`
- Метод `HandleGetGameState(HttpListenerContext context)`:
  - Reflection через `System.Type.GetType("SelStrom.Asteroids.RuntimeBridgeProxy, Assembly-CSharp")`
  - Если `type == null || !EditorApplication.isPlaying` → возвращает `{success:true, isPlaying:false, score/wave/lives:0}` без исключений
  - В Play Mode читает поля Score/Wave/Lives/IsRunning через reflection
  - `isPlaying = EditorApplication.isPlaying && isRunning` (D-01: различает Play Mode от активной игры)
  - StringBuilder JSON-сериализация без сторонних библиотек (паттерн Phase 02)
  - `isPlaying ? "true" : "false"` — JSON boolean без кавычек

## Decisions Made

1. `#if UNITY_EDITOR` в Runtime файле — проверенный паттерн проекта, без compile-time зависимости от Editor API
2. Reflection через `Assembly-CSharp` — стандартное имя Unity runtime assembly, пакет com.shtl.mcp-unity не зависит от игрового кода
3. `isPlaying = EditorApplication.isPlaying && IsRunning` — семантика: игра реально запущена, не только Play Mode открыт
4. StringBuilder для JSON в HandleGetGameState — JsonUtility не поддерживает анонимные типы (решение Phase 02)
5. UpdateState каждый кадр в ApplicationEntry.Update — минимальный оверхед, максимальная актуальность данных

## Deviations from Plan

None — план выполнен точно как написано.

## Known Stubs

None — все методы реализованы полностью. WebGL no-op заглушки являются намеренными и корректными для WebGL-платформы.

## Self-Check: PASSED

- RuntimeBridgeProxy.cs: FOUND
- Game.cs: FOUND (modified)
- Application.cs: FOUND (modified)
- ApplicationEntry.cs: FOUND (modified)
- McpUnityBridge.cs: FOUND (modified)
- Commit 236924d: FOUND
- Commit c346d16: FOUND
- Commit 15b36bc: FOUND
- Commit c7fb953: FOUND
