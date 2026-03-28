---
status: partial
phase: 04-asteroids-progression
source: [04-VERIFICATION.md]
started: 2026-03-28T00:00:00Z
updated: 2026-03-28T00:00:00Z
---

## Current Test

[awaiting human testing]

## Tests

### 1. Счёт в HUD обновляется в реальном времени (PROG-02)
expected: Уничтожение Large даёт +1, Medium +2, Small +3 — счёт меняется немедленно
result: [pending]

### 2. Иконки жизней отображаются в HUD (D-08)
expected: При старте 3 иконки корабля; при смерти убывают; при extra-life прибавляются
result: [pending]

### 3. Game Over экран появляется после потери 3 жизней (PROG-05)
expected: При 0 жизнях — Game Over экран с финальным счётом и кнопкой Play Again
result: [pending]

### 4. Play Again сбрасывает игру и сохраняет HighScore (D-12, PROG-07)
expected: Нажать Play Again → новая игра с 0 счётом; HighScore предыдущей игры сохраняется
result: [pending]

## Summary

total: 4
passed: 0
issues: 0
pending: 4
skipped: 0
blocked: 0

## Gaps
