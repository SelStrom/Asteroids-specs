# Phase 3: Core Mechanics — Research

**Researched:** 2026-03-27
**Domain:** Unity 2022.3 LTS — ECS+MVVM game architecture, ship movement, shooting, object pooling
**Confidence:** HIGH

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

- **D-01:** Sprite Atlas не используется. PNG нарезан на sub-sprites в Unity Sprite Editor. Prefabs ссылаются на слайсированные спрайты напрямую из `Assets/Media/sprites/`.
- **D-02:** `GameAtlas.spriteAtlas` можно удалить или оставить пустым.
- **D-03:** Лазер входит в Phase 3. Реализовать `LaserSystem`, `LaserComponent`, `LaserData.asset` по DATA_SCHEMA: MaxShoots=3, LaserUpdateDuration=10с, BeamEffectLifetime=0.5с.
- **D-04:** Ключ для лазера — `OnLaserAction` из `PlayerInput` (клавиша `Q`).
- **D-05:** VfxBlow (анимация взрыва) не реализуется — откладывается в Phase 6. При гибели корабля: ship GameObject скрывается, через 2 секунды появляется в центре с 3-секундным миганием (`SpriteRenderer.enabled` toggle через `LifeTimeSystem`/`ActionScheduler`).
- **D-06:** `GameData.VfxBlowPrefab` — поле создаётся, .asset-ссылка остаётся null. Код проверяет null перед созданием VFX-объекта.
- **D-07:** Все числовые параметры из DATA_SCHEMA (не из REQUIREMENTS):
  - `UserGunData.asset` MaxShoots = **5**, ReloadDurationSec = **2**
  - `BulletData.LifeTimeSeconds` = **2** с
  - `Bullet.Speed` = **20** ед/с
  - `Ship.ThrustUnitsPerSecond` = **6** ед/с²
  - `Ship.MaxSpeed` = **15** ед/с
  - `UfoGunData.asset` MaxShoots = **1**, Reload = **2** с
  - `GameData.AsteroidInitialCount` = **10**, `SpawnAllowedRadius` = **20**, `SpawnNewEnemyDurationSec` = **25**
  - Очки за астероиды: Big=1, Medium=2, Small=3
- **D-08:** Collision detection через `OnCollisionEnter2D` (не Trigger). `Rigidbody2D` с `BodyType = Kinematic`. Физика движения через `MoveSystem`, не через Rigidbody2D.
- **D-09:** Физические слои уже настроены: Layer 7=Player, Layer 8=Asteroid, Layer 9=PlayerBullet, Layer 10=EnemyBullet, Layer 11=Enemy. Collision Matrix из UNITY_CONFIG.md.
- **D-10:** Asset: `Assets/Input/PlayerActions.inputactions`. Action Map: `Player`. Actions: `Rotate` (A/D), `Thrust` (W), `Attack` (Space/ЛКМ), `Laser` (Q), `Back` (Escape).
- **D-11:** C# generated class: `Assets/Scripts/Input/Generated/PlayerActions.cs`. Partial-класс: `Assets/Scripts/Input/PlayerActions.cs`. Обёртка: `Assets/Scripts/Input/PlayerInput.cs`.
- **D-12..D-20:** Полный scope Phase 3 — всё перечислено в CONTEXT.md §D-12..D-20 (Application, Model, View, Configs, Prefabs, Scene).

### Claude's Discretion

- Конкретная клавиша для лазера — Q (следовать существующему `player_actions.inputactions`)
- Точный способ мигания неуязвимости (toggle `SpriteRenderer.enabled` каждые N мс через `ActionScheduler`)
- Форма коллайдеров (CircleCollider2D для ship, capsule или circle для bullet)
- Размер коллайдеров (соответствует спрайтам)
- Параметры ортографической камеры (size соответствует 1920×1080 WebGL)

### Deferred Ideas (OUT OF SCOPE)

- VfxBlowPrefab — визуальный взрыв корабля — Phase 6
- Астероиды (AsteroidModel, AsteroidData.asset, asteroid prefabs) — Phase 4
- UFO — Phase 4-5
- Screens: Game Over, Leaderboard — Phase 4/7
- Leaderboard layer — Phase 7
- AsteroidInitialCount/SpawnAllowedRadius/SpawnNewEnemyDurationSec в GameData.asset — значения прописать, но логика Phase 4
</user_constraints>

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| SHIP-01 | Корабль вращается влево/вправо (A/D или стрелки) с мгновенным откликом | `RotateSystem` + `RotateComponent`; `PlayerInput.OnRotateAction` → `ShipModel.Rotate` |
| SHIP-02 | Тяга (W) прибавляет скорость в направлении носа корабля (Ньютоновская физика) | `ThrustSystem` читает `ThrustComponent.IsActive` + `RotateComponent` → добавляет к скорости в `MoveComponent` |
| SHIP-03 | Без тяги корабль движется по инерции без замедления | `MoveSystem` просто прибавляет velocity к position каждый кадр; damping = 0 |
| SHIP-04 | Максимальная скорость ограничена константой | `ThrustSystem` зажимает `Vector2.ClampMagnitude(velocity, MaxSpeed)` из `GameData.ShipData.MaxSpeed=15` |
| SHIP-05 | Wrap-around по X и Y | `MoveSystem` проверяет `Model.GameArea` — при выходе за границы телепортирует на противоположную сторону |
| SHIP-06 | При уничтожении: скрыть, respawn через 2с в центре | `Game.OnShipDestroyed` → скрывает ShipVisual → `ActionScheduler` через 2с → `EntitiesCatalog.RespawnShip` (D-05: без VFX) |
| SHIP-07 | После respawn — неуязвимость 3с с миганием | `ShipModel` хранит `IsInvulnerable` флаг; toggle `SpriteRenderer.enabled` через `ActionScheduler` (N=0.2с) на 3с |
| SHIP-08 | Визуальный индикатор тяги — только при нажатой тяге | `ThrustComponent.IsActive` (ObservableValue) → биндинг → `ShipVisual` переключает `MainSprite`/`ThrustSprite` из `GameData.ShipData` |
| SHOT-01 | Пуля из носа корабля при Space | `PlayerInput.OnAttackAction` → `Game.OnAttack` → `ShipModel.Gun.Shooting=true` → `GunSystem` → коллбэк `OnShooting` → `EntitiesCatalog.CreateBullet` |
| SHOT-02 | Пуля = направление носа + скорость корабля | `BulletModel.MoveComponent.Velocity` = `bulletSpeed * shipDir + shipVelocity` |
| SHOT-03 | Не более 5 пуль одновременно (D-07: MaxShoots=5, не 4) | `GunSystem` проверяет `GunComponent.CurrentShoots < Gun.MaxShoots` |
| SHOT-04 | Пуля исчезает через 2с (D-07: LifeTimeSeconds=2, не 0.8) | `LifeTimeSystem` управляет `LifeTimeComponent`; по истечении — `bullet.Kill()` |
| SHOT-05 | Пули — wrap-around экрана | `MoveSystem` применяет ту же wrap-around логику ко всем сущностям с `MoveComponent` |
| SHOT-06 | Object Pool для пуль | `GameObjectPool` (Stack-based) + `ViewFactory` / `ModelFactory` — пул по prefab id |
</phase_requirements>

---

## Summary

Phase 3 создаёт всю кодовую базу игры с нуля: ~35+ C# файлов в 5 слоях (Application, Model, View, Input, Utils). Архитектура — кастомный ECS (системы обрабатывают компоненты сущностей) поверх MVVM через библиотеку `com.shtl.mvvm` (уже в PackageCache). Никаких внешних зависимостей добавлять не нужно — все пакеты уже подключены.

Ключевые технические инсайты: (1) движение через `ObservableValue<Vector2>` → `ReactiveValue` → `Transform.position` (не через `Rigidbody2D.MovePosition`), (2) коллизии через `OnCollisionEnter2D` на Visual MonoBehaviour → callback → `Game`, (3) `ActionScheduler` тикается в `Model.Update` — это единственный механизм таймеров (respawn, лазерная перезарядка, мигание), (4) `ReactiveValue<T>` в MVVM-библиотеке допускает ровно **одну** подписку (`Connect` бросает исключение при повторном вызове).

Самый сложный аспект фазы — правильная организация lifecycle: каждая сущность создаётся тройкой (ModelFactory + ViewFactory + EventBindingContext), уничтожается через `EntitiesCatalog.Release` с `CleanUp` биндингов. Input Action Map в существующем файле называется `PlayerControls` (не `Player` как в D-10) — нужно создать новый .inputactions или переименовать.

**Primary recommendation:** Создавать файлы волнами: сначала базовая инфраструктура (Utils → Configs → Model core → Input → Application scaffold), затем корабль (Ship entity full cycle), затем пули (Gun + Bullet), затем лазер. Каждая волна компилируется и тестируется через MCP `compile`.

---

## Standard Stack

### Core
| Библиотека | Версия | Назначение | Почему стандарт |
|------------|--------|-----------|----------------|
| Unity 2022.3.60f1 | LTS | Game engine | Зафиксировано в ProjectSettings |
| com.shtl.mvvm | git c7bda1c | MVVM + реактивные привязки | Уже в PackageCache, ключевой паттерн архитектуры |
| com.unity.inputsystem | 1.19.0 | Ввод игрока | Уже в Packages/manifest.json, PlayerActions.inputactions |
| com.unity.textmeshpro | 3.0.9 | UI текст (HUD, TitleScreen) | Уже подключён |
| com.unity.ugui | 1.0.0 | Canvas, Button, Image | Уже подключён |

### Supporting
| Библиотека | Версия | Назначение | Когда использовать |
|------------|--------|-----------|-------------------|
| com.unity.feature.2d | 2.0.1 | Physics2D, Sprites | Уже подключён — для SpriteRenderer, Rigidbody2D, Collider2D |

### Alternatives Considered
| Вместо | Можно было | Tradeoff |
|--------|-----------|---------|
| Кастомный ECS | Unity ECS (DOTS) | Unity ECS несовместим с existing архитектурой; кастомный ECS уже задан reference |
| ObservableValue → ReactiveValue pipeline | Прямой Update в Visual | Pipeline отделяет модель от вью; required by architecture |
| ActionScheduler | MonoBehaviour Coroutines | ActionScheduler не требует MonoBehaviour; тикается в pure C# |

**Установка не требуется** — все пакеты уже в `Packages/manifest.json` и `PackageCache`.

---

## Architecture Patterns

### Рекомендуемая структура создаваемых файлов

```
Assets/Scripts/
├── Application/
│   ├── IApplicationComponent.cs       # interface: OnUpdate, OnPause, OnResume events
│   ├── ApplicationEntry.cs             # MonoBehaviour: единственная точка входа Unity
│   ├── Application.cs                  # Pure C#: владеет всем, подписан на OnUpdate
│   ├── Game.cs                         # Игровой процесс, collision handlers, respawn
│   ├── EntitiesCatalog.cs              # Фабрика+реестр; CreateShip, CreateBullet, Release
│   ├── ModelFactory.cs                 # new TModel() + Model.AddEntity
│   ├── ViewFactory.cs                  # GameObjectPool.Get / Release
│   └── Screens/
│       ├── AbstractScreen.cs           # EventBindingContext владелец
│       ├── TitleScreen.cs              # Connect(onStart) → кнопка Play
│       └── GameScreen.cs               # HUD биндинги, State.Game / State.EndGame
├── Configs/
│   ├── BaseGameEntityData.cs           # abstract ScriptableObject: Score int
│   ├── GameData.cs                     # главный SO + вложенные structs: BulletData, LaserData, ShipData
│   ├── GunData.cs                      # ScriptableObject: MaxShoots, ReloadDurationSec
│   ├── BulletData.cs                   # (struct внутри GameData или отдельный SO)
│   ├── AsteroidData.cs                 # для Phase 4, структура создаётся
│   └── UfoData.cs                      # для Phase 4, структура создаётся
├── Input/
│   ├── PlayerInput.cs                  # C# обёртка, публикует события
│   ├── PlayerActions.cs                # partial-класс (ручная часть)
│   └── Generated/
│       └── PlayerActions.cs            # авто-генерируется Unity Input System
├── Model/
│   ├── Model.cs                        # реестр систем и сущностей, Update loop
│   ├── ActionScheduler.cs              # таймеры без корутин
│   ├── Components/
│   │   ├── IModelComponent.cs
│   │   ├── MoveComponent.cs
│   │   ├── RotateComponent.cs
│   │   ├── ThrustComponent.cs
│   │   ├── GunComponent.cs
│   │   ├── LaserComponent.cs
│   │   ├── LifeTimeComponent.cs
│   │   ├── MoveToComponent.cs          # Phase 4 (UFO), структура создаётся
│   │   └── ShootToComponent.cs         # Phase 4 (UFO), структура создаётся
│   ├── Entities/
│   │   ├── IGameEntityModel.cs         # IsDead(), Kill(), AcceptWith(IGroupVisitor)
│   │   ├── IGroupVisitor.cs            # Visit(ShipModel), Visit(BulletModel) ...
│   │   ├── ShipModel.cs
│   │   ├── BulletModel.cs
│   │   ├── AsteroidModel.cs            # Phase 4, структура
│   │   └── UfoBigModel.cs              # Phase 4-5, структура
│   └── Systems/
│       ├── BaseModelSystem.cs          # + IModelSystem interface
│       ├── MoveSystem.cs
│       ├── RotateSystem.cs
│       ├── ThrustSystem.cs
│       ├── GunSystem.cs
│       ├── LaserSystem.cs
│       ├── LifeTimeSystem.cs
│       ├── MoveToSystem.cs             # Phase 4, заглушка
│       └── ShootToSystem.cs            # Phase 4, заглушка
├── Utils/
│   ├── GameObjectPool.cs               # Stack<GameObject> по prefab id
│   ├── GameUtils.cs                    # позиции спавна
│   └── CoroutineResult.cs              # типизированный результат корутин
└── View/
    ├── Base/
    │   ├── BaseVisual.cs
    │   └── IEntityView.cs
    ├── Bindings/
    │   └── BindingToExtensions.cs      # From(ObservableValue<Vector2>).To(Transform)
    ├── Components/
    │   └── GuiText.cs
    ├── ShipVisual.cs                   # ShipViewModel + ShipVisual
    ├── BulletVisual.cs                 # BulletViewModel + BulletVisual
    ├── EffectVisual.cs
    ├── HudVisual.cs                    # HudData + HudVisual
    └── TitleScreenView.cs              # TitleScreenViewModel + TitleScreenView

Assets/Media/
├── configs/
│   ├── GameData.asset
│   ├── UserGunData.asset
│   └── UfoGunData.asset
└── prefabs/
    ├── ship.prefab
    ├── bullet.prefab
    └── bullet_enemy.prefab

Assets/Input/
└── PlayerActions.inputactions          # переименовать ActionMap: PlayerControls → Player
```

### Pattern 1: Создание сущности (Entity lifecycle)

**Что:** Тройка Model + View + EventBindingContext
**Когда использовать:** Для каждой игровой сущности (ship, bullet)

```csharp
// Source: ARCHITECTURE.md — EntitiesCatalog pattern
public ShipModel CreateShip()
{
    // 1. Model
    var model = _modelFactory.Create<ShipModel>();
    model.Move = new MoveComponent();
    model.Rotate = new RotateComponent();
    model.Thrust = new ThrustComponent(_configs.Ship.ThrustUnitsPerSecond, _configs.Ship.MaxSpeed);
    model.Gun = new GunComponent(_configs.Ship.Gun);

    // 2. View
    var view = _viewFactory.Get<ShipVisual>(_configs.Ship.Prefab);

    // 3. ViewModel + Bindings
    var vm = new ShipViewModel();
    var bind = new EventBindingContext();
    bind.From(model.Move.Position).To(vm.Position);
    bind.From(model.Thrust.IsActive).To(vm.IsThrusting);
    view.Connect(vm);

    // 4. Register
    _modelToView[model] = view;
    _modelToGo[model] = view.gameObject;
    return model;
}
```

### Pattern 2: Система (BaseModelSystem)

**Что:** Система регистрирует сущности через Visitor, обновляет их каждый кадр
**Когда использовать:** Для каждого аспекта поведения (Move, Rotate, Thrust, Gun, Laser, LifeTime)

```csharp
// Source: ARCHITECTURE.md — BaseModelSystem pattern
public class MoveSystem : BaseModelSystem<MoveComponent>
{
    private readonly Vector2 _gameArea;

    public MoveSystem(Vector2 gameArea) { _gameArea = gameArea; }

    protected override void UpdateNode(IGameEntityModel entity, MoveComponent move, float dt)
    {
        var newPos = move.Position.Value + move.Direction * (move.Speed.Value * dt);
        // Wrap-around
        newPos.x = WrapAxis(newPos.x, _gameArea.x);
        newPos.y = WrapAxis(newPos.y, _gameArea.y);
        move.Position.Value = newPos;
    }

    private static float WrapAxis(float val, float size)
    {
        if (val > size / 2f) { return val - size; }
        if (val < -size / 2f) { return val + size; }
        return val;
    }
}
```

### Pattern 3: Reactive Binding (ObservableValue → ReactiveValue → Unity)

**Что:** Изменение модели автоматически обновляет Unity-компонент без прямых ссылок
**Когда использовать:** Для всех пробросов данных Model → View

```csharp
// Source: com.shtl.mvvm — ModelToViewModelEventBindExtensions.cs
// В EntitiesCatalog:
bind.From(model.Move.Position).To(vm.Position);
// ^ ObservableValue<Vector2> → ReactiveValue<Vector2> в ViewModel

// В ShipVisual.OnConnected():
Bind.From(ViewModel.Position).To(transform);
// ^ ReactiveValue<Vector2> → Transform.position через BindingToExtensions
```

**КРИТИЧНО:** `ReactiveValue<T>.Connect()` допускает только **одну** подписку — если попытаться подключить второй раз, бросается `InvalidOperationException("Already bound")`. При `EntitiesCatalog.Release` нужно обязательно вызвать `EventBindingContext.CleanUp()` → `ReactiveValue.Unbind()`.

### Pattern 4: ActionScheduler для таймеров

**Что:** Отложенное действие через pure C# без корутин
**Когда использовать:** Respawn (2с), мигание неуязвимости (каждые 0.2с на 3с), лазерная перезарядка (10с)

```csharp
// Source: ARCHITECTURE.md — ActionScheduler usage
// Respawn через 2 секунды:
_model.ActionScheduler.Schedule(2f, () => RespawnShip());

// Мигание — серия через ActionScheduler:
void ScheduleBlink(int blinksLeft, bool spriteVisible)
{
    if (blinksLeft <= 0)
    {
        _spriteRenderer.enabled = true; // финальное включение
        _shipModel.IsInvulnerable = false;
        return;
    }
    _spriteRenderer.enabled = spriteVisible;
    _model.ActionScheduler.Schedule(0.2f, () => ScheduleBlink(blinksLeft - 1, !spriteVisible));
}
// Запуск: 15 миганий = 3с / 0.2с
ScheduleBlink(15, false);
```

### Pattern 5: GunSystem и Object Pool для пуль

**Что:** GunSystem обнаруживает флаг Shooting, вызывает callback; Game создаёт пулю через пул
**Когда использовать:** SHOT-01..06

```csharp
// Source: ARCHITECTURE.md — data flow ввод → пуля
// GunComponent:
public class GunComponent
{
    public bool Shooting;                    // устанавливается Game.OnAttack
    public int CurrentShoots;               // счётчик активных пуль
    public GunData Config;
    public Action<GunComponent> OnShooting; // коллбэк → Game.OnUserGunShooting
}

// GunSystem.UpdateNode:
protected override void UpdateNode(IGameEntityModel entity, GunComponent gun, float dt)
{
    if (gun.Shooting && gun.CurrentShoots < gun.Config.MaxShoots)
    {
        gun.CurrentShoots++;
        gun.OnShooting?.Invoke(gun);
    }
    gun.Shooting = false; // сброс после обработки
}
```

### Anti-Patterns to Avoid

- **MonoBehaviour для логики:** Вся игровая логика — pure C#. Только `ApplicationEntry` и `*Visual` классы — MonoBehaviour.
- **Rigidbody2D для движения:** `Rigidbody2D` используется только для collision detection (Kinematic). Движение — через `MoveComponent.Position` (ObservableValue).
- **Прямой доступ View → Model:** View знает только ViewModel. Связь Model↔View — только через `EventBindingContext` биндинги в `EntitiesCatalog`.
- **Несколько подписчиков на ReactiveValue:** `ReactiveValue.Connect()` — ровно одна подписка. Для fan-out используйте `ObservableValue` (поддерживает `event Action`).
- **Создание/удаление GameObject в игровом цикле:** Все объекты через `GameObjectPool`. `Instantiate`/`Destroy` только при инициализации пула.
- **async/await:** Проект использует `IEnumerator` корутины + `CoroutineResult<T>`. Не вводить async/await.

---

## Don't Hand-Roll

| Проблема | Не строить | Использовать | Почему |
|---------|-----------|-------------|--------|
| Реактивные биндинги | Свой event system | `ObservableValue<T>` + `ReactiveValue<T>` + `EventBindingContext` из `com.shtl.mvvm` | Уже в PackageCache, протестировано, CleanUp гарантирован |
| Таймеры без MonoBehaviour | Coroutine scheduler | `ActionScheduler` (из архитектуры) | Тикается в Model.Update; не требует GameObject |
| Object Pool | Stack вручную по месту | `GameObjectPool` (Utils/GameObjectPool.cs) | Уже спроектирован с prefab-id |
| Ввод игрока | Input.GetKey() | `com.unity.inputsystem` + `PlayerInput` | Уже в manifest; обработка axis, initial state check |
| Конфиги | Хардкод констант | `GameData.asset` (ScriptableObject) | Параметры изменяются без перекомпиляции |

**Key insight:** Весь infrastructure layer (MVVM, пул, таймеры, ввод) уже существует в пакетах. Phase 3 — написать игровой код поверх него, не переизобретать infrastructure.

---

## Common Pitfalls

### Pitfall 1: ReactiveValue допускает только одну подписку

**What goes wrong:** `EntitiesCatalog` повторно подключает ViewModel к уже связанному `ReactiveValue` — `InvalidOperationException: Already bound`.
**Why it happens:** При respawn корабля создаётся новая ViewModel, но если биндинг не был очищен — старая подписка осталась.
**How to avoid:** Перед `view.Connect(newVm)` всегда вызывать `bindingContext.CleanUp()` → это вызовет `ReactiveValue.Unbind()` на всех полях через `AbstractViewModel.Unbind()`.
**Warning signs:** Exception в `ReactiveValue.Connect()` после respawn.

### Pitfall 2: Input Action Map name mismatch

**What goes wrong:** Существующий файл `Assets/Input/player_actions.inputactions` содержит Action Map с именем `PlayerControls`, а архитектура ожидает `Player` (D-10). Генерация C# кода создаст класс с неправильным именем.
**Why it happens:** CONTEXT.md D-10 задаёт `Player` как имя Action Map, но текущий файл имеет `PlayerControls`.
**How to avoid:** При создании нового `PlayerActions.inputactions` именовать Action Map как `Player`. Сгенерированный класс будет иметь метод `asset.Player`.
**Warning signs:** `PlayerActions.cs` не компилируется или имеет `PlayerControls` вместо `Player` в публичном API.

### Pitfall 3: ObservableValue не триггерит при равном значении

**What goes wrong:** `MoveComponent.Position.Value = newPos` не вызывает `OnChanged` если `newPos == _value`. При wrap-around на экране объект "застрял" — не обновляется Transform.
**Why it happens:** `ObservableValue<T>` использует `EqualityComparer<T>.Default.Equals` — для `Vector2` это поточное сравнение float.
**How to avoid:** Wrap-around гарантирует изменение значения (новая позиция ≠ старая). Дополнительных действий не требуется. Но не использовать `ObservableValue` для флагов, которые могут повторно получить то же значение (использовать `ReactiveValue` с `Force` или отдельный `bool`).
**Warning signs:** Visual не двигается при первом кадре.

### Pitfall 4: Порядок регистрации систем в Model

**What goes wrong:** Система вызывает данные, обновлённые системой, которая ещё не запустилась в этом кадре — получаем лаг на 1 кадр.
**Why it happens:** `Model.Update` итерирует системы строго в порядке регистрации.
**How to avoid:** Строгий порядок из ARCHITECTURE.md: `RotateSystem` → `ThrustSystem` → `MoveSystem` → `LifeTimeSystem` → `GunSystem` → `LaserSystem` → `ShootToSystem` → `MoveToSystem`.
**Warning signs:** Корабль визуально запаздывает на 1 кадр при повороте перед тягой.

### Pitfall 5: GravityScale не обнулён в Rigidbody2D

**What goes wrong:** Корабль/пули падают вниз под действием гравитации Physics2D (gravity = (0, -9.81)).
**Why it happens:** `Rigidbody2D` по умолчанию `GravityScale=1`.
**How to avoid:** В каждом prefab (ship, bullet, bullet_enemy) установить `Rigidbody2D.GravityScale = 0` и `BodyType = Kinematic`.
**Warning signs:** Объекты дрейфуют вниз независимо от игровой логики.

### Pitfall 6: GameArea вычисляется некорректно для WebGL 1920×1080

**What goes wrong:** Размер игровой области вычислен неверно → wrap-around происходит не на краю экрана.
**Why it happens:** `Camera.orthographicSize` даёт полу-высоту; ширина зависит от aspect ratio.
**How to avoid:** В `Application.Start()`:
```csharp
var cam = Camera.main;
var height = cam.orthographicSize * 2f;
var width = height * cam.aspect;
_model.GameArea = new Vector2(width, height);
```
Для 1920×1080 при стандартном orthographicSize=5: height=10, width≈17.78. Выбрать orthographicSize чтобы охватывал всё игровое поле.
**Warning signs:** Объекты телепортируются до или после края экрана.

### Pitfall 7: BulletData — int vs float для LifeTimeSeconds

**What goes wrong:** `GameData.BulletData.LifeTimeSeconds` объявлен как `int` в DATA_SCHEMA (значение 2). Если разработчик объявит как `float` — нет проблем, но YAML .asset будет ожидать `int`.
**Why it happens:** DATA_SCHEMA явно указывает `int` для `LifeTimeSeconds`.
**How to avoid:** Объявить `public int LifeTimeSeconds` в struct `GameData.BulletData`. `LifeTimeSystem` приводит к `float` при вычитании deltaTime.
**Warning signs:** .asset не сохраняет значение или показывает 0.

---

## Code Examples

Верифицированные паттерны из библиотеки `com.shtl.mvvm` (прочитана из PackageCache):

### AbstractViewModel — поля как публичные ReactiveValue<T>

```csharp
// Source: PackageCache/com.shtl.mvvm@c7bda1c328/Runtime/Core/Types/AbstractViewModel.cs
// AbstractViewModel в конструкторе через reflection собирает все IReactiveValue поля
// => все ReactiveValue ДОЛЖНЫ быть публичными полями (не свойствами!)
public class ShipViewModel : AbstractViewModel
{
    public ReactiveValue<Vector2> Position = new();
    public ReactiveValue<bool> IsThrusting = new();
    public ReactiveValue<bool> IsVisible = new();
    public Action<Collision2D> OnCollision;  // не ReactiveValue — коллбэк
}
```

### AbstractWidgetView — OnConnected() паттерн

```csharp
// Source: PackageCache/com.shtl.mvvm@c7bda1c328/Runtime/Core/AbstractWidgetView.cs
public class ShipVisual : AbstractWidgetView<ShipViewModel>
{
    [SerializeField] private SpriteRenderer _spriteRenderer;

    protected override void OnConnected()
    {
        // Биндинги создаются через Bind (protected IEventBindingContext)
        Bind.From(ViewModel.Position).To(transform);
        Bind.From(ViewModel.IsThrusting).To(_spriteRenderer, (val, sr) => {
            sr.sprite = val ? _thrustSprite : _mainSprite;
        });
    }
}
```

### EventBindingContext.CleanUp() при Release

```csharp
// Source: PackageCache/com.shtl.mvvm@c7bda1c328/Runtime/Core/Bindings/EventBindingContext.cs
// CleanUp() снимает все биндинги и возвращает их в BindingPool
// ОБЯЗАТЕЛЬНО вызывать при EntitiesCatalog.Release
public void Release(IGameEntityModel model)
{
    if (_modelToBind.TryGetValue(model, out var bind))
    {
        bind.CleanUp(); // снимает ReactiveValue.Unbind() на всех полях ViewModel
    }
    _viewFactory.Release(_modelToView[model]);
    // ...
}
```

### ScriptableObject с CreateAssetMenu

```csharp
// Source: CONVENTIONS.md — [CreateAssetMenu] pattern
[CreateAssetMenu(menuName = "Game data")]
public class GameData : ScriptableObject
{
    [Header("Gameplay")]
    public int AsteroidInitialCount = 10;
    public int SpawnAllowedRadius = 20;
    public float SpawnNewEnemyDurationSec = 25f;

    [Space]
    public GameObject VfxBlowPrefab; // null в Phase 3

    [Serializable]
    public struct BulletData
    {
        public GameObject Prefab;
        public GameObject EnemyPrefab;
        public int LifeTimeSeconds;   // int! (DATA_SCHEMA)
        public float Speed;
    }

    // ...
    public BulletData Bullet;
    public LaserData Laser;
    public ShipData Ship;
}
```

---

## State of the Art

| Старый подход | Текущий подход | Изменился | Impact |
|--------------|----------------|-----------|--------|
| Input.GetKey() | Unity Input System 1.19 с .inputactions | Unity 2019+ | Биндинги через asset; поддержка axis, initial state check |
| MonoBehaviour повсюду | Единственный MonoBehaviour = ApplicationEntry | Архитектурное решение | Pure C# логика, тестируемость |
| Coroutines для таймеров | ActionScheduler (pure C# timer) | Архитектурное решение | Нет зависимости от GameObject lifecycle |
| Rigidbody2D для движения | Кинематика через MoveSystem + ObservableValue | Архитектурное решение | Полный контроль над физикой, детерминизм |

**Deprecated/Outdated:**
- `Sprite Atlas` (GameAtlas.spriteAtlas): создан в Phase 1, но D-01 отменяет его использование — prefabs используют sub-sprites напрямую.
- `Input.GetAxis()` / `Input.GetKey()` — старая Input система не используется (activeInputHandler=1 = New Input System).

---

## Open Questions

1. **Имя Action Map в PlayerActions.inputactions**
   - Что знаем: Существующий файл имеет Action Map `PlayerControls` (из UNITY_CONFIG.md). CONTEXT.md D-10 требует `Player`.
   - Что неясно: Нужно ли создать новый файл или переименовать Action Map в существующем.
   - Recommendation: Создать новый файл `Assets/Input/PlayerActions.inputactions` с Action Map `Player`, оставив старый `player_actions.inputactions` нетронутым (он не используется в коде).

2. **`BindingToExtensions` — нужно ли писать с нуля**
   - Что знаем: Архитектура ссылается на `View/Bindings/BindingToExtensions.cs` как на кастомный файл проекта. В `com.shtl.mvvm` есть `BindFromExtensions` (From/To паттерн) и `ViewModelToUIEventBindExtensions`, но нет готового `To(Transform)`.
   - Что неясно: Точная сигнатура extension `From(ObservableValue<Vector2>).To(Transform)`.
   - Recommendation: Создать `BindingToExtensions.cs` с методом `To(Transform t)` который вызывает `t.position = (Vector3)val` через `ObservableValueEventBinding`.

3. **Мигание — через ActionScheduler или LifeTimeSystem**
   - Что знаем: CONTEXT.md D-05 упоминает "через LifeTimeSystem / ActionScheduler".
   - Recommendation: Использовать `ActionScheduler` (рекурсивные schedule вызовы), так как `LifeTimeSystem` предназначен для уничтожения сущностей по таймеру, а не для визуальных эффектов. Мигание = серия `ActionScheduler.Schedule(0.2f, ...)` с счётчиком.

---

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|-------------|-----------|---------|---------|
| Unity 2022.3.60f1 | Весь проект | ✓ | 2022.3.60f1 | — |
| com.shtl.mvvm | MVVM layer | ✓ | c7bda1c (PackageCache) | — |
| com.unity.inputsystem | PlayerInput | ✓ | 1.19.0 | — |
| com.unity.textmeshpro | HudVisual, GuiText | ✓ | 3.0.9 | — |
| MCP Unity Bridge | compile/play через Claude | ✓ | Phase 2 завершена | Ручная компиляция |
| asteroids.png (спрайты) | Prefab спрайты | ✓ | В Assets/Media/sprites/ | — |

**Missing dependencies:** Нет. Все зависимости присутствуют.

---

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | Unity Test Framework 1.1.33 (NUnit) |
| Config file | нет явного конфига — Unity Test Runner встроен |
| Quick run command | MCP tool `compile` — проверяет компиляцию без ошибок |
| Full suite command | Play Mode через MCP `play` → ручная проверка по Success Criteria |

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| SHIP-01 | Корабль вращается по A/D | manual | `mcp:play` → нажать A/D | ❌ Wave 0 |
| SHIP-02 | Тяга накапливает скорость | manual | `mcp:play` → W | ❌ Wave 0 |
| SHIP-03 | Инерция без замедления | manual | `mcp:play` → отпустить W | ❌ Wave 0 |
| SHIP-04 | MaxSpeed ограничение | manual | `mcp:play` → длительная тяга | ❌ Wave 0 |
| SHIP-05 | Wrap-around корабля | manual | `mcp:play` → уйти за край | ❌ Wave 0 |
| SHIP-06 | Respawn через 2с | manual | `mcp:play` → коллизия с астероидом (Phase 4) или прямой вызов | ❌ Wave 0 |
| SHIP-07 | Мигание 3с | manual | `mcp:play` → respawn → наблюдать мигание | ❌ Wave 0 |
| SHIP-08 | Сопло только при W | manual | `mcp:play` → W зажат/отпущен | ❌ Wave 0 |
| SHOT-01 | Выстрел по Space | manual | `mcp:play` → Space | ❌ Wave 0 |
| SHOT-02 | Пуля = dir корабля + его velocity | manual | `mcp:play` → стрельба при движении | ❌ Wave 0 |
| SHOT-03 | Максимум 5 пуль | manual | `mcp:play` → быстро Space × 6 | ❌ Wave 0 |
| SHOT-04 | Пуля исчезает через 2с | manual | `mcp:play` → стрельба в воздух | ❌ Wave 0 |
| SHOT-05 | Wrap-around пуль | manual | `mcp:play` → стрельба к краю экрана | ❌ Wave 0 |
| SHOT-06 | Object Pool — нет Instantiate в loop | unit | `mcp:compile` — нет `Instantiate` в GunSystem/Update | ❌ Wave 0 |

**Основной механизм валидации:** MCP `compile` после каждого wave (0 ошибок компиляции) + MCP `play` для ручной проверки поведения.

### Sampling Rate
- **Per task commit:** `mcp:compile` — проверка компиляции
- **Per wave merge:** `mcp:compile` + `mcp:play` + ручная проверка списка Success Criteria
- **Phase gate:** Все 5 Success Criteria фазы зелёные перед `/gsd:verify-work`

### Wave 0 Gaps
- [ ] `Assets/Scripts/Utils/GameObjectPool.cs` — покрывает SHOT-06
- [ ] `Assets/Scripts/Model/ActionScheduler.cs` — покрывает SHIP-06, SHIP-07
- [ ] `Assets/Scripts/Application/ApplicationEntry.cs` — базовый scaffold для старта Play Mode
- [ ] `Assets/Input/PlayerActions.inputactions` — Action Map `Player` для Input System

*(Тестовая инфраструктура Unity Test Framework присутствует как пакет, но для Phase 3 достаточно compile+play проверок — поведенческие требования верифицируются вручную через MCP)*

---

## Project Constraints (from CLAUDE.md)

Глобальный `CLAUDE.md` найден в `~/.claude/CLAUDE.md` (не в проекте):
- **Язык:** Все ответы и документация (комментарии, README, docstrings) — на **русском языке**.
- **Фигурные скобки в C#:** Всегда использовать `{}` даже для однострочных тел. Стиль скобок — по окружающему коду (Allman для методов/классов, K&R для if/for/while — согласно CONVENTIONS.md).

Проектный `./CLAUDE.md` — **отсутствует**.

---

## Sources

### Primary (HIGH confidence)
- `.planning/codebase/ARCHITECTURE.md` — ECS+MVVM архитектура, поток данных, все системы, паттерны
- `.planning/codebase/CONVENTIONS.md` — naming, стиль кода, паттерны Connect/Dispose
- `.planning/codebase/STRUCTURE.md` — полная структура папок, именование файлов
- `.planning/codebase/DATA_SCHEMA.md` — все поля и значения GameData, BulletData, ShipData, LaserData, GunData
- `.planning/codebase/UNITY_CONFIG.md` — физические слои, Collision Matrix, Input bindings
- `.planning/codebase/STACK.md` — версии пакетов, asmdef файлы
- `Library/PackageCache/com.shtl.mvvm@c7bda1c328/` — исходный код MVVM библиотеки (ObservableValue, ReactiveValue, AbstractWidgetView, EventBindingContext)
- `.planning/phases/03-core-mechanics/3-CONTEXT.md` — все locked decisions D-01..D-20

### Secondary (MEDIUM confidence)
- `.planning/REQUIREMENTS.md` §Ship, §Shooting — поведенческие требования (числа NOT из этого файла — из DATA_SCHEMA)

### Tertiary (LOW confidence)
- Нет LOW confidence источников — все данные из official project files

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — все пакеты верифицированы в `Packages/manifest.json` и `PackageCache`
- Architecture: HIGH — прочитан исходный код MVVM библиотеки + детальная ARCHITECTURE.md
- Data schema: HIGH — прочитан DATA_SCHEMA.md с точными значениями .asset файлов
- Pitfalls: HIGH — выведены из прочитанного исходного кода ReactiveValue/ObservableValue + известных Unity-паттернов
- Input config: MEDIUM — обнаружено расхождение Action Map name; требует уточнения при создании файла

**Research date:** 2026-03-27
**Valid until:** 2026-04-27 (стабильный стек Unity LTS)
