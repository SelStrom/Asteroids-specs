---
phase: 05-ufo-progression-polish
plan: "04"
subsystem: game-logic
tags: [ufo, wave-banner, game-loop, progression]
dependency_graph:
  requires: [05-02, 05-03]
  provides: [ufo-spawn-logic, ufo-shooting, ufo-exit-detection, wave-banner]
  affects: [Application.cs, Game.cs]
tech_stack:
  added: []
  patterns: [ActionScheduler for periodic callbacks, UfoBigModel/UfoModel hierarchy, isEnemy bullet flag]
key_files:
  created: []
  modified:
    - Assets/Scripts/Application/Game.cs
    - Assets/Scripts/Application/Application.cs
decisions:
  - "_ufoActive флаг контролирует не более одного UFO одновременно (UFO-06)"
  - "UfoModel is-check used to differentiate Small vs Big UFO score in OnEntityDestroyed"
  - "ScheduleUfoShoot uses ActionScheduler recursion for periodic Large UFO shooting"
  - "CheckUfoExit uses wrap-around detection: first cross opposite edge, then return to start edge (UFO-05)"
metrics:
  duration_minutes: 15
  completed_date: "2026-03-28"
  tasks_completed: 2
  files_changed: 2
---

# Phase 5 Plan 04: UFO Game Logic + Wave Banner Summary

Реализована полная Game-логика UFO и HUD-баннера волны: спаун/управление UFO в Game.cs, передача Update и callbacks через Application.cs.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Реализовать UFO логику в Game.cs | 2a4377a | Assets/Scripts/Application/Game.cs |
| 2 | Расширить Application.cs — _game.Update + wave banner wire-up | 9d1e3f8 | Assets/Scripts/Application/Application.cs |

## What Was Built

**Game.cs** получил полную UFO систему:
- 4 новых поля UFO состояния (`_ufoActive`, `_activeUfo`, `_ufoStartedFromLeft`, `_activeUfoWrapped`)
- 2 wave banner callback поля (`_onWaveBannerShow`, `_onWaveBannerHide`)
- Расширена сигнатура `Connect()` с двумя опциональными параметрами
- `Start()` вызывает `ScheduleUfoSpawn()` при старте игры
- `Stop()` сбрасывает UFO флаги
- `StartWave()` показывает баннер волны через `ShowWaveBanner()`
- `OnEntityDestroyed()` обрабатывает уничтожение UFO с начислением очков
- `public void Update(float dt)` — публичный update для UFO логики
- 12 новых приватных методов: `ScheduleUfoSpawn`, `TrySpawnUfo`, `SpawnUfoBig`, `SpawnUfoSmall`, `BindUfoCollision`, `ScheduleUfoShoot`, `OnUfoGunShooting`, `OnUfoSmallShoot`, `OnUfoCollided`, `CheckUfoExit`, `UpdateUfoTarget`, `ShowWaveBanner`, `HideWaveBanner`

**Application.cs** подключён:
- `OnUpdate` вызывает `_game?.Update(deltaTime)`
- `OnGameStart` передаёт `onWaveBannerShow`/`onWaveBannerHide` callbacks в `Game.Connect`

## Deviations from Plan

None — план выполнен точно как описано.

## Known Stubs

None — вся логика подключена к реальным данным. UFO спаун использует реальные конфиги (`_configs.UfoBig.Speed`, `_configs.Ufo.Speed`, `_configs.UfoBig.Score`, `_configs.Ufo.Score`).

## Self-Check

Files modified exist:
- Assets/Scripts/Application/Game.cs - FOUND
- Assets/Scripts/Application/Application.cs - FOUND

Commits:
- 2a4377a - Task 1 commit
- 9d1e3f8 - Task 2 commit
