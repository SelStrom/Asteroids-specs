---
phase: 06-audio-visual-polish
plan: "04"
subsystem: audio
tags: [unity, csharp, audio, callbacks, wire-up, vfx]

# Dependency graph
requires:
  - phase: 06-audio-visual-polish
    plan: "02"
    provides: AudioManager MonoBehaviour с PlayShoot/PlayThrust/PlayExplodeShip/PlayExplodeAsteroid/SetUfoTone/StartBeat/StopBeat/SetAsteroidCount/StopAll
  - phase: 06-audio-visual-polish
    plan: "01"
    provides: EntitiesCatalog.SpawnEffect(position, scale) для VFX взрывов
  - phase: 06-audio-visual-polish
    plan: "03"
    provides: LeaderboardScreen/LeaderboardView stub для подключения в Application

provides:
  - Game.cs: 6 audio callback полей + вызовы на всех игровых событиях (выстрел, тяга, взрывы, UFO)
  - Game.cs: SpawnEffect при уничтожении корабля (scale=1.5) и астероида (scale=size*0.5)
  - Application.cs: AudioManager lifecycle (StartBeat при старте, StopBeat+StopAll при GameOver, StopAll при Restart)
  - Application.cs: LeaderboardScreen подключён с OnLeaderboardBack
  - ApplicationEntry.cs: SerializedField AudioManager, LeaderboardView, GameObject leaderboardGo
  - GameData.cs: поле AudioData Audio

affects:
  - 06-05-setup (Phase6Setup должен назначить AudioManager в ApplicationEntry)
  - 07-leaderboard-integration (LeaderboardScreen подключён и готов)

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Audio callback wire-up через опциональные Action параметры в Game.Connect() — нулевая зависимость между Game и AudioManager"
    - "AudioManager lifecycle в Application: StopAll() при GameOver и Restart, StartBeat() при старте"
    - "SpawnEffect вызывается в OnEntityDestroyed до Kill() — позиция захватывается до возврата в пул"

key-files:
  created: []
  modified:
    - Assets/Scripts/Application/Game.cs
    - Assets/Scripts/Application/Application.cs
    - Assets/Scripts/Application/ApplicationEntry.cs
    - Assets/Scripts/Configs/GameData.cs

key-decisions:
  - "Audio callbacks передаются в Game.Connect() как опциональные Action параметры — Game не зависит от AudioManager напрямую"
  - "StopAll() вызывается и в OnGameOver и в OnPlayAgain — защита от зависших loop-звуков (thrust, UFO tone)"
  - "SpawnEffect(shipPos, 1.5f) при Kill корабля — позиция берётся до Release(model) чтобы не потерять значение"
  - "_onAsteroidCount передаётся со значением _asteroidCount-1 до декремента, чтобы AudioManager получил правильное число"

patterns-established:
  - "Callback injection pattern: опциональные Action параметры в Connect() обеспечивают decoupling между Game и внешними системами"
  - "LeaderboardScreen подключается в Application.Connect() по паттерну GameOverScreen"

requirements-completed: [VIS-01, AUD-01, AUD-02, AUD-03, AUD-04, AUD-05, AUD-06]

# Metrics
duration: 8min
completed: 2026-03-28
---

# Phase 06 Plan 04: AudioManager Wire-up Summary

**Audio callback injection через Game.Connect() и AudioManager lifecycle в Application: StartBeat/StopAll/StopBeat, SpawnEffect при уничтожении корабля и астероидов**

## Performance

- **Duration:** ~8 min
- **Started:** 2026-03-28T21:30:54Z
- **Completed:** 2026-03-28T21:39:00Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments

- Game.cs расширен 6 audio callback полями (_onThrust, _onShoot, _onExplodeShip, _onExplodeAsteroid, _onUfoTone, _onAsteroidCount) — все вызываются на правильных игровых событиях
- SpawnEffect вызывается при уничтожении корабля (scale=1.5) и астероида (scale=size*0.5)
- Application.cs управляет AudioManager lifecycle: StartBeat() при старте, StopBeat()+StopAll() при GameOver, StopAll() при Restart
- LeaderboardScreen подключён в Application.Connect() с OnLeaderboardBack callback
- ApplicationEntry.cs получил SerializedField для AudioManager, LeaderboardView и leaderboardGo

## Task Commits

1. **Task 1: Добавить AudioData в GameData и audio callbacks в Game.cs** — `43b06c8` (feat)
2. **Task 2: Подключить AudioManager в Application.cs и ApplicationEntry.cs** — `b046bb5` (feat)

**Plan metadata:** _(docs commit следует)_

## Files Created/Modified

- `Assets/Scripts/Configs/GameData.cs` — добавлено поле `AudioData Audio`
- `Assets/Scripts/Application/Game.cs` — 6 audio callback полей, расширен Connect(), вызовы во всех игровых событиях, SpawnEffect
- `Assets/Scripts/Application/Application.cs` — _audioManager поле, расширен Connect(), audio lifecycle в OnGameStart/OnGameOver/OnPlayAgain, LeaderboardScreen
- `Assets/Scripts/Application/ApplicationEntry.cs` — SerializedField AudioManager, LeaderboardView, leaderboardGo; обновлён вызов Connect()

## Decisions Made

- Audio callbacks передаются как опциональные Action параметры в Game.Connect() — Game не импортирует AudioManager, полная развязка слоёв
- StopAll() вызывается как в OnGameOver так и в OnPlayAgain — защита от зависших loop-звуков (тяга, UFO тон) при любом сценарии завершения
- _onAsteroidCount передаётся со значением (_asteroidCount - 1) до декремента переменной — AudioManager получает корректное число
- SpawnEffect позиция корабля захватывается через `_ship != null ? _ship.Move.Position.Value : Vector2.zero` до Release(model)

## Deviations from Plan

None — план выполнен точно как написан.

## Issues Encountered

MCP compile недоступен в данном execution environment. Код написан строго по существующим паттернам проекта; компиляция будет проверена Unity автоматически.

## Known Stubs

None. Все audio callbacks подключены; AudioManager будет назначен через ApplicationEntry Inspector в Phase 06 Plan 05 (Phase6Setup).

## User Setup Required

None — wire-up полностью кодовый. AudioManager GameObject и SerializedField назначения выполняются Phase6Setup.cs в Plan 05.

## Next Phase Readiness

- Game.cs и Application.cs готовы к аудио воспроизведению
- ApplicationEntry.cs ждёт назначения _audioManager в Inspector (через Phase6Setup)
- LeaderboardScreen готов к подключению в Phase 7
- Plan 05 (Phase6Setup) создаст AudioManager GameObject и назначит все SerializedField

---
*Phase: 06-audio-visual-polish*
*Completed: 2026-03-28*
