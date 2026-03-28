---
phase: quick
plan: 260328-me1
subsystem: visual/bullet-asteroid
tags: [bug-fix, sprite, bullet, asteroid, visual]
tech-stack:
  added: []
  patterns: [ReactiveValue binding, SerializedObject editor setup]
key-files:
  created: []
  modified:
    - Assets/Scripts/Configs/GameData.cs
    - Assets/Scripts/View/BulletVisual.cs
    - Assets/Scripts/Application/EntitiesCatalog.cs
    - Assets/Scripts/View/AsteroidVisual.cs
    - Assets/Media/configs/GameData.asset
    - Assets/Editor/Phase4Setup.cs
decisions:
  - "BulletSprite назначается через vm.Sprite.Value перед view.Connect — паттерн идентичен AsteroidVisual"
  - "AsteroidVisual.OnConnected fallback дублирует Awake для защиты от реиспользования через пул"
  - "Phase4Setup.CreateOrUpdateAsteroidPrefab: _spriteRenderer назначается сериализованно чтобы prefab содержал ссылку до рантайма"
metrics:
  duration: ~5 min
  completed: 2026-03-28
---

# Quick Task 260328-me1: Fix bullet sprite missing + small asteroids invisible

**Один абзац**: Добавлена поддержка спрайта пули через `BulletData.BulletSprite` / `BulletViewModel.Sprite` / `BulletVisual` биндинг — паттерн идентичен AsteroidVisual. Добавлен защитный fallback `GetComponent<SpriteRenderer>` в `AsteroidVisual.OnConnected` и назначение `_spriteRenderer` через SerializedObject в `Phase4Setup.CreateOrUpdateAsteroidPrefab`.

## Что было сделано

### Задача 1: Отображение пули

**Проблема**: `BulletVisual.OnConnected()` биндил только `Position`. У `BulletViewModel` не было поля `Sprite`. В `bullet.prefab` SpriteRenderer оставался пустым.

**Решение**:
1. `GameData.cs` — добавлено `public Sprite BulletSprite` в `BulletData` struct
2. `BulletVisual.cs` — добавлено `ReactiveValue<Sprite> Sprite = new()` в `BulletViewModel`; добавлены `[SerializeField] SpriteRenderer _spriteRenderer`, `Awake` guard, биндинг `Sprite` в `OnConnected`
3. `EntitiesCatalog.CreateBullet` — `vm.Sprite.Value = _configs.Bullet.BulletSprite` перед `view.Connect(vm)`
4. `GameData.asset` — добавлена YAML ссылка `BulletSprite: {fileID: 11, guid: cf467e92e508b5878eb24c5a126421e0, type: 3}`
5. `Phase4Setup.UpdateGameData` — автоназначение BulletSprite через `SerializedObject` если не задан

### Задача 2: Отображение small астероидов

**Проблема**: `AsteroidVisual._spriteRenderer` мог быть null при реиспользовании объектов пула (Awake не гарантирован).

**Решение**:
1. `AsteroidVisual.OnConnected` — добавлен fallback `if (_spriteRenderer == null) { _spriteRenderer = GetComponent<SpriteRenderer>(); }`
2. `Phase4Setup.CreateOrUpdateAsteroidPrefab` — после `AddComponent<AsteroidVisual>()` назначается `_spriteRenderer` через `SerializedObject` чтобы prefab содержал правильную ссылку до рантайма
3. `EntitiesCatalog.CreateAsteroid` — warning лог `[EntitiesCatalog] AsteroidData размера {size} не имеет SpriteVariants!` если sprite == null

## Изменённые файлы

| Файл | Изменение |
|------|-----------|
| `Assets/Scripts/Configs/GameData.cs` | `BulletData.BulletSprite Sprite` поле |
| `Assets/Scripts/View/BulletVisual.cs` | `BulletViewModel.Sprite` + `_spriteRenderer` + `Awake` + биндинг |
| `Assets/Scripts/Application/EntitiesCatalog.cs` | `vm.Sprite.Value` в CreateBullet + warning лог в CreateAsteroid |
| `Assets/Scripts/View/AsteroidVisual.cs` | Fallback `GetComponent<SpriteRenderer>` в `OnConnected` |
| `Assets/Media/configs/GameData.asset` | Ссылка `BulletSprite` fileID=11 |
| `Assets/Editor/Phase4Setup.cs` | BulletSprite автоназначение + _spriteRenderer через SerializedObject |

## Коммиты

| # | Hash | Описание |
|---|------|----------|
| 1 | `9f60353` | feat(quick-260328-me1): добавить BulletSprite в BulletData/BulletViewModel/BulletVisual |
| 2 | `a06b585` | fix(quick-260328-me1): защитный fallback _spriteRenderer в AsteroidVisual + Phase4Setup + warning лог |

## Deviations from Plan

None — план выполнен точно.

## Self-Check: PASSED
