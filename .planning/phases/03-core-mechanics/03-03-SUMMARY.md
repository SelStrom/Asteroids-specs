---
phase: 03-core-mechanics
plan: 03
subsystem: ui
tags: [unity, csharp, mvvm, ecs, view-layer, application-layer, prefabs, editor-script]

# Dependency graph
requires:
  - phase: 03-02
    provides: "ShipModel, BulletModel, MoveSystem, GunSystem, LaserSystem, PlayerInput — весь Model layer"
  - phase: 03-01
    provides: "IGameEntityModel, Model.cs, ActionScheduler, GameObjectPool, GameData configs"

provides:
  - "View layer: ShipVisual/ShipViewModel, BulletVisual/BulletViewModel, HudVisual, TitleScreenView/TitleScreenViewModel, EffectVisual, BaseVisual, IEntityView"
  - "Application layer: ApplicationEntry (MonoBehaviour), Application, Game, EntitiesCatalog, ModelFactory, ViewFactory"
  - "Screen management: AbstractScreen, TitleScreen, GameScreen"
  - "BindingToExtensions — extension-методы для ObservableValue → Transform/SpriteRenderer биндингов"
  - "Phase3Setup.cs — Editor скрипт для создания prefabs, config assets и настройки сцены"
  - "Respawn + мигание неуязвимости: 2с задержка → 15 миганий × 0.15с = 3с через ActionScheduler"

affects: [phase-04-asteroids, phase-05-ufo, phase-06-vfx, phase-07-leaderboards]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "AbstractWidgetView<TVM>.Connect(vm) с ReactiveValue.Connect(action) в OnConnected() — одна подписка на поле"
    - "EventBindingContext для model→vm (ObservableValue→ReactiveValue), AbstractWidgetView.Dispose() для vm→view"
    - "EntitiesCatalog: двойная очистка при Release — bind.CleanUp() (model→vm) + view.Dispose() (vm→view)"
    - "IEntityView.SetPrefabId(int) + _prefabRegistry dict — Object Pool возврат через prefab id"
    - "Phase3Setup [MenuItem] — одноразовый Editor скрипт для создания всех Unity assets"

key-files:
  created:
    - Assets/Scripts/View/Base/IEntityView.cs
    - Assets/Scripts/View/Base/BaseVisual.cs
    - Assets/Scripts/View/Bindings/BindingToExtensions.cs
    - Assets/Scripts/View/Components/GuiText.cs
    - Assets/Scripts/View/ShipVisual.cs
    - Assets/Scripts/View/BulletVisual.cs
    - Assets/Scripts/View/EffectVisual.cs
    - Assets/Scripts/View/HudVisual.cs
    - Assets/Scripts/View/TitleScreenView.cs
    - Assets/Scripts/Application/IApplicationComponent.cs
    - Assets/Scripts/Application/ApplicationEntry.cs
    - Assets/Scripts/Application/Application.cs
    - Assets/Scripts/Application/ModelFactory.cs
    - Assets/Scripts/Application/ViewFactory.cs
    - Assets/Scripts/Application/EntitiesCatalog.cs
    - Assets/Scripts/Application/Game.cs
    - Assets/Scripts/Application/Screens/AbstractScreen.cs
    - Assets/Scripts/Application/Screens/TitleScreen.cs
    - Assets/Scripts/Application/Screens/GameScreen.cs
    - Assets/Editor/Phase3Setup.cs
  modified:
    - Assets/Scripts/Model/Entities/BulletModel.cs

key-decisions:
  - "ReactiveValue<T>.Connect(action) вместо EventBindingContext для vm→view биндингов — ReactiveValue позволяет только одну подписку и это правильно для жизненного цикла View"
  - "EntitiesCatalog.Release: двойной CleanUp — bind.CleanUp() (model→vm ObservableValue подписки) + sv.Dispose()/bv.Dispose() (vm ReactiveValue callbacks)"
  - "Phase3Setup использует SerializedObject API для назначения ссылок в ApplicationEntry в сцене"
  - "BulletModel.Gun поле добавлено для связи пули с GunComponent — декремент CurrentShoots при уничтожении пули"

patterns-established:
  - "View lifecycle: Connect(vm) → OnConnected() [ReactiveValue.Connect callbacks] → Dispose() [Unbind all]"
  - "EntitiesCatalog паттерн: model+view+bind в трёх Dictionary, Release чистит все три + возвращает GO в пул"

requirements-completed: [SHIP-06, SHIP-07, SHIP-08, SHOT-05, SHOT-06]

# Metrics
duration: 15min
completed: 2026-03-27
---

# Phase 3 Plan 03: View layer + Application layer — финальная сборка Summary

**View/MVVM + Application layers: 19 C# классов соединяют Model ECS с Unity — TitleScreen, HUD, ShipVisual с миганием неуязвимости, Editor скрипт для настройки prefabs/scene через MenuItem**

## Performance

- **Duration:** ~15 min
- **Started:** 2026-03-27T18:49:20Z
- **Completed:** 2026-03-27T18:58:00Z
- **Tasks:** 2 of 3 (Task 3 = human-verify checkpoint)
- **Files modified:** 21

## Accomplishments

- View layer: ShipVisual + ShipViewModel с тремя реактивными биндингами (Position, IsThrusting, IsVisible); BulletVisual; HudVisual с Update-loop для координат
- Application layer: ApplicationEntry (MonoBehaviour), Application (инициализация всех слоёв), Game (respawn + blink через ActionScheduler), EntitiesCatalog (фабрика+реестр с Object Pool)
- Phase3Setup.cs Editor скрипт: одна команда Menu создаёт все prefabs (ship/bullet/bullet_enemy), config assets (GameData/UserGunData/UfoGunData) и настраивает сцену (Canvas 1920x1080, TitleScreen, HUD inactive, Camera orthographic 22.5)

## Task Commits

1. **Task 1: View layer + Application layer — 19 C# файлов** — `552ec71` (feat)
2. **Task 2: Phase3Setup editor script** — `b3654b8` (feat)

## Files Created/Modified

- `Assets/Scripts/View/ShipVisual.cs` — ShipViewModel (Position/IsThrusting/IsVisible) + ShipVisual (OnCollisionEnter2D)
- `Assets/Scripts/View/BulletVisual.cs` — BulletViewModel + BulletVisual (OnCollisionEnter2D)
- `Assets/Scripts/View/HudVisual.cs` — отображение координат корабля через Update-loop
- `Assets/Scripts/View/TitleScreenView.cs` — TitleScreenViewModel + кнопка PLAY
- `Assets/Scripts/Application/EntitiesCatalog.cs` — CreateShip/CreateBullet/Release с двойным CleanUp
- `Assets/Scripts/Application/Game.cs` — respawn через 2с, мигание 15×0.15с=3с, input callbacks
- `Assets/Scripts/Application/Application.cs` — инициализация всех слоёв, TitleScreen → game start
- `Assets/Scripts/Application/ApplicationEntry.cs` — единственный MonoBehaviour, IApplicationComponent
- `Assets/Scripts/Application/Screens/TitleScreen.cs` — соединяет TitleScreenView с OnGameStart
- `Assets/Editor/Phase3Setup.cs` — [MenuItem] создание prefabs/configs/scene
- `Assets/Scripts/Model/Entities/BulletModel.cs` — добавлено поле `GunComponent Gun`

## Decisions Made

- `ReactiveValue<T>.Connect(action)` используется в `OnConnected()` вместо `Bind.From(reactiveValue)` — MVVM пакет не имеет extension для `BindFrom<ReactiveValue<T>>`, только для `BindFrom<ObservableValue<T>>`. ReactiveValue поддерживает только одну подписку что идеально для View lifecycle.
- Двойной CleanUp в `EntitiesCatalog.Release`: `bind.CleanUp()` снимает ObservableValue подписки (model→vm), `view.Dispose()` снимает ReactiveValue callbacks (vm→view). Без второго шага — утечка при respawn.
- `BulletModel.Gun` поле добавлено (Rule 2 — отсутствовало в Plan 02) для корректного декремента `CurrentShoots` при уничтожении пули.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Добавлено view.Dispose() в EntitiesCatalog.Release**
- **Found during:** Task 1 (EntitiesCatalog)
- **Issue:** Plan не указывал явную очистку vm→view биндингов при Release. Без `view.Dispose()` ReactiveValue callbacks оставались подписанными на удалённый ShipViewModel — утечка памяти и потенциальный crash при respawn.
- **Fix:** `Release()` вызывает `sv.Dispose()` или `bv.Dispose()` перед возвратом GO в пул.
- **Files modified:** Assets/Scripts/Application/EntitiesCatalog.cs
- **Committed in:** 552ec71 (Task 1)

**2. [Rule 2 - Missing Critical] Добавлен EventSystem в Phase3Setup.SetupScene**
- **Found during:** Task 2 (Phase3Setup)
- **Issue:** Canvas с кнопкой PLAY требует EventSystem для обработки кликов — без него кнопка не реагирует.
- **Fix:** Создаётся `EventSystem` + `StandaloneInputModule` GameObject в сцене.
- **Files modified:** Assets/Editor/Phase3Setup.cs
- **Committed in:** b3654b8 (Task 2)

---

**Total deviations:** 2 auto-fixed (2 missing critical)
**Impact on plan:** Оба исправления необходимы для корректной работы — утечки памяти и нефункциональный UI. Нет scope creep.

## Known Stubs

- `HudVisual.Update()` отображает только координаты корабля (X/Y). Rotation, Speed, LaserShootCount, LaserReloadTime — TextMeshPro поля присутствуют но не обновляются. Эти данные будут подключены в Phase 4 когда LaserComponent будет полностью интегрирован.
- `Phase3Setup`: `GameData.Ship.Prefab`, `GameData.Ship.MainSprite`, `GameData.Ship.ThrustSprite`, `GameData.Ship.Gun` — не назначены автоматически (спрайты зависят от наличия разрезанного PNG). Требуется ручное назначение в Inspector.
- `ApplicationEntry._configs` — не назначен в сцене автоматически Phase3Setup. Требует ручного назначения через Inspector (GameData.asset).

## User Setup Required

После запуска `Asteroids → Setup Phase 3 Assets`:
1. Назначить в `GameData.asset` (Inspector): `Ship.Prefab = ship.prefab`, `Ship.MainSprite`, `Ship.ThrustSprite`, `Ship.Gun = UserGunData.asset`, `Bullet.Prefab = bullet.prefab`, `Bullet.EnemyPrefab = bullet_enemy.prefab`
2. Назначить в сцене (ApplicationEntry Inspector): `_configs = GameData.asset`
3. Назначить в `ship.prefab` (Inspector): `ShipVisual._spriteRenderer`, `_mainSprite`, `_thrustSprite`

## Next Phase Readiness

- View + Application layers полностью реализованы — Phase 4 может добавлять AsteroidVisual/UfoVisual по тому же паттерну
- EntitiesCatalog готов к расширению: добавить `CreateAsteroid()`, `CreateUfo()` методы
- Game.cs готов к получению логики столкновений с астероидами (OnEntityDestroyed уже обрабатывает BulletModel/ShipModel)

---
*Phase: 03-core-mechanics*
*Completed: 2026-03-27*
