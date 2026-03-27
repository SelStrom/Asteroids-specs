---
phase: 03-core-mechanics
plan: 02
subsystem: gameplay
tags: [ecs, unity, csharp, input-system, ship, bullet, asteroid, ufo]

# Dependency graph
requires:
  - phase: 03-01
    provides: "Компоненты ECS (MoveComponent, GunComponent и др.), интерфейсы IGameEntityModel/IGroupVisitor, BaseModelSystem, Model.cs, ActionScheduler"

provides:
  - "ShipModel с полным набором компонентов Move/Rotate/Thrust/Gun/Laser, методами Setup(GameData)/Reset()/Kill()"
  - "BulletModel с MoveComponent + LifeTimeComponent + IsEnemy"
  - "AsteroidModel, UfoBigModel/UfoModel — заглушки Phase 4-5"
  - "MoveSystem с wrap-around (WrapAxis), SetGameArea(Vector2)"
  - "RotateSystem — вращение направления через угол"
  - "ThrustSystem — Ньютоновская физика, Vector2.ClampMagnitude(MaxSpeed)"
  - "LifeTimeSystem — entity.Kill() при TimeLeft <= 0"
  - "GunSystem — стрельба с проверкой CurrentShoots < MaxShoots, сброс флага"
  - "LaserSystem — восстановление зарядов, сброс IsLaserFiring"
  - "MoveToSystem, ShootToSystem — заглушки Phase 4"
  - "PlayerInput — 5 событий: OnRotateAction/OnTrustAction/OnAttackAction/OnLaserAction/OnBackAction"
  - "PlayerActions — partial class структура (Generated + ручное расширение)"

affects: [03-03, phase-04-asteroids, phase-05-ufo, phase-06-vfx]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "ECS BaseModelSystem<TNode> с кортежным TNode для многокомпонентных систем (ThrustSystem)"
    - "Visitor-dispatch через AcceptWith(IGroupVisitor) для регистрации сущностей"
    - "Partial class структура для сгенерированного Input System кода"
    - "Connect()/Dispose() паттерн в PlayerInput без внедрения через конструктор"

key-files:
  created:
    - Assets/Scripts/Model/Entities/ShipModel.cs
    - Assets/Scripts/Model/Entities/BulletModel.cs
    - Assets/Scripts/Model/Entities/AsteroidModel.cs
    - Assets/Scripts/Model/Entities/UfoBigModel.cs
    - Assets/Scripts/Model/Systems/MoveSystem.cs
    - Assets/Scripts/Model/Systems/RotateSystem.cs
    - Assets/Scripts/Model/Systems/ThrustSystem.cs
    - Assets/Scripts/Model/Systems/LifeTimeSystem.cs
    - Assets/Scripts/Model/Systems/GunSystem.cs
    - Assets/Scripts/Model/Systems/LaserSystem.cs
    - Assets/Scripts/Model/Systems/MoveToSystem.cs
    - Assets/Scripts/Model/Systems/ShootToSystem.cs
    - Assets/Scripts/Input/Generated/PlayerActions.cs
    - Assets/Scripts/Input/PlayerActions.cs
    - Assets/Scripts/Input/PlayerInput.cs
  modified: []

key-decisions:
  - "MoveToSystem/ShootToSystem регистрируются как заглушки в Model.cs — компилируются, но пусты до Phase 4"
  - "UfoModel и UfoBigModel в одном файле UfoBigModel.cs (исключение из правила 'один класс — один файл')"
  - "PlayerActions/Generated — ручная реализация IInputActionCollection2 для работы без Unity Editor"
  - "ThrustSystem TNode — кортеж (ThrustComponent, MoveComponent, RotateComponent), не отдельный wrapper класс"

patterns-established:
  - "ECS TNode tuple: BaseModelSystem<(ComponentA A, ComponentB B)> для систем с несколькими компонентами"
  - "Флаги сбрасываются в конце UpdateNode: gun.Shooting = false, laser.IsLaserFiring = false"
  - "WrapAxis: wrap-around через half-size для обоих осей"

requirements-completed: [SHIP-01, SHIP-02, SHIP-03, SHIP-04, SHIP-05, SHIP-06, SHIP-07, SHIP-08, SHOT-01, SHOT-02, SHOT-03, SHOT-04]

# Metrics
duration: 2min
completed: 2026-03-27
---

# Phase 3 Plan 02: Сущности, системы ECS и Input layer Summary

**ShipModel с компонентами Move/Rotate/Thrust/Gun/Laser, 8 систем ECS включая wrap-around MoveSystem и Ньютоновский ThrustSystem, PlayerInput с 5 событиями для управления кораблём**

## Performance

- **Duration:** 2 min
- **Started:** 2026-03-27T18:43:13Z
- **Completed:** 2026-03-27T18:45:49Z
- **Tasks:** 3
- **Files modified:** 15

## Accomplishments

- ShipModel готов к полному циклу spawning/respawn: Setup(GameData), Reset(), IsInvulnerable
- 8 систем ECS: MoveSystem (wrap-around), ThrustSystem (ClampMagnitude), GunSystem (лимит MaxShoots), LaserSystem (восстановление зарядов)
- PlayerInput с 5 событиями и partial class PlayerActions для Unity Input System

## Task Commits

1. **Task 1: Сущности Model (ShipModel, BulletModel, заглушки)** — `614b394` (feat)
2. **Task 2: Системы ECS (Move, Rotate, Thrust, LifeTime, Gun, Laser + заглушки)** — `7b6a74e` (feat)
3. **Task 3: Input layer — PlayerActions partial + PlayerInput** — `bff62e8` (feat)

## Files Created/Modified

- `Assets/Scripts/Model/Entities/ShipModel.cs` — корабль: Move/Rotate/Thrust/Gun/Laser, Setup(GameData), Reset()
- `Assets/Scripts/Model/Entities/BulletModel.cs` — пуля: Move + LifeTime + IsEnemy
- `Assets/Scripts/Model/Entities/AsteroidModel.cs` — заглушка Phase 4: Move + Size
- `Assets/Scripts/Model/Entities/UfoBigModel.cs` — заглушки Phase 4-5: UfoBigModel + UfoModel в одном файле
- `Assets/Scripts/Model/Systems/MoveSystem.cs` — движение + wrap-around, SetGameArea(Vector2)
- `Assets/Scripts/Model/Systems/RotateSystem.cs` — вращение направления корабля
- `Assets/Scripts/Model/Systems/ThrustSystem.cs` — Ньютоновское ускорение, ClampMagnitude
- `Assets/Scripts/Model/Systems/LifeTimeSystem.cs` — Kill() при TimeLeft <= 0
- `Assets/Scripts/Model/Systems/GunSystem.cs` — стрельба с лимитом, сброс флага
- `Assets/Scripts/Model/Systems/LaserSystem.cs` — лазер с восстановлением зарядов
- `Assets/Scripts/Model/Systems/MoveToSystem.cs` — заглушка Phase 4
- `Assets/Scripts/Model/Systems/ShootToSystem.cs` — заглушка Phase 4
- `Assets/Scripts/Input/Generated/PlayerActions.cs` — ручная реализация IInputActionCollection2
- `Assets/Scripts/Input/PlayerActions.cs` — partial class (пустая ручная часть)
- `Assets/Scripts/Input/PlayerInput.cs` — Connect()/Dispose(), 5 событий ввода

## Decisions Made

- UfoModel и UfoBigModel в одном файле UfoBigModel.cs — явно указано в плане, допустимое исключение из правила "один класс — один файл"
- PlayerActions/Generated реализован вручную (ручная имплементация IInputActionCollection2) — Unity Editor не доступен в среде выполнения
- ThrustSystem TNode — кортеж `(ThrustComponent, MoveComponent, RotateComponent)` без дополнительного wrapper класса, как указано в ARCHITECTURE.md

## Deviations from Plan

Нет — план выполнен точно по спецификации.

## Issues Encountered

Нет.

## User Setup Required

Нет — внешние сервисы не требуются. Опционально: если Unity Editor доступен, можно запустить Generate C# Class для PlayerActions.inputactions и заменить ручную реализацию Generated/PlayerActions.cs сгенерированным кодом.

## Next Phase Readiness

- Вся игровая логика корабля готова: движение, вращение, тяга, стрельба, лазер, wrap-around
- Model.cs уже зарегистрировала все 8 систем (из Plan 01) — Plan 02 добавил реализации
- Plan 03 (View layer) может использовать ShipModel.Move.Position, Ship.Rotate, ShipModel.Laser.CurrentShoots через ObservableValue
- Phase 4 (астероиды) использует AsteroidModel и UfoBigModel заглушки

---
*Phase: 03-core-mechanics*
*Completed: 2026-03-27*
