---
phase: 08-mcp-runtime
plan: 02
subsystem: mcp-typescript-tool
tags: [mcp, typescript, get_game_state, debug]
dependency_graph:
  requires: [08-01]
  provides: [get_game_state-mcp-tool]
  affects:
    - Packages/com.shtl.mcp-unity/Editor~/Server/src/index.ts
    - Packages/com.shtl.mcp-unity/Editor~/Server/dist/index.js
    - Packages/com.shtl.mcp-unity/Editor/McpUnityBridge.cs
tech_stack:
  added: []
  patterns: [callBridge-pattern, typescript-mcp-tool]
key_files:
  created: []
  modified:
    - Packages/com.shtl.mcp-unity/Editor~/Server/src/index.ts
    - Packages/com.shtl.mcp-unity/Editor~/Server/dist/index.js
    - Packages/com.shtl.mcp-unity/Editor/McpUnityBridge.cs
decisions:
  - "isPlaying = EditorApplication.isPlaying (не && isRunning) — различает Play Mode от игровой логики"
  - "Добавлено поле isRunning = RuntimeBridgeProxy.IsRunning — отдельный сигнал активности геймплея"
  - "FindRuntimeBridgeProxyType() итерирует AppDomain.GetAssemblies() — надёжнее чем GetType с именем сборки"
  - "Убраны #if UNITY_EDITOR guards из RuntimeBridgeProxy.cs — тип должен присутствовать в Assembly-CSharp"
metrics:
  duration_minutes: 45
  completed_date: "2026-03-29"
  tasks_completed: 2
  tasks_total: 2
  files_created: 0
  files_modified: 3
---

# Phase 08 Plan 02: TypeScript get_game_state Tool + Debug Fix Summary

**One-liner:** TypeScript MCP tool get_game_state добавлен, dist пересобран, выявлены и исправлены 3 дефекта в C# реализации. Human verify пройден.

## Tasks Completed

| Task | Name | Files |
|------|------|-------|
| 1 | Добавить get_game_state tool в index.ts и пересобрать dist | index.ts, dist/index.js |
| 2 | Human verify (выявил дефекты → debug сессия → approved) | McpUnityBridge.cs, RuntimeBridgeProxy.cs |

## What Was Built

### index.ts — MCP tool get_game_state

Добавлен `server.tool("get_game_state", ...)` по паттерну play/stop (без параметров).
TypeScript пересобран через `npm run build` без ошибок.

### Debug Session — 3 исправленных дефекта

При human verify обнаружено: get_game_state всегда возвращает `isPlaying:false` в Play Mode.

**Дефект 1:** `#if UNITY_EDITOR` guard в `RuntimeBridgeProxy.cs` скрывал поля `Score/Wave/Lives/IsRunning` от сборки `Assembly-CSharp` (рантайм). Editor-код (McpUnityBridge) не мог найти их через рефлексию.
→ Исправление: убраны все `#if UNITY_EDITOR` директивы из RuntimeBridgeProxy.cs.

**Дефект 2:** `System.Type.GetType("..., Assembly-CSharp")` — McpUnityBridge.asmdef не ссылается на сборку `Asteroids` (имя .asmdef проекта), поэтому тип никогда не находился.
→ Исправление: `FindRuntimeBridgeProxyType()` теперь итерирует `AppDomain.CurrentDomain.GetAssemblies()`.

**Дефект 3:** `isPlaying = EditorApplication.isPlaying && isRunning` — на TitleScreen `IsRunning=false`, поэтому ответ содержал `isPlaying:false` даже в Play Mode. Семантика была нечёткой.
→ Исправление: `isPlaying` = `EditorApplication.isPlaying` (Unity в Play Mode); добавлено поле `isRunning` = `RuntimeBridgeProxy.IsRunning` (геймплей активен).

## Final Response Format

```json
// В Play Mode, игра на TitleScreen:
{"success":true,"score":0,"wave":0,"lives":0,"isPlaying":true,"isRunning":false}

// В Play Mode, геймплей запущен:
{"success":true,"score":0,"wave":1,"lives":3,"isPlaying":true,"isRunning":true}

// Вне Play Mode:
{"success":true,"score":0,"wave":0,"lives":0,"isPlaying":false}
```

## Deviations from Plan

- Ожидался формат `{ score, wave, lives, isPlaying }` — фактически добавлено поле `isRunning`
- McpUnityBridge.cs не был в списке files_modified плана, но потребовал 3 исправления
- `#if UNITY_EDITOR` guard в RuntimeBridgeProxy.cs был архитектурным решением плана 08-01, оказался некорректным

## Self-Check: PASSED

- index.ts содержит `get_game_state` ✓
- dist/index.js содержит `get_game_state` ✓
- В Play Mode: isPlaying:true ✓
- Вне Play Mode: isPlaying:false ✓
- После Stop: isPlaying:false ✓
- Human verify: approved ✓
