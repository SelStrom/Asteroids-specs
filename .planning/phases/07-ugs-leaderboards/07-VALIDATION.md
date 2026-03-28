---
phase: 7
slug: ugs-leaderboards
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-29
---

# Phase 7 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | Unity Test Framework 1.1.33 (EditMode/PlayMode) |
| **Config file** | none — через Unity Test Runner |
| **Quick run command** | Unity Test Runner → EditMode |
| **Full suite command** | Unity Test Runner → All Tests |
| **Estimated runtime** | ~manual (UGS требует реальный сервис) |

---

## Sampling Rate

- **After every task commit:** Compile → Play → Stop (проверить Console на ошибки)
- **After every plan wave:** Запустить игру, проверить соответствующий функционал вручную
- **Before `/gsd:verify-work`:** Все 6 Manual Checks должны быть выполнены
- **Max feedback latency:** Manual PlayMode — сразу после коммита задачи

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 07-xx-01 | UgsService | 1 | LEAD-01 | Manual | Compile only | ✅ | ⬜ pending |
| 07-xx-02 | GameOverScreen | 1 | LEAD-02 | Manual | Compile only | ✅ | ⬜ pending |
| 07-xx-03 | LeaderboardView | 1 | LEAD-03, LEAD-04 | Manual | Compile only | ✅ | ⬜ pending |
| 07-xx-04 | Application integration | 2 | LEAD-01..05 | Manual PlayMode | — | ✅ | ⬜ pending |
| 07-xx-05 | GameData + Phase7Setup | 1 | LEAD-06 | Manual EditMode | — | ✅ | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

Нет — автоматизированные тесты не применимы (UGS требует реальный сервис). Существующая инфраструктура компиляции покрывает все требования фазы.

*Existing infrastructure covers all phase requirements through compilation checks + manual PlayMode verification.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Guest sign-in при запуске, повторный — использует кэш | LEAD-01 | UGS SDK требует реальный сетевой запрос и Dashboard project | Запустить игру → проверить Console (нет auth ошибки); перезапустить → нет повторного sign-in |
| Submit Score отправляет счёт в UGS | LEAD-02 | Требует реальный Leaderboard в Dashboard | Game Over → ввести имя → нажать Submit → открыть Dashboard → проверить запись |
| Top-10 отображается на экране лидерборда | LEAD-03 | Требует реальные данные из UGS | Нажать Leaderboard → убедиться что 10 строк отображаются |
| Позиция игрока видна под разделителем | LEAD-04 | Требует реальный счёт отправленный в UGS | Отправить счёт → открыть лидерборд → найти строку текущего игрока под чертой |
| Ошибка сети → errorText, нет краша | LEAD-05 | Требует реальную сетевую ошибку | Отключить сеть → открыть лидерборд → увидеть errorText, нет краша |
| Project ID и LeaderboardId в ScriptableObject | LEAD-06 | Inspector-only проверка | Открыть GameData.asset в Inspector → проверить поля UgsProjectId и LeaderboardId |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: compile after every task, manual PlayMode after wave completion
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency: compile = immediate; PlayMode = per wave
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
