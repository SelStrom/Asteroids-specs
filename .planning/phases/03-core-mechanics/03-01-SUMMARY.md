---
phase: 03-core-mechanics
plan: 01
subsystem: infra
tags: [unity, csharp, ecs, mvvm, input-system, scriptableobject]

requires:
  - phase: 02-mcp-basic
    provides: "com.shtl.mvvm пакет с ObservableValue<T> и ReactiveValue<T>"

provides:
  - "GameObjectPool: stack-based пул GameObject по prefab-id"
  - "GameUtils: утилита случайного спауна вне радиуса"
  - "CoroutineResult<T>: типизированный результат корутины"
  - "GameData ScriptableObject с BulletData/LaserData/ShipData structs"
  - "GunData/AsteroidData/UfoData ScriptableObjects"
  - "Model ECS ядро: компоненты, системы, ActionScheduler, IGroupVisitor"
  - "PlayerActions.inputactions: Action Map Player с 5 actions"

affects:
  - 03-02
  - 03-03
  - 03-04
  - 03-05

tech-stack:
  added: []
  patterns:
    - "ECS BaseModelSystem<TNode>: системы обрабатывают узлы по словарю entity→node"
    - "Visitor dispatch: IGroupVisitor с Visit-перегрузками, GroupCreator внутри Model"
    - "ActionScheduler: планировщик таймеров без MonoBehaviour (pure C#)"
    - "namespace Model.Components отдельно от SelStrom.Asteroids"
    - "CoroutineResult<T> вместо async/await для Unity корутин"

key-files:
  created:
    - Assets/Scripts/Utils/GameObjectPool.cs
    - Assets/Scripts/Utils/GameUtils.cs
    - Assets/Scripts/Utils/CoroutineResult.cs
    - Assets/Scripts/Configs/BaseGameEntityData.cs
    - Assets/Scripts/Configs/GameData.cs
    - Assets/Scripts/Configs/GunData.cs
    - Assets/Scripts/Configs/AsteroidData.cs
    - Assets/Scripts/Configs/UfoData.cs
    - Assets/Scripts/Model/Components/IModelComponent.cs
    - Assets/Scripts/Model/Components/MoveComponent.cs
    - Assets/Scripts/Model/Components/RotateComponent.cs
    - Assets/Scripts/Model/Components/ThrustComponent.cs
    - Assets/Scripts/Model/Components/GunComponent.cs
    - Assets/Scripts/Model/Components/LaserComponent.cs
    - Assets/Scripts/Model/Components/LifeTimeComponent.cs
    - Assets/Scripts/Model/Components/MoveToComponent.cs
    - Assets/Scripts/Model/Components/ShootToComponent.cs
    - Assets/Scripts/Model/Entities/IGameEntityModel.cs
    - Assets/Scripts/Model/Entities/IGroupVisitor.cs
    - Assets/Scripts/Model/Systems/BaseModelSystem.cs
    - Assets/Scripts/Model/ActionScheduler.cs
    - Assets/Scripts/Model/Model.cs
    - Assets/Input/PlayerActions.inputactions
  modified: []

key-decisions:
  - "BulletData.LifeTimeSeconds тип int (не float) — строго по DATA_SCHEMA Pitfall 7"
  - "Model.cs регистрирует системы в строгом порядке: Rotate→Thrust→Move→LifeTime→Gun→Laser→ShootTo→MoveTo"
  - "GroupCreator как вложенный private класс Model реализует IGroupVisitor"
  - "MoveToComponent и ShootToComponent созданы как заглушки Phase 4 (UFO)"
  - "PlayerActions.inputactions использует Action Map 'Player' (не 'PlayerControls') согласно D-10"

patterns-established:
  - "ECS: системы наследуют BaseModelSystem<TNode>, TNode — компонент или ValueTuple"
  - "Visitor: каждая IGameEntityModel реализует AcceptWith(IGroupVisitor)"
  - "Config structs: вложенные [Serializable] struct в GameData для группировки параметров"

requirements-completed:
  - SHIP-01
  - SHIP-02
  - SHIP-03
  - SHIP-04
  - SHIP-05
  - SHOT-01
  - SHOT-02
  - SHOT-03
  - SHOT-04
  - SHOT-06

duration: 15min
completed: 2026-03-27
---

# Phase 03 Plan 01: Infrastructure Summary

**ECS+MVVM инфраструктурный слой: 22 C# файла (Utils/Configs/Model), ActionScheduler без MonoBehaviour, PlayerInput Actions с 5 действиями — фундамент для всех остальных планов Phase 3**

## Performance

- **Duration:** 15 min
- **Started:** 2026-03-27T18:36:55Z
- **Completed:** 2026-03-27T18:52:00Z
- **Tasks:** 3
- **Files modified:** 23

## Accomplishments

- Создан полный инфраструктурный слой: 3 файла Utils, 5 файлов Configs, 14 файлов Model layer
- GameData ScriptableObject содержит все вложенные structs (BulletData/LaserData/ShipData) с корректными типами по DATA_SCHEMA
- ECS ядро: BaseModelSystem<TNode>, ActionScheduler, GroupCreator Visitor, Model с правильным порядком систем
- PlayerActions.inputactions с Action Map "Player" и 5 actions (Rotate/Thrust/Attack/Laser/Back)

## Task Commits

1. **Task 1: Utils + Configs C# классы** - `3c589f1` (feat)
2. **Task 2: Model core — компоненты, интерфейсы, ActionScheduler, Model.cs** - `7e166c9` (feat)
3. **Task 3: Input Actions asset** - `7959e8e` (feat)

## Files Created/Modified

- `Assets/Scripts/Utils/GameObjectPool.cs` — stack-based пул GameObject по prefab id
- `Assets/Scripts/Utils/GameUtils.cs` — GetRandomPositionOutsideRadius для спауна врагов
- `Assets/Scripts/Utils/CoroutineResult.cs` — CoroutineResult<T> для async-операций через корутины
- `Assets/Scripts/Configs/BaseGameEntityData.cs` — абстрактный ScriptableObject с Score
- `Assets/Scripts/Configs/GameData.cs` — главный конфиг с BulletData/LaserData/ShipData
- `Assets/Scripts/Configs/GunData.cs` — MaxShoots, ReloadDurationSec
- `Assets/Scripts/Configs/AsteroidData.cs` — Prefab, SpriteVariants[]
- `Assets/Scripts/Configs/UfoData.cs` — Prefab, Speed, Gun, ShootDurationSec
- `Assets/Scripts/Model/Components/` — 9 компонентов: Move/Rotate/Thrust/Gun/Laser/LifeTime/MoveTo/ShootTo + IModelComponent
- `Assets/Scripts/Model/Entities/IGameEntityModel.cs` — IsDead/Kill/AcceptWith
- `Assets/Scripts/Model/Entities/IGroupVisitor.cs` — Visit перегрузки для 4 сущностей
- `Assets/Scripts/Model/Systems/BaseModelSystem.cs` — IModelSystem + BaseModelSystem<TNode>
- `Assets/Scripts/Model/ActionScheduler.cs` — планировщик без MonoBehaviour
- `Assets/Scripts/Model/Model.cs` — реестр систем/сущностей, GroupCreator Visitor
- `Assets/Input/PlayerActions.inputactions` — Unity Input System config

## Decisions Made

- `BulletData.LifeTimeSeconds` объявлен как `int` (не `float`) строго по DATA_SCHEMA — данные в `.asset` хранятся без дробной части
- Системы-заглушки `ShootToSystem` и `MoveToSystem` зарегистрированы в Model.cs — они создаются в Plan 02, компиляция пройдёт только после его выполнения
- `MoveToComponent` и `ShootToComponent` созданы в Plan 01 как заглушки для Phase 4 (UFO), чтобы `IGroupVisitor` мог их упоминать

## Deviations from Plan

None — план выполнен точно по спецификации.

## Issues Encountered

None.

## User Setup Required

None — конфигурация внешних сервисов не требуется.

## Next Phase Readiness

- Plan 02 (ship/bullet/laser модели и системы) может начинаться немедленно — все зависимости готовы
- Plan 03, 04, 05 зависят от Plan 01 и 02 — можно выполнять параллельно после Plan 02
- Model.cs скомпилируется только после создания RotateSystem, ThrustSystem и других систем в Plan 02

---
*Phase: 03-core-mechanics*
*Completed: 2026-03-27*
