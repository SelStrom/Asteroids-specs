---
phase: 06-audio-visual-polish
plan: "02"
subsystem: audio
tags: [unity, audio, scriptableobject, monobehaviour, coroutine]

# Dependency graph
requires:
  - phase: 06-audio-visual-polish
    provides: research и паттерны аудио системы (RESEARCH.md)

provides:
  - AudioData ScriptableObject с 9 AudioClip полями (Shoot, Thrust, ExplodeShip, ExplodeAsteroidBig/Medium/Small, UfoTone, BeatLow, BeatHigh)
  - AudioManager MonoBehaviour: PlayShoot, PlayThrust, PlayExplodeShip, PlayExplodeAsteroid, SetUfoTone, StartBeat, StopBeat, SetAsteroidCount, StopAll
  - BeatLoop корутина с динамическим темпом Lerp(0.25f, 1.0f, asteroidCount/12f)

affects:
  - 06-audio-visual-polish (Plan 04 — wire-up AudioManager в сцене)

# Tech tracking
tech-stack:
  added: []
  patterns:
    - AudioManager как MonoBehaviour с SerializeField AudioSource полями, назначаемыми через Phase6Setup
    - PlayOneShot через AudioSource.PlayOneShot с null-guard на src и src.clip
    - Loop-источники (thrust, ufoTone) управляются через Play()/Stop() флаг-методами

key-files:
  created:
    - Assets/Scripts/Configs/AudioData.cs
    - Assets/Scripts/Application/AudioManager.cs
  modified: []

key-decisions:
  - "AudioManager хранит отдельный AudioSource на каждый тип звука — максимальный контроль громкости/pitch без mixer"
  - "null-guard в PlayOneShot: src == null || src.clip == null — предотвращает crash при отсутствии AudioClip"
  - "BeatLoop: интервал = Lerp(0.25f, 1.0f, asteroidCount/12f) — 12 астероидов = 1.0s, 0 астероидов = 0.25s"
  - "ApplyAudioData() вызывается из Awake() и доступна публично для Phase6Setup"

patterns-established:
  - "AudioSource per sound type: отдельный AudioSource на каждый звук в [Header(AudioSources)] секции"
  - "Loop management: Play()/Stop() с isPlaying guard вместо toggle корутин"

requirements-completed: [AUD-01, AUD-02, AUD-03, AUD-04, AUD-05, AUD-06, AUD-07]

# Metrics
duration: 5min
completed: 2026-03-28
---

# Phase 06 Plan 02: Audio Manager Summary

**AudioData ScriptableObject (9 clips) и AudioManager MonoBehaviour с PlayOneShot/loop/coroutine BeatLoop для полной аудио логики Asteroids**

## Performance

- **Duration:** 5 min
- **Started:** 2026-03-28T20:57:35Z
- **Completed:** 2026-03-28T21:02:30Z
- **Tasks:** 2
- **Files modified:** 2

## Accomplishments

- AudioData ScriptableObject создан с 9 AudioClip полями и [CreateAssetMenu(menuName = "Audio data")]
- AudioManager MonoBehaviour реализует все методы AUD-01..07: PlayShoot, PlayThrust, PlayExplodeShip, PlayExplodeAsteroid, SetUfoTone, StartBeat/StopBeat, SetAsteroidCount, StopAll
- BeatLoop корутина с динамическим интервалом Lerp(0.25f, 1.0f, asteroidCount/12f) — темп нарастает по мере уничтожения астероидов
- null-guard в PlayOneShot предотвращает crash при отсутствии AudioClip в Inspector

## Task Commits

Каждый task закоммичен атомарно:

1. **Task 1: Создать AudioData ScriptableObject** — `b42fbea` (feat)
2. **Task 2: Создать AudioManager MonoBehaviour** — `a1820aa` (feat)

**Plan metadata:** _(docs commit следует)_

## Files Created/Modified

- `Assets/Scripts/Configs/AudioData.cs` — ScriptableObject с 9 AudioClip полями, namespace SelStrom.Asteroids.Configs
- `Assets/Scripts/Application/AudioManager.cs` — Централизованный аудио менеджер, все методы AUD-01..07

## Decisions Made

- AudioManager хранит отдельный AudioSource на каждый тип звука (максимальный контроль) — не используется AudioMixer на данном этапе
- null-guard в PlayOneShot (`src == null || src.clip == null`) — предотвращает crash когда AudioClip не назначен в Inspector
- BeatLoop: интервал = Lerp(0.25f, 1.0f, asteroidCount/12f) — точно соответствует паттерну из RESEARCH.md
- ApplyAudioData() вызывается из Awake() и доступна публично для Phase6Setup (гибкость при инициализации)

## Deviations from Plan

None — план выполнен точно как написан.

## Issues Encountered

None.

## User Setup Required

None — AudioClip assets будут назначены через Phase 6 Plan 04 (Phase6Setup.cs wire-up).

## Next Phase Readiness

- AudioData.cs и AudioManager.cs компилируются и готовы к wire-up
- Plan 04 создаст Phase6Setup.cs, добавит AudioManager GameObject в сцену, назначит AudioSource компоненты и аудио файлы
- AudioClip .wav/.ogg файлы должны быть предоставлены пользователем или будут stubbed (silent) до финальной поставки

---
*Phase: 06-audio-visual-polish*
*Completed: 2026-03-28*
