---
phase: 3
slug: core-mechanics
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-27
---

# Phase 3 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | Unity MCP — mcp__mcp-unity__compile + mcp__mcp-unity__play |
| **Config file** | none — Unity проект компилируется через MCP |
| **Quick run command** | `mcp__mcp-unity__compile` |
| **Full suite command** | `mcp__mcp-unity__compile` + `mcp__mcp-unity__play` |
| **Estimated runtime** | ~10-30 seconds |

---

## Sampling Rate

- **After every task commit:** Run `mcp__mcp-unity__compile`
- **After every plan wave:** Run `mcp__mcp-unity__compile` + `mcp__mcp-unity__play`
- **Before `/gsd:verify-work`:** Компиляция без ошибок + игра запускается
- **Max feedback latency:** 30 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 3-01-01 | 01 | 1 | SHIP-01 | compile | `mcp__mcp-unity__compile` | ❌ W0 | ⬜ pending |
| 3-01-02 | 01 | 1 | SHIP-02 | compile | `mcp__mcp-unity__compile` | ❌ W0 | ⬜ pending |
| 3-01-03 | 01 | 2 | SHIP-03 | compile+play | `mcp__mcp-unity__play` | ❌ W0 | ⬜ pending |
| 3-01-04 | 01 | 2 | SHIP-04 | compile+play | `mcp__mcp-unity__play` | ❌ W0 | ⬜ pending |
| 3-01-05 | 01 | 3 | SHIP-05 | compile+play | `mcp__mcp-unity__play` | ❌ W0 | ⬜ pending |
| 3-01-06 | 01 | 3 | SHIP-06 | compile+play | `mcp__mcp-unity__play` | ❌ W0 | ⬜ pending |
| 3-01-07 | 01 | 3 | SHIP-07 | compile+play | `mcp__mcp-unity__play` | ❌ W0 | ⬜ pending |
| 3-01-08 | 01 | 3 | SHIP-08 | compile+play | `mcp__mcp-unity__play` | ❌ W0 | ⬜ pending |
| 3-02-01 | 02 | 1 | SHOT-01 | compile | `mcp__mcp-unity__compile` | ❌ W0 | ⬜ pending |
| 3-02-02 | 02 | 2 | SHOT-02 | compile+play | `mcp__mcp-unity__play` | ❌ W0 | ⬜ pending |
| 3-02-03 | 02 | 2 | SHOT-03 | compile+play | `mcp__mcp-unity__play` | ❌ W0 | ⬜ pending |
| 3-02-04 | 02 | 2 | SHOT-04 | compile+play | `mcp__mcp-unity__play` | ❌ W0 | ⬜ pending |
| 3-02-05 | 02 | 3 | SHOT-05 | compile+play | `mcp__mcp-unity__play` | ❌ W0 | ⬜ pending |
| 3-02-06 | 02 | 3 | SHOT-06 | compile+play | `mcp__mcp-unity__play` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] Все C# файлы Phase 3 созданы — компиляция без ошибок
- [ ] `Assets/Input/PlayerActions.inputactions` создан с Action Map `Player`
- [ ] `Assets/Media/configs/GameData.asset` создан с правильными значениями

*Wave 0 — инфраструктурные файлы, необходимые для компиляции.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Корабль вращается A/D с инерцией | SHIP-01/02 | Физика ощущается только в play | Запустить игру, нажать A/D, проверить вращение |
| Wrap-around на краях экрана | SHIP-03 | Визуальное поведение | Долететь до края, проверить телепортацию |
| Лимит пуль 5 штук | SHOT-03 | Счётчик в runtime | Нажать Space 6 раз, убедиться что 6-я пуля не создаётся |
| Мигание неуязвимости 3с | SHIP-07 | Визуальный эффект | Столкнуться с объектом, наблюдать мигание корабля |
| Индикатор сопла при W | SHIP-08 | Визуальное состояние | Зажать W, проверить что сопло видно; отпустить — скрылось |
| Лазер 3 заряда / 10с восстановление | SHOT-05/06 | Тайминг в runtime | Нажать Q 3 раза, убедиться что 4-й выстрел заблокирован |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 30s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
