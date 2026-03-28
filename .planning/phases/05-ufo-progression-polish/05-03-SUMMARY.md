---
phase: 05-ufo-progression-polish
plan: "03"
subsystem: model-ecs
tags: [ufo, ecs, systems, catalog]
dependency_graph:
  requires: [05-01]
  provides: [ufo-model-infrastructure]
  affects: [Game.cs (Plan 04), EntitiesCatalog]
tech_stack:
  added: []
  patterns: [BaseModelSystem<T>, GroupCreator Visitor, EntitiesCatalog factory]
key_files:
  created: []
  modified:
    - Assets/Scripts/Model/Systems/MoveToSystem.cs
    - Assets/Scripts/Model/Systems/ShootToSystem.cs
    - Assets/Scripts/Model/Model.cs
    - Assets/Scripts/Model/Entities/UfoBigModel.cs
    - Assets/Scripts/Application/EntitiesCatalog.cs
decisions:
  - "MoveToSystem использует direction * distance вместо Vector2.MoveTowards для более точного управления скоростью"
  - "ShootToSystem сбрасывает Timer в ShootInterval ПЕРЕД проверкой null entity — избегает пропуска первого выстрела"
  - "SetGun() добавлен в UfoBigModel как публичный метод (Gun имеет private set)"
  - "CreateUfoBig/CreateUfoSmall не устанавливают Sprite из prefab — UfoVisual.Sprite остаётся null до назначения в UfoData.asset"
metrics:
  duration_seconds: 100
  completed_date: "2026-03-28"
  tasks_completed: 2
  files_modified: 5
---

# Phase 05 Plan 03: UFO Model Infrastructure Summary

**One-liner:** MoveToSystem и ShootToSystem реализованы, GroupCreator регистрирует UFO в ECS, EntitiesCatalog создаёт/освобождает UfoBigModel и UfoModel по паттерну астероидов.

## Tasks Completed

| # | Task | Commit | Status |
|---|------|--------|--------|
| 1 | Реализовать MoveToSystem и ShootToSystem | ff25350 | Done |
| 2 | GroupCreator.Visit(UfoBigModel) + EntitiesCatalog.CreateUfoBig/CreateUfoSmall | d3fc626 | Done |

## Changes Made

### Task 1: MoveToSystem и ShootToSystem

**MoveToSystem.cs** — реализована логика движения Small UFO к Target:
- Читает `ufo.Move.Position.Value`, вычисляет direction к `moveTo.Target`
- Обновляет позицию: `currentPos + direction * distance` (где distance = Speed * deltaTime)

**ShootToSystem.cs** — реализован таймер стрельбы:
- `shootTo.Timer -= deltaTime` каждый кадр
- При Timer <= 0: сброс в ShootInterval, вызов `shootTo.OnShoot?.Invoke(ufo)`

### Task 2: GroupCreator + EntitiesCatalog

**UfoBigModel.cs** — добавлен `SetGun(GunComponent gun)` (Gun имеет `private set`)

**Model.cs** — заменена заглушка `Visit(UfoBigModel)`:
- Регистрирует в `MoveSystem` + `GunSystem` для всех UFO
- Дополнительно регистрирует Small UFO (`is UfoModel`) в `ShootToSystem` + `MoveToSystem`

**EntitiesCatalog.cs** — три изменения:
- `Connect()`: RegisterPrefab для `_configs.UfoBig.Prefab` и `_configs.Ufo.Prefab`
- `CreateUfoBig(position, direction, speed)`: создаёт UfoBigModel + UfoVisual по паттерну CreateAsteroid
- `CreateUfoSmall(position, direction, speed)`: создаёт UfoModel + UfoVisual, инициализирует ShootTo.ShootInterval/Timer из конфига
- `Release()`: добавлена ветка `else if (view is UfoVisual uv) { uv.Dispose(); }`

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical Functionality] Добавить SetGun() в UfoBigModel**
- **Found during:** Task 2
- **Issue:** UfoBigModel.Gun имеет `private set` — EntitiesCatalog не может установить GunComponent из внешнего кода
- **Fix:** Добавлен публичный метод `SetGun(GunComponent gun)` в UfoBigModel.cs
- **Files modified:** Assets/Scripts/Model/Entities/UfoBigModel.cs
- **Commit:** d3fc626

## Known Stubs

- `CreateUfoBig` не устанавливает `vm.Sprite.Value` — UfoVisual будет без спрайта до назначения спрайтов в UfoBigData.asset и привязки в Game.cs или UfoData.
  - **File:** Assets/Scripts/Application/EntitiesCatalog.cs, line ~196
  - **Reason:** UfoData не содержит поля Sprite (только Prefab). Спрайт будет взят из SpriteRenderer на prefab'е автоматически при инстанцировании из пула. UfoVisual.OnConnected слушает vm.Sprite, но если не задан — спрайт prefab'а остаётся (SpriteRenderer уже настроен на prefab'е в Unity Editor).

## Self-Check: PASSED

- FOUND: Assets/Scripts/Model/Systems/MoveToSystem.cs
- FOUND: Assets/Scripts/Model/Systems/ShootToSystem.cs
- FOUND: Assets/Scripts/Model/Model.cs
- FOUND: Assets/Scripts/Application/EntitiesCatalog.cs
- FOUND: Assets/Scripts/Model/Entities/UfoBigModel.cs
- FOUND commit ff25350: feat(05-03): реализовать MoveToSystem и ShootToSystem
- FOUND commit d3fc626: feat(05-03): GroupCreator.Visit(UfoBigModel), CreateUfoBig/CreateUfoSmall в EntitiesCatalog
