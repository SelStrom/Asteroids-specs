---
phase: quick
plan: 260328-jmk
subsystem: sprites, gameplay
tags: [bug-fix, sprites, asteroid-splitting, collision]
dependency_graph:
  requires: []
  provides:
    - spritePixelsToUnits=16 в asteroids.png.meta
    - OnAsteroidCollided фильтрует по BulletModel
  affects:
    - Assets/Media/sprites/asteroids.png.meta
    - Assets/Scripts/Application/Game.cs
tech_stack:
  added: []
  patterns:
    - pattern-match фильтрация коллизий по типу модели (hitModel is not BulletModel)
key_files:
  created: []
  modified:
    - Assets/Media/sprites/asteroids.png.meta
    - Assets/Scripts/Application/Game.cs
decisions:
  - "PPU=16 применяется к единственному sprite sheet проекта — затрагивает все спрайты"
  - "Фильтрация по BulletModel в OnAsteroidCollided исключает ложные срабатывания при столкновении корабля с астероидом"
metrics:
  duration: "1 мин"
  completed_date: "2026-03-28"
  tasks_completed: 2
  tasks_total: 2
  files_changed: 2
---

# Quick Task 260328-jmk: Fix sprite pixelsPerUnit to 16 and asteroid splitting bug

**Одной строкой:** Исправлено PPU спрайтов 100→16 и баг дробления астероидов — теперь только пуля дробит астероид, а не любая коллизия.

## Цель

Два исправления:
1. `spritePixelsToUnits: 100 → 16` в `asteroids.png.meta` — спрайты отображались слишком маленькими
2. Баг в `OnAsteroidCollided` — астероид дробился при столкновении с кораблём (любая коллизия запускала дробление)

## Выполненные задачи

| # | Задача | Коммит | Файлы |
|---|--------|--------|-------|
| 1 | Исправить pixelsPerUnit в asteroids.png.meta | 45d2d71 | Assets/Media/sprites/asteroids.png.meta |
| 2 | Исправить баг дробления астероидов | b9a45ff | Assets/Scripts/Application/Game.cs |

## Задача 1: PPU 100→16

Файл `Assets/Media/sprites/asteroids.png.meta`, строка 87:
- До: `spritePixelsToUnits: 100`
- После: `spritePixelsToUnits: 16`

Единственный sprite sheet проекта — затрагивает все спрайты: asteroid_big, asteroid_medium, ship, bullet, ufo_big.

## Задача 2: Фильтрация коллизий астероида

В методе `OnAsteroidCollided` в `Game.cs` добавлена проверка типа объекта столкновения:

```csharp
var hitModel = _catalog.GetModelByGo(col.gameObject);
if (hitModel is not BulletModel) { return; }
```

**Проблема:** `OnAsteroidCollided` вызывался при ЛЮБОЙ коллизии астероида. При столкновении корабля с астероидом срабатывали оба коллбэка — `OnShipCollided` и `OnAsteroidCollided`. Это приводило к начислению очков и дроблению при столкновении с кораблём.

**Решение:** Используем существующий метод `EntitiesCatalog.GetModelByGo(col.gameObject)` для получения модели объекта коллизии. Если это не `BulletModel` — выходим без действий.

**Цепочка дробления:** пуля попадает → `OnAsteroidCollided` → `asteroid.Kill()` → `model.OnEntityDestroyed` → `OnEntityDestroyed` → `SpawnFragments` → создание 2 дочерних астероидов.

## Deviations from Plan

None — план выполнен точно как написан.

## Known Stubs

None.

## Self-Check: PASSED

- FOUND: Assets/Media/sprites/asteroids.png.meta
- FOUND: Assets/Scripts/Application/Game.cs
- FOUND commit 45d2d71: fix(quick-260328-jmk): spritePixelsToUnits 100→16 в asteroids.png.meta
- FOUND commit b9a45ff: fix(quick-260328-jmk): фильтровать коллизии астероида — дробить только при попадании пули
