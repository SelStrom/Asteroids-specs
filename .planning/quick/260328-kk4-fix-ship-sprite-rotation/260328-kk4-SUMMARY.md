---
phase: quick
plan: 260328-kk4
subsystem: ship-rotation
tags: [mvvm, observable, binding, rotation, ship]
dependency_graph:
  requires: []
  provides: [ship-sprite-rotation]
  affects: [MoveComponent, RotateSystem, MoveSystem, ThrustSystem, ShipModel, EntitiesCatalog, Game, ShipVisual]
tech_stack:
  added: []
  patterns: [ObservableValue<T>, ReactiveValue<T>, EventBindingContext.From().To()]
key_files:
  created: []
  modified:
    - Assets/Scripts/Model/Components/MoveComponent.cs
    - Assets/Scripts/Model/Systems/RotateSystem.cs
    - Assets/Scripts/Model/Systems/MoveSystem.cs
    - Assets/Scripts/Model/Systems/ThrustSystem.cs
    - Assets/Scripts/Model/Entities/ShipModel.cs
    - Assets/Scripts/Application/EntitiesCatalog.cs
    - Assets/Scripts/Application/Game.cs
    - Assets/Scripts/View/ShipVisual.cs
decisions:
  - "Direction вычитает 90° при конвертации в угол поворота (angleDeg - 90f) — спрайт корабля нарисован смотрящим вверх, Quaternion.Euler(0,0,0) = вправо"
metrics:
  duration: "~10 min"
  completed_date: "2026-03-28"
  tasks_completed: 2
  files_modified: 8
---

# Phase quick Plan 260328-kk4: Fix Ship Sprite Rotation Summary

**One-liner:** MoveComponent.Direction сделан реактивным (ObservableValue<Vector2>) с MVVM-биндингом на поворот спрайта корабля через Quaternion.Euler.

## Tasks Completed

| # | Task | Commit | Files |
|---|------|--------|-------|
| 1 | MoveComponent.Direction → ObservableValue, обновить RotateSystem/MoveSystem/ThrustSystem/ShipModel/EntitiesCatalog/Game | 714ea30 | 7 файлов |
| 2 | ShipViewModel.Rotation + ShipVisual биндинг поворота | 714ea30 | ShipVisual.cs |

## What Was Done

Спрайт корабля не поворачивался, потому что `MoveComponent.Direction` был обычным полем `Vector2` — изменения не уведомляли никого. Исправление:

1. `MoveComponent.Direction` изменён с `public Vector2 Direction` на `public ObservableValue<Vector2> Direction = new()`.

2. Все 8 файлов, читающих или пишущих `Direction`, обновлены для использования `.Value`.

3. В `EntitiesCatalog.CreateShip()` добавлен биндинг:
   `bind.From(model.Move.Direction).To(val => vm.Rotation.Value = Mathf.Atan2(val.y, val.x) * Mathf.Rad2Deg)`

4. В `ShipViewModel` добавлено поле `public ReactiveValue<float> Rotation = new()`.

5. В `ShipVisual.OnConnected()` добавлен биндинг:
   `ViewModel.Rotation.Connect(angleDeg => transform.rotation = Quaternion.Euler(0f, 0f, angleDeg - 90f))`

Вычитание 90° компенсирует то, что спрайт нарисован смотрящим вверх, тогда как `Quaternion.Euler(0,0,0)` = вправо. При `Direction = Vector2.up` → `angleDeg = 90°` → `Euler(0,0,0°)` — нейтраль спрайта.

## Deviations from Plan

None — план выполнен точно.

## Known Stubs

None — биндинг поворота полностью подключён к реальным данным.

## Self-Check: PASSED

Файлы существуют:
- Assets/Scripts/Model/Components/MoveComponent.cs: FOUND
- Assets/Scripts/View/ShipVisual.cs: FOUND

Коммит 714ea30 существует в git log.
