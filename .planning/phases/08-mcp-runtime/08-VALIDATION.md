---
phase: 08
slug: mcp-runtime
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-29
---

# Phase 08 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | Ручное тестирование в Unity Editor (нет авто-тестов для Editor bridge) |
| **Config file** | нет |
| **Quick run command** | `compile` MCP tool после каждого изменения C# |
| **Full suite command** | Enter Play Mode → вызвать `get_game_state` через MCP → проверить JSON |
| **Estimated runtime** | ~30 секунд |

---

## Sampling Rate

- **After every task commit:** Run `compile` MCP tool — проверить отсутствие ошибок компиляции
- **After every plan wave:** Enter Play Mode → вызвать `get_game_state` → проверить JSON
- **Before `/gsd:verify-work`:** Все 3 success criteria выполнены
- **Max feedback latency:** 30 секунд

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 08-01-01 | 01 | 1 | MCP-11 | compile | `compile` MCP tool | ✅ | ⬜ pending |
| 08-02-01 | 02 | 1 | MCP-07 | smoke/manual | MCP tool вызов в Play Mode | ❌ W0 | ⬜ pending |
| 08-02-02 | 02 | 1 | MCP-07 | smoke/manual | MCP tool вызов вне Play Mode | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] Ручная проверка: Enter Play Mode → вызов `get_game_state` → `{ score, wave, lives, isPlaying: true }`
- [ ] Ручная проверка: вне Play Mode → вызов `get_game_state` → `{ isPlaying: false }`
- [ ] TypeScript rebuild: `npm run build` в `Packages/com.shtl.mcp-unity/Editor~/Server/` после изменения index.ts

*Нет автоматизированных тестов для Editor bridge — тестирование только через MCP вручную.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| `get_game_state` возвращает корректные данные в Play Mode | MCP-07 | Editor bridge не тестируется unit-тестами | 1. Запустить Play Mode. 2. Вызвать `get_game_state`. 3. Проверить `{ score: 0, wave: 1, lives: 3, isPlaying: true }` |
| `get_game_state` возвращает `isPlaying: false` вне Play Mode | MCP-07 | Требует Editor state | 1. Остановить Play Mode. 2. Вызвать `get_game_state`. 3. Проверить `{ isPlaying: false }` |
| `RuntimeBridgeProxy.cs` компилируется без ошибок | MCP-11 | WebGL-билд требует ручной сборки | `compile` MCP tool после добавления файла |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
