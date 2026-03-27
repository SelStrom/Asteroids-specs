# Phase 3: Core Mechanics — Context

**Gathered:** 2026-03-27
**Status:** Ready for planning

<domain>
## Phase Boundary

Реализовать весь игровой слой с нуля: движение корабля с инерцией (Ньютоновская физика), wrap-around, стрельба с object pool, лазерное оружие (полностью по reference), respawn с иммунитетом. Вся архитектура (ApplicationEntry, Application, Game, EntitiesCatalog, Model, Systems, View, Input, Utils, Configs) создаётся в этой фазе.

**Астероиды, UFO и Game Loop — Phase 4+.** Phase 3 строит фундамент — ship + bullets + laser + инфраструктура.

</domain>

<decisions>
## Implementation Decisions

### Спрайты — без Sprite Atlas
- **D-01:** Sprite Atlas (GameAtlas.spriteAtlas) **не используется**. Он избыточен — PNG уже нарезан на sub-sprites в Unity Sprite Editor. Все prefabs ссылаются на слайсированные спрайты напрямую из `Assets/Media/sprites/` (через ссылку на конкретный sub-sprite объекта).
- **D-02:** Это отменяет решение D-04 из Phase 1 Context. `GameAtlas.spriteAtlas` можно удалить или оставить пустым.

### Лазер — реализовать полностью
- **D-03:** Лазер входит в Phase 3 наравне с обычной стрельбой. Реализовать LaserSystem, LaserComponent, LaserData.asset полностью по DATA_SCHEMA: MaxShoots=3 заряда, LaserUpdateDuration=10с (восстановление), BeamEffectLifetime=0.5с.
- **D-04:** Ключ для лазера — `OnLaserAction` из `PlayerInput` (уже предусмотрен в reference Input-слое).

### Взрыв корабля (SHIP-06/07) — логика без VFX
- **D-05:** VfxBlow (анимация взрыва) в Phase 3 **не реализуется** — откладывается в backlog до Phase 6 (Visual Polish). В Phase 3 при гибели корабля: ship GameObject скрывается, через 2 секунды корабль появляется в центре с 3-секундным миганием (`SpriteRenderer.enabled` toggle — мигание через `LifeTimeSystem` / `ActionScheduler`).
- **D-06:** `GameData.VfxBlowPrefab` — поле создаётся в `GameData.cs`, но .asset-ссылка остаётся пустой (null). Код в `Game.cs` проверяет null перед созданием VFX-объекта.

### Числовые значения — только из DATA_SCHEMA
- **D-07:** Все числовые параметры берутся из `DATA_SCHEMA.md`, **не из REQUIREMENTS.md** (там есть расхождения):
  - `UserGunData.asset` MaxShoots = **5** (не 4, как в SHOT-03)
  - `BulletData.LifeTimeSeconds` = **2** с (не 0.8, как в SHOT-04)
  - `Bullet.Speed` = **20** ед/с
  - `Ship.ThrustUnitsPerSecond` = **6** ед/с²
  - `Ship.MaxSpeed` = **15** ед/с
  - `UfoGunData.asset` MaxShoots = **1**, Reload = **2** с
  - `GameData.AsteroidInitialCount` = **10**, `SpawnAllowedRadius` = **20**, `SpawnNewEnemyDurationSec` = **25**
  - Баллы за астероиды в .asset-файлах: Big=1, Medium=2, Small=3 (а не 20/50/100 из REQUIREMENTS)

### Коллизии — OnCollisionEnter2D
- **D-08:** Collision detection через стандартный `OnCollisionEnter2D` (не Trigger), как в reference. Rigidbody2D на prefab'ах с `BodyType = Kinematic` — физика движения управляется кодом (MoveSystem), Rigidbody только для collision.

### Слои коллизий
- **D-09:** Физические слои уже настроены в Phase 1 (D-11): Layer 7=Player, Layer 8=Asteroid, Layer 9=PlayerBullet, Layer 10=EnemyBullet, Layer 11=Enemy. Collision Matrix: PlayerBullet↔Asteroid, PlayerBullet↔Enemy, EnemyBullet↔Player, Asteroid↔Player.

### Input Actions
- **D-10:** Asset: `Assets/Input/PlayerActions.inputactions`. Имя Action Map: `Player`. Actions: `Rotate` (A/D/стрелки), `Thrust` (W/стрелка вверх), `Attack` (Space/ЛКМ), `Laser` (Q или другая кнопка — как в reference), `Back` (Escape).
- **D-11:** C# generated class: `Assets/Scripts/Input/Generated/PlayerActions.cs`. Partial-класс: `Assets/Scripts/Input/PlayerActions.cs`. Обёртка: `Assets/Scripts/Input/PlayerInput.cs` (публикует C#-события: `OnAttackAction`, `OnRotateAction`, `OnTrustAction`, `OnLaserAction`, `OnBackAction`).

### Что реализуется в Phase 3 (полный scope)
- **D-12:** Весь слой Application: `ApplicationEntry.cs`, `Application.cs`, `Game.cs`, `EntitiesCatalog.cs`, `ModelFactory.cs`, `ViewFactory.cs`, `IApplicationComponent.cs`.
- **D-13:** Model layer: `Model.cs`, `ActionScheduler.cs`; все компоненты (Move, Rotate, Thrust, Gun, Laser, LifeTime, MoveTo, ShootTo, IModelComponent); сущности `ShipModel`, `BulletModel` + интерфейсы (`IGameEntityModel`, `IGroupVisitor`); системы (`MoveSystem`, `RotateSystem`, `ThrustSystem`, `GunSystem`, `LaserSystem`, `LifeTimeSystem`).
- **D-14:** View layer: `BaseVisual.cs`, `IEntityView.cs`, `BindingToExtensions.cs`, `ShipVisual.cs` (+ ShipViewModel), `BulletVisual.cs`, `EffectVisual.cs`, `HudVisual.cs` (+ HudData), `TitleScreenView.cs` (+ TitleScreenViewModel), `GuiText.cs`.
- **D-15:** Screens: `AbstractScreen.cs`, `TitleScreen.cs`, `GameScreen.cs`.
- **D-16:** Configs (C#): `BaseGameEntityData.cs`, `GameData.cs`, `GunData.cs`, `BulletData.cs`, `AsteroidData.cs`, `UfoData.cs`. Конфиги для Phase 4+ создаются как структуры, но .asset-данные Phase 4 (AsteroidBigData и т.д.) не создаются.
- **D-17:** Config assets (в `Assets/Media/configs/`): `GameData.asset`, `UserGunData.asset`, `UfoGunData.asset`. Остальные .asset файлы создаются в Phase 4.
- **D-18:** Utils: `GameObjectPool.cs`, `GameUtils.cs`, `CoroutineResult.cs`.
- **D-19:** Prefabs (в `Assets/Media/prefabs/`): `ship.prefab`, `bullet.prefab`, `bullet_enemy.prefab`. Остальные (asteroid_*, ufo_*, vfx_blow) — Phase 4+.
- **D-20:** Сцена `Assets/Scenes/Main.unity`: разместить `ApplicationEntry` на GameObject, настроить `Camera` (ортографическая, Black background), Canvas с экранами Title и Game (Game Over — Phase 4).

### Claude's Discretion
- Конкретная клавиша для лазера (Q, Shift или другая — следовать reference .inputactions если есть, иначе выбрать)
- Точный способ мигания неуязвимости (toggle SpriteRenderer.enabled каждые N мс через ActionScheduler)
- Форма коллайдеров (CircleCollider2D для ship, capsule или circle для bullet)
- Размер коллайдеров (соответствует спрайтам)
- Параметры ортографической камеры (size соответствует 1920×1080 WebGL)

</decisions>

<specifics>
## Specific Ideas

- **Точная реконструкция**: цель проекта — воспроизвести reference, поэтому DATA_SCHEMA значения имеют приоритет над REQUIREMENTS где они расходятся
- **Без атласа**: PNG нарезан → sub-sprites доступны → prefabs используют их напрямую. Проще, без лишнего слоя.
- **VfxBlowPrefab null-check**: `if (_configs.VfxBlowPrefab != null) ...` — код готов к Phase 6 без обязательной зависимости
- **Лазер сразу**: reference-кодовая база содержит полный LaserSystem, включаем чтобы не разрывать архитектуру

</specifics>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Архитектура и паттерны
- `.planning/codebase/ARCHITECTURE.md` — ECS+MVVM архитектура, поток данных, все системы, паттерны (Visitor, MVVM, ECS, Pool)
- `.planning/codebase/CONVENTIONS.md` — naming conventions, стиль кода (Allman/K&R), паттерны Connect/Dispose, логирование

### Структура файлов
- `.planning/codebase/STRUCTURE.md` — полная структура папок, имена всех файлов, куда добавлять новый код
- `.planning/codebase/STACK.md` — версии пакетов, asmdef-файлы, зависимости сборок

### Данные и конфиги
- `.planning/codebase/DATA_SCHEMA.md` — все поля GameData, BulletData, ShipData, LaserData, GunData — ЭТО ИСТОЧНИК ИСТИНЫ для числовых значений

### Требования
- `.planning/REQUIREMENTS.md` §Ship, §Shooting — SHIP-01..08, SHOT-01..06 (поведенческие требования; числа брать из DATA_SCHEMA, не отсюда)
- `.planning/ROADMAP.md` §Phase 3 — Success Criteria фазы

### Unity конфигурация
- `.planning/codebase/UNITY_CONFIG.md` — физические слои (Layer 7-11), Collision Matrix, Physics2D настройки

### Предыдущие контексты
- `.planning/phases/01-project-foundation/1-CONTEXT.md` §D-11 — физические слои
- `.planning/phases/01-project-foundation/1-CONTEXT.md` §D-06,D-07 — структура папок, asmdef файлы

### Нет внешних спеков
- Вся необходимая информация содержится в codebase maps выше

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Assets/Media/sprites/` — PNG нарезан, sub-sprites доступны напрямую (ship, bullets, asteroids, ufo и т.д.)
- `Assets/Scenes/Main.unity` — существует, пустая сцена; нужно добавить GameObjects
- `Packages/com.shtl.mcp-unity/` — работающий MCP-сервер (Phase 2), позволяет compile/play/stop из чата
- `Assets/Editor/AsteroidsEditor.asmdef` — Editor-сборка, для LeaderboardPrefabCreator

### Established Patterns
- Единственный MonoBehaviour: `ApplicationEntry.cs` (всё остальное — pure C# через `new`)
- Движение через MoveSystem → ObservableValue → BindingToExtensions → Transform.position (не Rigidbody2D)
- Collision через OnCollisionEnter2D на Visual MonoBehaviours → ViewModel.OnCollision callback → Game
- Object Pool через `GameObjectPool` (Stack-based, по prefab id)
- Конфиги: `[Serializable] struct` внутри GameData.cs для BulletData, ShipData, LaserData

### Integration Points
- `ApplicationEntry` → `Application.Connect(GameData, ...)` → создаёт Model, Game, EntitiesCatalog
- `Game.Start()` → спаунит корабль через `EntitiesCatalog.CreateShip()`; астероиды — Phase 4
- `PlayerInput` → события → `Game.OnRotate/OnThrust/OnAttack/OnLaser` → пишет в компоненты ShipModel
- `GameScreen` (Phase 3) показывает HUD; Game Over screen — Phase 4
- `ActionScheduler` — для таймера respawn (2с) и лазерного reload (10с)

</code_context>

<deferred>
## Deferred Ideas

- **VfxBlowPrefab** — визуальный взрыв корабля (ParticleSystem / sprite animation) — Phase 6 (Visual Polish)
- **Астероиды** (AsteroidModel, AsteroidData.asset, asteroid prefabs, AsteroidBigData/Medium/Small.asset) — Phase 4
- **UFO** (UfoBigModel, UfoModel, MoveToSystem, ShootToSystem, ufo prefabs) — Phase 4-5
- **Screens**: Game Over, Leaderboard — Phase 4/7
- **Leaderboard layer** (LeaderboardService, IAuthProxy, ILeaderboardProxy и т.д.) — Phase 7
- **AsteroidInitialCount/SpawnAllowedRadius/SpawnNewEnemyDurationSec** в GameData.asset — значения прописать, но логика — Phase 4

</deferred>

---

*Phase: 03-core-mechanics*
*Context gathered: 2026-03-27*
