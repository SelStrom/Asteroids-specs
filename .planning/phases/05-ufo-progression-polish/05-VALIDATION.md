---
phase: 5
slug: ufo-progression-polish
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-28
---

# Phase 5 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | Unity Play Mode (MCP mcp__mcp-unity__compile + mcp__mcp-unity__play) |
| **Config file** | none — Unity Editor runtime |
| **Quick run command** | `mcp__mcp-unity__compile` then `mcp__mcp-unity__play` |
| **Full suite command** | compile → play → verify game state via MCP |
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
| 05-01-01 | 01 | 1 | UFO-01 | compile | `mcp__mcp-unity__compile` | ❌ W0 | ⬜ pending |
| 05-01-02 | 01 | 2 | UFO-02 | compile | `mcp__mcp-unity__compile` | ❌ W0 | ⬜ pending |
| 05-02-01 | 02 | 1 | UFO-03 | compile | `mcp__mcp-unity__compile` | ❌ W0 | ⬜ pending |
| 05-02-02 | 02 | 2 | UFO-04 | compile | `mcp__mcp-unity__compile` | ❌ W0 | ⬜ pending |
| 05-03-01 | 03 | 1 | UFO-05 | compile | `mcp__mcp-unity__compile` | ❌ W0 | ⬜ pending |
| 05-03-02 | 03 | 2 | UFO-06 | compile | `mcp__mcp-unity__compile` | ❌ W0 | ⬜ pending |
| 05-04-01 | 04 | 1 | PROG-06 | compile | `mcp__mcp-unity__compile` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `Assets/Scripts/GameObjects/UfoVisual.cs` — UFO visual component stub
- [ ] `Assets/Editor/Phase5Setup.cs` — Editor setup script stub

*Existing infrastructure (MoveToSystem, ShootToSystem, GunSystem, ActionScheduler) covers the rest.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Large UFO appears from random screen edge | UFO-01 | Requires visual inspection in Play Mode | Start game, observe UFO spawn edge after 25-40s |
| Small UFO targets ship accurately | UFO-02 | Requires visual inspection | Reach 10000 score, verify bullet direction toward ship |
| Wave N banner appears on HUD | PROG-06 | Requires visual inspection | Start new wave, check brief banner display |
| UFO disappears after crossing screen | UFO-05 | Requires visual inspection | Watch UFO traverse full screen |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 60s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
