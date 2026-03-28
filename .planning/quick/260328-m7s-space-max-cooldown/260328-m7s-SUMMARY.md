---
phase: quick
plan: 260328-m7s
subsystem: gun
tags: [gun, cooldown, shooting, mechanics]
dependency_graph:
  requires: []
  provides: [gun-cooldown]
  affects: [GunComponent, GunSystem]
tech_stack:
  added: []
  patterns: [обратный таймер ReloadTimer, флаг IsReloading]
key_files:
  modified:
    - Assets/Scripts/Model/Components/GunComponent.cs
    - Assets/Scripts/Model/Systems/GunSystem.cs
decisions:
  - "ReloadTimer тикает в UpdateNode каждый кадр — нет отдельного coroutine, логика полностью в ECS-системе"
  - "IsReloading — вычисляемое свойство (=> ReloadTimer > 0f), а не отдельное поле — нет рассинхронизации"
  - "После истечения таймера стрельба разблокируется только на следующем нажатии, а не автоматически в том же фрейме"
metrics:
  duration_minutes: 5
  completed_date: "2026-03-28"
  tasks_completed: 2
  files_modified: 2
---

# Quick Task 260328-m7s: Space Max Cooldown Summary

**One-liner:** Механика перезарядки пушки: таймер ReloadTimer запускается при MaxShoots пулях и сбрасывает CurrentShoots через ReloadDurationSec секунд.

## Objective

Исправить баг: после выстрела MaxShoots пуль стрельба блокировалась навсегда — кулдаун не запускался, счётчик не сбрасывался.

## Tasks Completed

| # | Task | Commit | Files |
|---|------|--------|-------|
| 1 | Добавить состояние кулдауна в GunComponent | 0a21c16 | GunComponent.cs |
| 2 | Реализовать логику кулдауна в GunSystem | a574c0a | GunSystem.cs |

## Changes

### GunComponent.cs

Добавлены два поля без изменения существующего интерфейса:
- `public float ReloadTimer` — обратный отсчёт, 0 = не перезаряжается
- `public bool IsReloading => ReloadTimer > 0f` — читаемый флаг для GunSystem

### GunSystem.UpdateNode

Новая логика:
1. `if config == null` → выйти (без изменений)
2. `if IsReloading` → тикать `ReloadTimer -= deltaTime`, при `<= 0` обнулить оба и выйти
3. `if Shooting && CurrentShoots < MaxShoots` → выстрел, при достижении MaxShoots запустить `ReloadTimer = ReloadDurationSec`
4. `Shooting = false` — всегда сбрасывается

## Deviations from Plan

None — план выполнен точно как написан.

## Success Criteria Verification

- [x] Нажатие Space выпускает одну пулю — `gun.CurrentShoots++; gun.OnShooting?.Invoke(gun)` срабатывает единожды
- [x] После MaxShoots пуль стрельба блокируется — `if IsReloading` перехватывает до проверки Shooting
- [x] Через ReloadDurationSec секунд счётчик сбрасывается — `ReloadTimer = 0; CurrentShoots = 0`
- [x] Уничтожение пули декрементирует CurrentShoots — существующий код в Game.cs не затронут

## Self-Check: PASSED

- `GunComponent.cs` содержит ReloadTimer и IsReloading — подтверждено grep
- `GunSystem.cs` содержит логику кулдауна — подтверждено grep
- Commit `0a21c16` существует — подтверждено
- Commit `a574c0a` существует — подтверждено
