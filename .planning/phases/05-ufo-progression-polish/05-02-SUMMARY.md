---
phase: 05-ufo-progression-polish
plan: 02
subsystem: ui
tags: [unity, editor-setup, scriptableobject, ufo, hud, wave-banner, phase5setup]

# Dependency graph
requires:
  - phase: 04-game-systems-polish
    provides: Phase4Setup.cs паттерн Editor скриптов, HudVisual.cs базовый класс
provides:
  - Phase5Setup.cs с MenuItem "Asteroids/Setup Phase 5 Assets"
  - UFO prefabs (ufo_big, ufo_small) с UfoVisual + CircleCollider2D + Rigidbody2D Kinematic
  - UfoBigData.asset (Score=200, Speed=8, ShootDurationSec=1.5)
  - UfoSmallData.asset (Score=1000, Speed=10, ShootDurationSec=2)
  - UfoBigGunData.asset и UfoSmallGunData.asset
  - GameData.asset ссылки UfoBig/Ufo обновляются при запуске скрипта
  - HudVisual.ShowWaveBanner(int) и HideWaveBanner() методы
  - wave_banner_text в HUD сцены (скрыт по умолчанию)
affects: [05-03, 05-04, 05-ufo-progression-polish]

# Tech tracking
tech-stack:
  added: []
  patterns: [Editor ScriptableObject setup, SaveOrReplacePrefab, SerializedObject property assignment]

key-files:
  created:
    - Assets/Editor/Phase5Setup.cs
  modified:
    - Assets/Scripts/View/HudVisual.cs

key-decisions:
  - "Phase5Setup.cs следует точному паттерну Phase4Setup.cs: MenuItem, LoadSprite, SaveOrReplacePrefab"
  - "UFO prefab НЕ содержит RotateSystem — UFO движется прямолинейно без вращения спрайта"
  - "wave_banner_text добавляется в HUD как дочерний GO, скрыт по умолчанию (SetActive=false)"

patterns-established:
  - "Editor setup скрипты используют SaveOrReplacePrefab для идемпотентного создания prefabs"
  - "Null-guard pattern (_waveBannerText == null) { return; } в HudVisual методах"

requirements-completed: [UFO-01, UFO-02, PROG-06]

# Metrics
duration: 8min
completed: 2026-03-28
---

# Phase 5 Plan 02: UFO Editor Setup и HUD Wave Banner Summary

**Phase5Setup.cs Editor скрипт генерирует UFO prefabs (ufo_big/ufo_small), ScriptableObject конфиги и wave banner в HUD через MenuItem "Asteroids/Setup Phase 5 Assets"**

## Performance

- **Duration:** ~8 min
- **Started:** 2026-03-28T19:50:00Z
- **Completed:** 2026-03-28T19:58:00Z
- **Tasks:** 2
- **Files modified:** 2

## Accomplishments

- HudVisual расширен полем `_waveBannerText` и методами `ShowWaveBanner(int)`/`HideWaveBanner()`
- Phase5Setup.cs создан по точному паттерну Phase4Setup.cs с полной реализацией 5 методов
- UFO prefabs создаются идемпотентно: UfoVisual + CircleCollider2D + Rigidbody2D Kinematic, layer=8
- GameData.asset получает ссылки UfoBig и Ufo через FindProperty + ApplyModifiedProperties

## Task Commits

Каждая задача закоммичена атомарно:

1. **Task 1: HudVisual wave banner** - `7b2c1dd` (feat)
2. **Task 2: Phase5Setup.cs** - `e7d291f` (feat)

## Files Created/Modified

- `Assets/Scripts/View/HudVisual.cs` - добавлены _waveBannerText поле и ShowWaveBanner/HideWaveBanner методы
- `Assets/Editor/Phase5Setup.cs` - новый Editor setup скрипт для Phase 5 assets

## Decisions Made

- Phase5Setup.cs следует точному паттерну Phase4Setup.cs для согласованности Editor скриптов
- UFO prefab не содержит RotateSystem (в отличие от астероидов) — UFO движется без вращения спрайта
- wave_banner_text добавляется как дочерний GO в HUD, скрыт по умолчанию через SetActive(false)
- GunData конфиги вынесены в отдельные assets (UfoBigGunData, UfoSmallGunData) для независимой настройки

## Deviations from Plan

None - план выполнен точно как написан. Phase5Setup.cs создан как точная копия паттерна Phase4Setup.cs с UFO-специфичными методами.

## Issues Encountered

None

## User Setup Required

После создания файлов необходимо запустить в Unity Editor:
- Меню "Asteroids/Setup Phase 5 Assets" — создаст UFO prefabs, configs и wave banner в сцене

## Known Stubs

None — Phase5Setup.cs это Editor-only скрипт. UFO prefabs/configs будут созданы при запуске меню в Unity Editor.

## Next Phase Readiness

- HudVisual готов к использованию wave banner системой волн (Plan 03/04)
- Phase5Setup.cs создаёт все необходимые assets одним вызовом меню
- UFO система (gameplay логика) реализуется в последующих планах Phase 05

## Self-Check

- `Assets/Editor/Phase5Setup.cs` — FOUND (создан в этом плане)
- `Assets/Scripts/View/HudVisual.cs` — FOUND (модифицирован в этом плане)
- Commit `7b2c1dd` — Task 1: HudVisual
- Commit `e7d291f` — Task 2: Phase5Setup.cs

## Self-Check: PASSED

---
*Phase: 05-ufo-progression-polish*
*Completed: 2026-03-28*
