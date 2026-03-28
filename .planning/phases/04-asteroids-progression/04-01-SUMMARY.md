---
phase: 04-asteroids-progression
plan: "01"
subsystem: asteroids-ecs
tags: [model, view, mvvm, ecs, asteroids]
dependency_graph:
  requires: []
  provides: [AsteroidModel-extended, AsteroidVisual, EntitiesCatalog.CreateAsteroid, GroupCreator.Visit-asteroid]
  affects: [EntitiesCatalog, Model.GroupCreator]
tech_stack:
  added: []
  patterns: [MVVM (AbstractWidgetView + AbstractViewModel), Visitor-dispatch, EventBindingContext]
key_files:
  created:
    - Assets/Scripts/View/AsteroidVisual.cs
  modified:
    - Assets/Scripts/Model/Entities/AsteroidModel.cs
    - Assets/Scripts/Application/EntitiesCatalog.cs
    - Assets/Scripts/Model/Model.cs
decisions:
  - "AsteroidData — ScriptableObject, поэтому data == _configs.AsteroidBig — ссылочное сравнение, корректно"
  - "AngularSpeed задаётся случайно в CreateAsteroid (не в AsteroidModel) — данные конфигурации принадлежат каталогу"
  - "Dispose() вызывает base.Disconnect() для снятия биндингов ViewModel→View"
metrics:
  duration_seconds: 102
  completed_date: "2026-03-28"
  tasks_completed: 2
  files_modified: 4
---

# Phase 4 Plan 1: Asteroid ECS + View Layer Summary

AsteroidModel расширен полем AngularSpeed, создан AsteroidVisual/ViewModel по паттерну ShipVisual, EntitiesCatalog получил CreateAsteroid с биндингами и Release для AsteroidVisual, GroupCreator.Visit(AsteroidModel) подключает астероид к MoveSystem.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | AsteroidModel + AsteroidVisual | d2775f6 | AsteroidModel.cs (modified), AsteroidVisual.cs (created) |
| 2 | EntitiesCatalog.CreateAsteroid + GroupCreator | 14f3ecb | EntitiesCatalog.cs, Model.cs |

## What Was Built

### Task 1: AsteroidModel + AsteroidVisual

**AsteroidModel.cs** — добавлено поле `public float AngularSpeed { get; set; }` (угловая скорость вращения в °/с, диапазон 30–120, задаётся при спавне).

**AsteroidVisual.cs** — новый файл с двумя классами:
- `AsteroidViewModel : AbstractViewModel` — поля Position (Vector2), Sprite (Sprite), OnCollision (Action)
- `AsteroidVisual : AbstractWidgetView<AsteroidViewModel>, IEntityView` — биндинг позиции и спрайта в OnConnected(), визуальное вращение в Update() через Transform.Rotate, OnCollisionEnter2D callback, Dispose() через base.Disconnect()

### Task 2: EntitiesCatalog + Model.GroupCreator

**EntitiesCatalog.Connect** — добавлена регистрация трёх prefab-ов астероидов (AsteroidBig/Medium/Small).

**EntitiesCatalog.CreateAsteroid(AsteroidData, Vector2, Vector2)** — полный фабричный метод: создаёт AsteroidModel (Size, AngularSpeed, Move), получает AsteroidVisual из пула, выбирает случайный спрайт из SpriteVariants, создаёт AsteroidViewModel с биндингами, регистрирует в словарях.

**EntitiesCatalog.Release** — добавлена ветка `else if (view is AsteroidVisual av) { av.Dispose(); }`.

**Model.GroupCreator.Visit(AsteroidModel)** — реализован: регистрирует модель в MoveSystem через `GetSystem<MoveSystem>().Add(model, model.Move)`. Visit(UfoBigModel) оставлен пустым (Phase 5).

## Deviations from Plan

Нет — план выполнен точно как написан.

## Known Stubs

Нет — все методы полностью реализованы. AsteroidVisual получает реальные данные через биндинги. CreateAsteroid возвращает полноценную модель, готовую к регистрации через Model.AddEntity().

## Self-Check: PASSED

- Assets/Scripts/View/AsteroidVisual.cs — FOUND
- Assets/Scripts/Model/Entities/AsteroidModel.cs — FOUND (содержит AngularSpeed)
- Assets/Scripts/Application/EntitiesCatalog.cs — FOUND (содержит CreateAsteroid)
- Assets/Scripts/Model/Model.cs — FOUND (Visit реализован)
- Commit d2775f6 — Task 1
- Commit 14f3ecb — Task 2
