# Phase 5: UFO & Progression Polish — Research

**Researched:** 2026-03-28
**Domain:** Unity 2022.3 C# — ECS-подобная архитектура (Model/View/System), Object Pool, MVVM-биндинги
**Confidence:** HIGH (вся кодовая база изучена напрямую)

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
Нет зафиксированных решений — фаза discuss пропущена (`workflow.skip_discuss: true`).

### Claude's Discretion
Все архитектурные и реализационные решения на усмотрение Claude. Руководствоваться целью фазы, критериями успеха и паттернами существующего кода.

### Deferred Ideas (OUT OF SCOPE)
Нет — фаза discuss пропущена.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| UFO-01 | Large UFO появляется случайно каждые 25–40 сек с края экрана, движется прямолинейно с редкими сменами направления | `ActionScheduler.Schedule()` для таймера, `GameArea` для краёв экрана, `MoveSystem` уже поддерживает wrap-around |
| UFO-02 | Small UFO появляется после счёта 10 000, стреляет в направлении корабля (точный) | `_model.Score >= 10000` как триггер, `ShootToSystem` (заглушка) нужно реализовать, `EntitiesCatalog.Ship` даёт позицию корабля |
| UFO-03 | Large UFO стреляет в случайном направлении | `GunSystem` уже работает, нужна регистрация `GunComponent` в GroupCreator + `OnShooting` callback аналогично кораблю |
| UFO-04 | UFO проходит wrap-around экрана | `MoveSystem` обрабатывает wrap-around автоматически — нужно зарегистрировать `UfoBigModel.Move` в системе через GroupCreator |
| UFO-05 | UFO исчезает после пересечения экрана (если не уничтожен) | Нужна логика отслеживания "прошёл ли UFO всю ширину/высоту экрана" — через `LifeTimeComponent` не подходит, нужен отдельный счётчик пройденного расстояния |
| UFO-06 | На экране одновременно не более одного UFO | Флаг `_ufoActive` в `Game.cs` + проверка перед спауном |
| PROG-06 | Текущая волна отображается в HUD при старте каждой новой волны (кратковременный баннер) | `HudVisual` + новый `TextMeshProUGUI` элемент + `ActionScheduler` для автоскрытия через 2–3 сек |
</phase_requirements>

---

## Summary

Phase 5 реализует два типа UFO и баннер номера волны. Вся необходимая инфраструктура уже существует в кодовой базе: `UfoBigModel` и `UfoModel` определены, `MoveToSystem` и `ShootToSystem` зарегистрированы (как заглушки), `GunSystem` работает, `ActionScheduler` позволяет планировать спаун. Основная работа — снять статус заглушки с систем и дописать логику в `Game.cs` и `EntitiesCatalog`.

Ключевая сложность UFO-05 (исчезновение после пересечения экрана): UFO должен пройти через весь экран и исчезнуть на противоположном краю — это отличается от простого wrap-around. Правильное решение: отслеживать начальный край появления и уничтожать UFO когда он добирается до противоположного края второй раз (или отслеживать суммарное пройденное расстояние > ширина/высота экрана).

**Primary recommendation:** Реализовать UFO как расширение паттернов Phase 4 (asteroid/bullet). Все компоненты уже созданы — нужны только полные реализации систем и EntitiesCatalog.CreateUfo().

---

## Project Constraints (from CLAUDE.md)

- Язык документации и комментариев: **русский**
- В C-подобных языках (C#): **всегда фигурные скобки** в if/else/for/while, даже при однострочном теле
- Стиль скобок: K&R (открывающая на той же строке) — соответствует существующему коду

---

## Standard Stack

### Core (существующий стек проекта)
| Компонент | Версия/Тип | Назначение | Комментарий |
|-----------|-----------|-----------|-------------|
| Unity | 2022.3 LTS | Движок | Built-in RP, WebGL |
| C# | .NET Standard 2.1 | Язык | |
| com.shtl.mvvm | git hash c7bda1c | MVVM-биндинги | `ObservableValue`, `ReactiveValue`, `EventBindingContext` |
| TextMeshPro | built-in | UI текст | HUD баннер волны |
| ActionScheduler | custom | Отложенные вызовы | Уже в Model — таймер спауна UFO, автоскрытие баннера |
| GameObjectPool | custom | Object Pool | Уже работает — UFO prefab нужно зарегистрировать |

### Нет внешних зависимостей
Все необходимые утилиты уже в проекте. Нет новых пакетов для установки.

---

## Architecture Patterns

### Существующая архитектура (строго следовать)

```
Assets/Scripts/
├── Application/          # Application, Game, EntitiesCatalog, ModelFactory, Screens/
├── Configs/              # ScriptableObject данные (GameData, UfoData уже есть)
├── Model/
│   ├── Components/       # GunComponent, MoveComponent, MoveToComponent, ShootToComponent (всё есть)
│   ├── Entities/         # UfoBigModel, UfoModel (уже определены в Phase 4)
│   └── Systems/          # MoveToSystem, ShootToSystem (заглушки — нужно реализовать)
├── View/                 # VisualModel пары (нужно создать UfoVisual)
└── Utils/                # ActionScheduler, GameObjectPool
```

### Pattern 1: Создание сущности (паттерн EntitiesCatalog)

Каждая игровая сущность создаётся по схеме:
1. Создать Model через `_modelFactory.Create<TModel>()`
2. Взять View из пула: `_pool.Get<TVisual>(prefab)`
3. Создать ViewModel + EventBindingContext, назначить биндинги
4. Зарегистрировать в словарях: `_modelToView`, `_modelToBind`, `_modelToGo`, `_goToModel`

```csharp
// Паттерн — аналогично CreateAsteroid():
public UfoBigModel CreateUfoBig(Vector2 position, Vector2 velocity)
{
    // 1. Model
    var model = _modelFactory.Create<UfoBigModel>();
    model.Move.Position.Value = position;
    model.Move.Direction.Value = velocity.normalized;
    model.Move.Speed.Value = velocity.magnitude;

    // 2. View
    var view = _pool.Get<UfoVisual>(_configs.UfoBig.Prefab);
    view.SetPrefabId(_configs.UfoBig.Prefab.GetInstanceID());

    // 3. ViewModel + Bindings
    var vm = new UfoViewModel();
    var bind = new EventBindingContext();
    bind.From(model.Move.Position).To(vm.Position);
    view.Connect(vm);
    bind.InvokeAll();

    // 4. Register
    _modelToView[model] = view;
    _modelToBind[model] = bind;
    _modelToGo[model] = view.gameObject;
    _goToModel[view.gameObject] = model;

    return model;
}
```

**Источник:** Прямой анализ `EntitiesCatalog.cs`, HIGH confidence.

### Pattern 2: Регистрация в системах через GroupCreator (Visitor)

Чтобы UFO попал в `MoveSystem`, `GunSystem` и т.д. — реализовать `Visit(UfoBigModel)` в GroupCreator:

```csharp
// В Model.cs GroupCreator.Visit(UfoBigModel model):
public void Visit(UfoBigModel model)
{
    _owner.GetSystem<MoveSystem>().Add(model, model.Move);
    _owner.GetSystem<GunSystem>().Add(model, model.Gun);
    // UfoModel (Small) дополнительно:
    if (model is UfoModel ufo)
    {
        _owner.GetSystem<ShootToSystem>().Add(model, ufo.ShootTo);
    }
}
```

**Источник:** `Model.cs` (GroupCreator), `IGroupVisitor.cs`, HIGH confidence.

### Pattern 3: Спаун UFO через ActionScheduler

```csharp
// В Game.cs — запланировать первый спаун:
private void ScheduleUfoSpawn()
{
    var delay = UnityEngine.Random.Range(25f, 40f);
    _model.ActionScheduler.Schedule(delay, TrySpawnUfo);
}

private void TrySpawnUfo()
{
    if (!_isRunning || _ufoActive) { return; }
    SpawnUfoBig();
    ScheduleUfoSpawn(); // запланировать следующий
}
```

**Источник:** `ActionScheduler.cs`, паттерн из `Game.cs` (RespawnShip), HIGH confidence.

### Pattern 4: UFO-05 — исчезновение после пересечения экрана

UFO-05 сложнее чем wrap-around. Алгоритм:

- Записать при создании: стартовый край (Left/Right/Top/Bottom) и флаг `_hasWrapped`
- В `Update` или через компонент — отслеживать когда UFO пересёк первый wrap-around (координата вышла за границу)
- При первом wrap-around установить `_hasWrapped = true`
- При следующем выходе за тот же край — `Kill()`

Альтернатива (проще): добавить в `UfoBigModel` поле `float DistanceTraveled`. Через `MoveSystem` или кастомное обновление: если пройденное расстояние > `GameArea.x * 1.5f` → Kill(). Работает для горизонтального движения.

**Рекомендация:** Использовать отслеживание стартового края + флаг wrap-around. Реализовать в `Game.cs` через подписку на `model.Move.Position` (ObservableValue).

### Pattern 5: ShootToSystem — точная стрельба Small UFO

`ShootToComponent` содержит `ShootInterval` и `Timer`. Реализация `ShootToSystem.UpdateNode`:

```csharp
protected override void UpdateNode(IGameEntityModel entity, ShootToComponent shootTo, float deltaTime)
{
    shootTo.Timer -= deltaTime;
    if (shootTo.Timer > 0f) { return; }
    shootTo.Timer = shootTo.ShootInterval;

    // Вычислить направление на корабль
    if (entity is UfoModel ufo)
    {
        shootTo.OnShoot?.Invoke(ufo); // callback → Game → CreateBullet
    }
}
```

Нужно добавить `Action<UfoModel> OnShoot` в `ShootToComponent` (аналогично `GunComponent.OnShooting`).

**Источник:** `ShootToComponent.cs`, `GunSystem.cs` (паттерн), HIGH confidence.

### Pattern 6: HUD-баннер волны (PROG-06)

Баннер — новый `TextMeshProUGUI` в HudVisual. Автоскрытие через `ActionScheduler`:

```csharp
// В Game.cs после StartWave():
private void ShowWaveBanner()
{
    _onWaveBannerShow?.Invoke(_waveNumber);           // callback → Application → HudVisual
    _model.ActionScheduler.Schedule(2.5f, HideWaveBanner);
}

private void HideWaveBanner()
{
    _onWaveBannerHide?.Invoke();
}
```

Альтернатива: `HudVisual.ShowWaveBanner(int wave)` + Unity Coroutine для скрытия. Но ActionScheduler уже используется в проекте и не требует MonoBehaviour.

**Рекомендация:** Callback через `_onWaveBannerShow` в `Game.cs` → `Application.cs` → `GameScreen.cs` → `HudVisual`.

### Anti-Patterns to Avoid

- **Не использовать `Coroutine`** для таймеров — в проекте используется `ActionScheduler`
- **Не добавлять MonoBehaviour** в Model-слой — строгое разделение
- **Не создавать UFO prefab через код** — только через `Phase5Setup.cs` (Editor script)
- **Не дублировать Dictionary `_goToModel`** — UFO должен быть в той же карте для коллизий
- **Не забыть `CleanUp()` в `ActionScheduler`** при Restart — уже есть в `Model.CleanUp()`
- **Не забыть зарегистрировать UFO prefab** в `EntitiesCatalog.Connect()` (аналогично asteroid prefabs)
- **Не использовать `GameObject.Find()`** для неактивных объектов в Setup-скрипте — использовать `Resources.FindObjectsOfTypeAll<T>()`

---

## Don't Hand-Roll

| Проблема | Не строить | Использовать | Причина |
|----------|-----------|-------------|---------|
| Таймер спауна UFO | Custom timer class | `ActionScheduler.Schedule()` | Уже в проекте, очищается при CleanUp |
| Object Pool для UFO prefab | Stack/список вручную | `GameObjectPool.Register()` + `Get<T>()` | Тот же пул что для asteroid/bullet |
| Биндинг позиции UFO | Прямой `transform.position = ...` в MonoBehaviour | `EventBindingContext` + `ReactiveValue` | Паттерн всех сущностей в проекте |
| Wrap-around UFO | Кастомный wrap код | `MoveSystem` (уже реализован) | Регистрация через GroupCreator.Visit() |
| Коллизия UFO с пулей | Отдельная физика | `OnCollisionEnter2D` → `ViewModel.OnCollision` | Паттерн AsteroidVisual/ShipVisual |

---

## Common Pitfalls

### Pitfall 1: UfoModel.Gun не инициализирован
**Что идёт не так:** `UfoBigModel.Gun` — `null` по умолчанию. `GunSystem` получит null Config → `if (gun.Config == null) { return; }` не выстрелит.
**Почему:** В отличие от ShipModel, Gun не создаётся в конструкторе UfoBigModel.
**Как избежать:** В `EntitiesCatalog.CreateUfoBig()` назначить `model.Gun = new GunComponent(_configs.UfoBig.Gun)` после создания.
**Признак:** UFO никогда не стреляет.

### Pitfall 2: UFO не попадает в Release при Restart
**Что идёт не так:** При `EntitiesCatalog.Reset()` UFO не попадёт в Release если не зарегистрирован в `_modelToView`.
**Почему:** `Reset()` итерирует `_modelToView.Keys` — если UFO там есть, всё ок. Если была ошибка при Create и он не попал в словарь — orphaned GO.
**Как избежать:** Регистрировать UFO в все 4 словаря в `CreateUfoBig()` до возврата.

### Pitfall 3: UFO-05 — бесконечный цикл wrap-around
**Что идёт не так:** UFO с wrap-around никогда не уходит — он телепортируется обратно.
**Почему:** `MoveSystem.WrapAxis()` не различает "первый" и "повторный" выход за границу.
**Как избежать:** Отдельная логика в Game.cs — подписаться на `Position.Changed` или в `Update` проверять "пересёк ли UFO стартовый край второй раз".
**Признак:** UFO ходит по кругу вечно.

### Pitfall 4: GunComponent.OnShooting не назначен для UFO
**Что идёт не так:** GunSystem вызывает `gun.OnShooting?.Invoke(gun)` но callback null → пуля не создаётся.
**Почему:** Для корабля `OnShooting` назначается в `Game.Start()`. Для UFO нужно то же самое при создании.
**Как избежать:** В `Game.SpawnUfoBig()` после `CreateUfoBig()` назначить `ufo.Gun.OnShooting = OnUfoGunShooting`.

### Pitfall 5: Баннер волны показывается при Game Over
**Что идёт не так:** ActionScheduler.Schedule(2.5f, HideWaveBanner) может сработать после Game Over.
**Почему:** ActionScheduler.CleanUp() вызывается в Model.CleanUp() → в Game.Restart(). Но если Game Over — Restart не вызывается, HideWaveBanner может быть в очереди.
**Как избежать:** Проверять `_isRunning` в HideWaveBanner, или добавить возможность отмены в ActionScheduler (или просто проверять null-guard в callback).

### Pitfall 6: Release UFO prefab — не зарегистрирован в пуле
**Что идёт не так:** `GameObjectPool.Release()` бросает `Exception("Unknown prefab id")`.
**Почему:** UfoBig.Prefab не передан в `_pool.Register()` в `EntitiesCatalog.Connect()`.
**Как избежать:** Добавить `RegisterPrefab(_configs.UfoBig.Prefab)` и `RegisterPrefab(_configs.Ufo.Prefab)` в `EntitiesCatalog.Connect()`.

---

## Code Examples

### UfoViewModel + UfoVisual (по паттерну AsteroidVisual)

```csharp
// View/UfoVisual.cs
// Источник: паттерн AsteroidVisual.cs + BulletVisual.cs
public class UfoViewModel : AbstractViewModel
{
    public ReactiveValue<Vector2> Position = new();
    public Action<Collision2D> OnCollision;
}

public class UfoVisual : AbstractWidgetView<UfoViewModel>, IEntityView
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    private int _prefabInstanceId;

    public int PrefabInstanceId => _prefabInstanceId;

    public void SetPrefabId(int id)
    {
        _prefabInstanceId = id;
    }

    protected override void OnConnected()
    {
        if (_spriteRenderer == null) { _spriteRenderer = GetComponent<SpriteRenderer>(); }
        ViewModel.Position.Connect(val =>
            transform.position = new Vector3(val.x, val.y, transform.position.z));
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        ViewModel?.OnCollision?.Invoke(col);
    }

    public new void Dispose()
    {
        base.Dispose();
    }
}
```

### Спаун UFO с края экрана

```csharp
// В Game.cs
private void SpawnUfoBig()
{
    var gameArea = _model.GameArea;
    // Выбрать случайный край: Left/Right (горизонтальное движение)
    var fromLeft = UnityEngine.Random.value > 0.5f;
    var x = fromLeft ? -gameArea.x / 2f : gameArea.x / 2f;
    var y = UnityEngine.Random.Range(-gameArea.y / 2f, gameArea.y / 2f);
    var pos = new Vector2(x, y);
    var dir = fromLeft ? Vector2.right : Vector2.left;

    var ufo = _catalog.CreateUfoBig(pos, dir * _configs.UfoBig.Speed);
    ufo.Gun.OnShooting = OnUfoGunShooting;
    // Сохранить стартовый край для UFO-05
    _activeUfo = ufo;
    _ufoStartedFromLeft = fromLeft;
    _ufoActive = true;

    // Подписка на коллизию
    if (_catalog.GetViewByModel(ufo) is UfoVisual ufoVisual)
    {
        ufoVisual.ViewModel.OnCollision = col => OnUfoCollided(ufo, col);
    }
}
```

### UFO-05: отслеживание пересечения экрана

```csharp
// В Game.Update() или через подписку на Position.Changed:
// Вызывается в Application.OnUpdate → _model.Update() — нужно проверять в Game.Update()
// Добавить метод Update(float dt) в Game, вызываемый из Application.OnUpdate

private void CheckUfoExit()
{
    if (_activeUfo == null || _activeUfo.IsDead()) { return; }
    var pos = _activeUfo.Move.Position.Value;
    var halfW = _model.GameArea.x / 2f;
    // UFO вышел за край противоположный старту
    var reachedOpposite = _ufoStartedFromLeft ? pos.x >= halfW - 0.5f : pos.x <= -halfW + 0.5f;
    if (reachedOpposite && _activeUfoWrapped)
    {
        // UFO прошёл через весь экран — уничтожить без очков
        _activeUfo.Kill();
        _ufoActive = false;
        _activeUfo = null;
    }
}
```

### Баннер волны в HudVisual

```csharp
// В HudVisual.cs — добавить поле:
[SerializeField] private TextMeshProUGUI _waveBannerText;

public void ShowWaveBanner(int wave)
{
    if (_waveBannerText == null) { return; }
    _waveBannerText.text = $"WAVE {wave}";
    _waveBannerText.gameObject.SetActive(true);
}

public void HideWaveBanner()
{
    if (_waveBannerText != null) { _waveBannerText.gameObject.SetActive(false); }
}
```

---

## State of the Art

| Старый подход | Текущий подход в проекте | Статус |
|--------------|--------------------------|--------|
| MonoBehaviour Update для таймеров | `ActionScheduler.Schedule()` | Принято в проекте |
| `FindObjectOfType` в Setup скриптах | `Resources.FindObjectsOfTypeAll<T>()` + фильтр по scene | Принято (см. Phase4Setup) |
| Прямой `Instantiate/Destroy` | `GameObjectPool.Get/Release` | Принято для всех сущностей |

---

## Environment Availability

Step 2.6: SKIPPED — фаза является чисто code/config изменением в существующем Unity проекте. Нет новых внешних зависимостей. Все инструменты (Unity Editor, MCP) уже работают по Phase 4.

---

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | MCP Unity (ручное тестирование через Play Mode) |
| Config file | нет автотестов — WebGL/Unity без headless runner |
| Quick run command | MCP: `compile` → `play` → визуальная проверка |
| Full suite command | MCP: `compile` → `play` → все критерии успеха вручную |

### Phase Requirements → Test Map
| Req ID | Поведение | Тип теста | Команда | Файл |
|--------|-----------|-----------|---------|------|
| UFO-01 | Large UFO появляется каждые 25–40 сек | manual smoke | Play Mode, ждать 40 сек | — |
| UFO-02 | Small UFO после 10 000 очков | manual smoke | Play Mode, набрать 10 000 | — |
| UFO-03 | Large UFO стреляет случайно | manual smoke | Play Mode, наблюдать | — |
| UFO-04 | UFO wrap-around | manual smoke | Play Mode, UFO дойти до края | — |
| UFO-05 | UFO исчезает после пересечения | manual smoke | Play Mode, ждать прохода | — |
| UFO-06 | Не более одного UFO | manual smoke | Play Mode, проверить в консоли | — |
| PROG-06 | Баннер Wave N при старте волны | manual smoke | Play Mode, старт волны | — |

### Wave 0 Gaps
- [ ] `Assets/Editor/Phase5Setup.cs` — создать UFO prefabs, UfoData assets, Wave Banner в HUD, зарегистрировать в GameData
- [ ] `Assets/Media/prefabs/ufo_big.prefab` — UFO Large prefab (SpriteRenderer + CircleCollider2D + UfoVisual)
- [ ] `Assets/Media/prefabs/ufo_small.prefab` — UFO Small prefab
- [ ] `Assets/Media/configs/UfoBigData.asset` — ScriptableObject UfoData для Large UFO
- [ ] `Assets/Media/configs/UfoSmallData.asset` — ScriptableObject UfoData для Small UFO

---

## Open Questions

1. **Спрайты UFO**
   - Что знаем: В `asteroids.png` есть спрайты всех игровых объектов. Спрайты нарезаются по имени (паттерн Phase 4).
   - Что неясно: Конкретные имена спрайтов UFO в атласе (нужно проверить sub-sprites в Editor).
   - Рекомендация: В `Phase5Setup.cs` использовать `LoadSprite("ufo_big")` / `LoadSprite("ufo_small")` с предупреждением если спрайт не найден — аналогично Phase4Setup.

2. **Направление движения UFO — только горизонтально или любое?**
   - Что знаем: Requirements говорят "движется прямолинейно с редкими сменами направления".
   - Рекомендация: Только горизонтальное движение (Left↔Right) для UFO-05 (простое обнаружение пересечения). Смены направления — по Y (вверх/вниз) через ActionScheduler.

3. **Update() в Game.cs — нужен ли?**
   - Что знаем: Сейчас `Application.OnUpdate` вызывает только `_model.Update()`. `Game` не имеет Update-метода.
   - Что неясно: UFO-05 требует проверки позиции каждый кадр.
   - Рекомендация: Добавить `Game.Update(float dt)`, вызываемый из `Application.OnUpdate` после `_model.Update()`. Это нужно и для UFO-05.

4. **UfoData.ShootDurationSec — назначение поля**
   - Что знаем: `UfoData.ShootDurationSec` определён в конфиге. Предположительно — интервал между выстрелами.
   - Рекомендация: Использовать как `ShootToComponent.ShootInterval` при создании UfoModel.

---

## Детальный план реализации по файлам

### Новые файлы
| Файл | Назначение |
|------|-----------|
| `Assets/Scripts/View/UfoVisual.cs` | UfoViewModel + UfoVisual (ViewModel, SpriteRenderer биндинг, коллизия) |
| `Assets/Editor/Phase5Setup.cs` | Editor-скрипт: UFO prefabs, UfoData assets, HUD wave banner, GameData обновление |

### Изменения существующих файлов
| Файл | Что меняется |
|------|-------------|
| `Assets/Scripts/Model/Model.cs` | `GroupCreator.Visit(UfoBigModel)` — реализовать (убрать пустое тело) |
| `Assets/Scripts/Model/Systems/MoveToSystem.cs` | Реализовать `UpdateNode` для Small UFO (MoveTo Target) |
| `Assets/Scripts/Model/Systems/ShootToSystem.cs` | Реализовать `UpdateNode` (таймер + OnShoot callback) |
| `Assets/Scripts/Model/Components/ShootToComponent.cs` | Добавить `Action<UfoModel> OnShoot` |
| `Assets/Scripts/Application/EntitiesCatalog.cs` | `CreateUfoBig()`, `CreateUfoSmall()`, регистрация prefabs в `Connect()`, `Release()` для UfoVisual |
| `Assets/Scripts/Application/Game.cs` | `ScheduleUfoSpawn()`, `SpawnUfoBig()`, `SpawnUfoSmall()`, `OnUfoCollided()`, `OnUfoGunShooting()`, `CheckUfoExit()`, `Game.Update(dt)`, `ShowWaveBanner()`, UFO-06 guard |
| `Assets/Scripts/Application/Application.cs` | Вызвать `_game.Update(dt)` в `OnUpdate()`, передавать wave banner callbacks |
| `Assets/Scripts/View/HudVisual.cs` | `ShowWaveBanner(int)`, `HideWaveBanner()`, поле `_waveBannerText` |
| `Assets/Scripts/Application/Screens/GameScreen.cs` | Прокинуть `ShowWaveBanner` / `HideWaveBanner` от Application к HudVisual |

---

## Sources

### Primary (HIGH confidence)
- Прямой анализ кодовой базы: `Game.cs`, `EntitiesCatalog.cs`, `Model.cs`, `UfoBigModel.cs`, `MoveToSystem.cs`, `ShootToSystem.cs`, `GunSystem.cs`, `ActionScheduler.cs`, `GameObjectPool.cs`, `HudVisual.cs`, `AsteroidVisual.cs`, `Phase4Setup.cs` — все файлы прочитаны напрямую
- `REQUIREMENTS.md` — требования UFO-01..06, PROG-06
- `.planning/STATE.md` — решения предыдущих фаз

### Secondary (MEDIUM confidence)
- Паттерны Unity 2022.3 Editor Setup (SerializedObject, PrefabUtility) — на основе Phase4Setup.cs, подтверждено кодом
- Unity UI anchor/pivot паттерны — `.claude/skills/unity-ui/SKILL.md`

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — весь стек взят из существующего кода
- Architecture: HIGH — паттерны CreateAsteroid/AsteroidVisual применены напрямую
- Pitfalls: HIGH — выведены из анализа существующих ошибок в quick tasks и комментариев в коде
- Open Questions: MEDIUM — требуют проверки в Unity Editor (имена спрайтов)

**Research date:** 2026-03-28
**Valid until:** 2026-04-28 (стабильная кодовая база, нет внешних зависимостей)
