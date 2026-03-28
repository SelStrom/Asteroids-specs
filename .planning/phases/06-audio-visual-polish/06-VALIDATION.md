---
phase: 6
slug: audio-visual-polish
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-28
---

# Phase 6 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | Unity Play Mode (MCP mcp__mcp-unity__compile + mcp__mcp-unity__play) |
| **Config file** | none — Unity Editor runtime |
| **Quick run command** | `mcp__mcp-unity__compile` |
| **Full suite command** | compile → play → stop → verify log |
| **Estimated runtime** | ~30 seconds |

---

## Sampling Rate

- **After every task commit:** Run `mcp__mcp-unity__compile`
- **After every plan wave:** Run compile + play + stop + check log
- **Before `/gsd:verify-work`:** Full play mode verification must pass
- **Max feedback latency:** 60 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 06-01-01 | 01 | 1 | VIS-01 | compile | `mcp__mcp-unity__compile` | ❌ W0 | ⬜ pending |
| 06-01-02 | 01 | 1 | VIS-03 | compile | `mcp__mcp-unity__compile` | ❌ W0 | ⬜ pending |
| 06-02-01 | 02 | 1 | AUD-01 | compile | `mcp__mcp-unity__compile` | ❌ W0 | ⬜ pending |
| 06-02-02 | 02 | 2 | AUD-05 | compile | `mcp__mcp-unity__compile` | ❌ W0 | ⬜ pending |
| 06-03-01 | 03 | 2 | VIS-06 | compile | `mcp__mcp-unity__compile` | ❌ W0 | ⬜ pending |
| 06-03-02 | 03 | 2 | AUD-07 | compile | `mcp__mcp-unity__compile` | ❌ W0 | ⬜ pending |
| 06-04-01 | 04 | 3 | VIS-07 | compile | `mcp__mcp-unity__compile` | ❌ W0 | ⬜ pending |
| 06-04-02 | 04 | 3 | VIS-08 | compile | `mcp__mcp-unity__compile` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `Assets/Scripts/View/EffectVisual.cs` — particle effect implementation (currently stub)
- [ ] `Assets/Scripts/Application/AudioManager.cs` — new AudioManager MonoBehaviour
- [ ] `Assets/Editor/Phase6Setup.cs` — Editor setup script

*Existing infrastructure (GameData, EntitiesCatalog, Phase setup pattern) covers the rest.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Взрыв корабля 4+ фреймов | VIS-03 | Требует визуального осмотра | Уничтожить корабль, наблюдать анимацию взрыва |
| Взрывы астероидов масштабируются | VIS-04 | Требует визуального осмотра | Уничтожить астероиды разных размеров |
| Звук выстрела/тяги без задержек | AUD-01/02 | Требует аудио-проверки | Стрелять и использовать тягу, слушать звуки |
| Фоновый пульс ускоряется | AUD-07 | Требует аудио-проверки | Уничтожить несколько астероидов, слушать пульс |
| UI экраны — полноценный layout | VIS-07/08 | Требует визуального осмотра | Проверить TitleScreen, Game Over, Leaderboard |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 60s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
