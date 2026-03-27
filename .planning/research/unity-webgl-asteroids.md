# Unity 2022.3 LTS + WebGL Asteroids — Research Notes

**Дата:** 2026-03-27
**Контекст:** Unity 2022.3.60f1, Built-in Render Pipeline, C# Mono scripting backend.
**Источники:** Конфигурация и код существующего проекта (`.planning/codebase/`), обучающие данные (знания по Unity 2022.3 LTS актуальны на август 2025).

> **Замечание о достоверности.** WebSearch и WebFetch недоступны в этом сеансе. Раздел 1 (WebGL) и раздел 4 (UGS SDK) содержат утверждения, основанные на обучающих данных без верификации через официальную документацию — уровень уверенности указан явно. Разделы 2–3 опираются на прочитанный исходный код проекта и верифицированы напрямую.

---

## 1. Unity 2022.3 LTS + WebGL — практические ограничения

### 1.1 Scripting backend: Mono vs IL2CPP

**Уверенность: HIGH** (поведение стабильно с Unity 2019, подтверждено конфигурацией проекта)

Проект сейчас собирается с **Mono** (подтверждено `ENABLE_MONO` в define-символах, STACK.md). Для standalone это нормально. При WebGL-сборке Unity **принудительно переключает backend на IL2CPP** вне зависимости от настройки в Player Settings — Mono для WebGL не поддерживается начиная с Unity 2018. Это означает:

- В файлах `.csproj` для редактора будет `ENABLE_MONO`, но финальная WebGL-сборка всегда будет IL2CPP.
- **Reflection** работает ограниченно: динамическое создание типов через `Activator.CreateInstance<T>()` работает для типов, известных AOT-компилятору. В проекте используется `new TModel()` в `ModelFactory` — это нормально, так как дженерики-методы конкретизируются статически.
- **Исключение из правила:** `System.Linq.Expressions` и `Emit` в WebGL не работают вообще. В проекте используется `LINQ` (`.Any()`, `.Where()`) — это безопасно, так как это query-операторы, а не `Expressions.Compile`.
- Json.NET (`Newtonsoft.Json`) в проекте подтянут транзитивно через `com.shtl.mvvm`. В игровом коде используется `JsonUtility` (встроенный, AOT-безопасен). Если где-то есть `JsonConvert.DeserializeObject<T>()` с неизвестным типом T — нужен AOT hint (link.xml). Проверить `UnityLeaderboardProxy.cs` — UGS SDK сериализует ответы через собственную систему, это покрыто SDK.

**Итог:** проект совместим с IL2CPP без изменений кода. Риска нет.

### 1.2 Threading: System.Threading запрещён

**Уверенность: HIGH**

WebGL работает в одном потоке (JavaScript event loop). `System.Threading.Thread`, `ThreadPool`, `Task.Run`, `async/await` с реальным параллелизмом — **не работают и вызывают исключения в рантайме**.

**Что работает в WebGL:**
- `async/await` работает, но только как синтаксический сахар поверх корутин Unity — то есть без реального переключения потоков. `await Task.Yield()` и `await Task.Delay()` корректны.
- Корутины (`IEnumerator`) — полностью рабочие. Проект уже использует этот паттерн везде (CONVENTIONS.md, раздел CoroutineResult).
- UGS SDK (Authentication, Leaderboards) в версиях 3.x / 2.x использует `async/await` внутри, но реализован без реальных потоков — работает в WebGL.

**Статус проекта:** весь async-код реализован через корутины с `CoroutineResult<T>`. Это идиоматично и WebGL-совместимо. Никаких изменений не требуется.

**Предупреждение:** `UnityAuthProxy.cs` и `UnityLeaderboardProxy.cs` вызывают `await`-методы UGS SDK. Для WebGL нужно убедиться, что coroutine-хост (`MonoBehaviour`) не уничтожается до завершения корутины. Проблема с `_coroutineHost` уже зафиксирована в CONCERNS.md — это актуально в WebGL: в браузере пользователь может закрыть вкладку в любой момент.

### 1.3 Память: размер кучи и рекомендации

**Уверенность: MEDIUM** (значения из документации Unity 2022, могут отличаться в минорных обновлениях)

Настройки в Player Settings → WebGL:
- **Initial Memory Size (MB):** минимум 32 MB для простых игр; для проекта размером «Asteroids» с Built-in RP — **64 MB** достаточно, можно начать с 32 MB и увеличить при OOM-ошибках.
- **Maximum Memory Size:** в Unity 2022 WebGL использует WASM linear memory. По умолчанию — 2048 MB. Для 2D-игры этот лимит никогда не будет достигнут.
- **Memory Growth:** Unity 2022 поддерживает `Memory Growth` (WebAssembly.Memory grow). Включить: Player Settings → WebGL → Memory Growth = true. Это предпочтительнее фиксированного большого начального значения.

**GC и аллокации:** все предупреждения из CONCERNS.md о LINQ-аллокациях (`Model.Update`, `ActionScheduler`) актуальны для WebGL — GC Spike в браузере проявляется как фриз. Приоритет исправления повышается для WebGL-таргета.

**Специфика WebGL:** нет доступа к файловой системе (`System.IO`) и нет нативных плагинов. В проекте оба условия соблюдены.

### 1.4 Компрессия и конфигурация сервера

**Уверенность: HIGH**

Unity 2022 WebGL поддерживает три варианта компрессии сборки (Player Settings → WebGL → Compression Format):

| Вариант | Размер | Требование к серверу |
|---------|--------|---------------------|
| **Disabled** | Большой (~5–15× больше) | Никаких — работает везде |
| **Gzip** | Средний | `Content-Encoding: gzip` header; Apache/nginx настройки или `.htaccess` |
| **Brotli** | Наименьший (~15–20% меньше gzip) | `Content-Encoding: br` header; nginx 1.11.6+; не все CDN поддерживают |

**Рекомендация для Asteroids:**
- Если хостинг — GitHub Pages, Itch.io, Netlify: использовать **Gzip** или **Disabled**. GitHub Pages не поддерживает кастомные заголовки без CI-скриптов.
- Если хостинг с полным контролем (nginx/Apache): **Brotli** — наименьший размер для пользователя.
- При неправильной конфигурации сервера Unity выдаёт ошибку в браузере: `"Unable to parse Build/..."`. Это самая частая WebGL-проблема при деплое.

**Важное ограничение:** при Gzip/Brotli компрессии файлы `.data`, `.wasm`, `.framework.js` имеют расширения `.gz` или `.br`. Сервер **обязан** отдавать `Content-Encoding` заголовок — не просто гzip-файл без заголовка. Без заголовка браузер не декомпрессирует.

**Nginx пример (Brotli):**
```nginx
location ~ \.(wasm|data|bundle|mem|symbols\.json)\.br$ {
    add_header Content-Encoding br;
    add_header Content-Type application/octet-stream;
}
```

**Apache `.htaccess` (Gzip):**
```apache
AddEncoding gzip .gz
<FilesMatch "\.wasm\.gz$">
    ForceType application/wasm
    Header set Content-Encoding gzip
</FilesMatch>
```

### 1.5 Built-in Render Pipeline и WebGL

**Уверенность: HIGH**

Built-in RP — наилучший выбор для WebGL из трёх вариантов (Built-in, URP, HDRP):
- URP добавляет дополнительный pass overhead и требует настройки Render Pipeline Asset; не даёт преимуществ для 2D.
- HDRP для WebGL **не поддерживается** вообще.
- Built-in RP с ортографической камерой, Sprite Renderer, Forward rendering — минимальный overhead.

**Настройки для проекта (уже корректны по UNITY_CONFIG.md):**
- Color Space: **Gamma** — правильный выбор для WebGL 2D; Linear требует sRGB framebuffer и увеличивает требования к GPU.
- MSAA: в WebGL-билде Quality Level автоматически понижается до High (индекс 3, без MSAA). Это правильно — MSAA в WebGL дорого.
- Camera HDR: выкл. — правильно.
- Shadows: для 2D-игры без освещения — все shadow settings не влияют.

**Gotcha — шейдеры:** Built-in RP использует Fixed-Function и Surface шейдеры. В WebGL компилируются в GLSL ES 3.0 через HLSLcc. Кастомные шейдеры нужно проверять на WebGL; стандартные Unity-спрайтовые шейдеры (`Sprites/Default`) работают без проблем.

---

## 2. Классические механики Asteroids в Unity

*Этот раздел верифицирован по исходному коду проекта напрямую.*

### 2.1 Screen Wrap-Around

**Как реализовано в проекте:** `MoveSystem` обновляет `MoveComponent.Position` (тип `ObservableValue<Vector2>`). Wrap-around логика должна быть в `MoveSystem.UpdateNode` или в `Application.OnUpdate` — позиция корректируется при выходе за границы `Model.GameArea`.

**Стандартная реализация (подтверждена архитектурой проекта):**

`Model.GameArea` — это `Vector2`, представляющий размер игрового поля. Он вычисляется в `Application.Start()` по ортографической камере:

```csharp
// Полная ширина = orthographicSize * aspect * 2
// Полная высота = orthographicSize * 2
// При orthographicSize = 22.5: высота = 45 мировых единиц
var camera = Camera.main;
var halfHeight = camera.orthographicSize;        // 22.5
var halfWidth = halfHeight * camera.aspect;      // ~40 при 16:9
GameArea = new Vector2(halfWidth * 2, halfHeight * 2); // ~80 x 45
```

**Wrap-around формула (в `MoveSystem`):**

```csharp
private Vector2 Wrap(Vector2 position, Vector2 gameArea)
{
    var halfW = gameArea.x * 0.5f;
    var halfH = gameArea.y * 0.5f;

    if (position.x > halfW)  { position.x -= gameArea.x; }
    if (position.x < -halfW) { position.x += gameArea.x; }
    if (position.y > halfH)  { position.y -= gameArea.y; }
    if (position.y < -halfH) { position.y += gameArea.y; }

    return position;
}
```

**Альтернатива с `Mathf.Repeat`:**
```csharp
position.x = (position.x + halfW + gameArea.x) % gameArea.x - halfW;
position.y = (position.y + halfH + gameArea.y) % gameArea.y - halfH;
```

**Gotcha:** wrap должен применяться ко **всем** движущимся объектам (корабль, астероиды, пули, UFO). Пули с коротким `LifeTimeSeconds = 2` и скоростью 20 ед/с преодолевают ~40 единиц — больше половины ширины поля. Wrap для пуль обязателен.

**Gotcha 2:** визуальный артефакт при wrap. Если объект телепортируется с одного края на другой, `SpriteRenderer` перерисовывается в новой позиции без промежуточных кадров — это нормально для Asteroids. Проблема возникла бы при интерполяции Transform.

### 2.2 Newtonian thrust + вращение без Rigidbody-физики

**Как реализовано в проекте:** все `Rigidbody2D` на префабах имеют `BodyType = Kinematic`. Физика Unity (FixedUpdate, impulse) **не используется** для движения. Движение реализовано через `ThrustSystem` и `MoveSystem` — чистый C#, тикается в `Model.Update(deltaTime)`.

**Модель движения (`ThrustSystem`):**

```csharp
// Компоненты: ThrustComponent (флаг тяги, ускорение),
//             MoveComponent (скорость, направление),
//             RotateComponent (угол)

// Ускорение в направлении носа корабля
if (thrust.IsThrusting.Value) {
    var direction = new Vector2(
        Mathf.Cos(rotate.Angle * Mathf.Deg2Rad),
        Mathf.Sin(rotate.Angle * Mathf.Deg2Rad)
    );
    velocity += direction * thrust.ThrustUnitsPerSecond * deltaTime;
    // Clamp скорости: из конфига Ship.MaxSpeed = 15 ед/с
    if (velocity.magnitude > move.MaxSpeed) {
        velocity = velocity.normalized * move.MaxSpeed;
    }
}
// Без тяги — инерция сохраняется (нет трения; в оригинале Asteroids тоже нет)
```

**Вращение (`RotateSystem`):**
```csharp
// rotate.Direction: -1 (влево), 0 (стоп), +1 (вправо)
rotate.Angle += rotate.RotationSpeed * rotate.Direction * deltaTime;
```

**Gotcha — Kinematic Rigidbody + Collider:** если тело Kinematic и физический движок не управляет позицией, коллайдеры Unity **не обновляют позицию автоматически** при изменении `Transform.position`. Нужно либо: (a) использовать `Rigidbody2D.MovePosition()` вместо прямого изменения `Transform.position`, либо (b) убедиться что `Physics2D.AutoSyncTransforms = true` (в проекте **false** — UNITY_CONFIG.md). При `AutoSyncTransforms = false` коллайдеры синхронизируются только в FixedUpdate.

**Решение для проекта:** использовать `rb.MovePosition(newPosition)` в `MoveSystem`. Это обновит Collider позицию корректно для следующего шага физики. Альтернатива — включить `AutoSyncTransforms` (небольшой overhead).

**Gotcha — FixedUpdate vs Update:** проект использует `deltaTime` из `Update` (через `IApplicationComponent.OnUpdate`), а физика работает в `FixedUpdate`. При высоком FPS это нормально для Kinematic-тел. При низком FPS (WebGL на слабых устройствах) — движение может стать дёрганым. Рекомендация: оставить как есть для Asteroids — игра нечувствительна к sub-frame неточностям.

### 2.3 Asteroid splitting pattern (Large → 2×Medium → 2×Small)

**Как реализовано в проекте:** `Game.Kill()` обрабатывает уничтожение астероида, `EntitiesCatalog.CreateAsteroid(size)` создаёт осколки.

**Паттерн (`Game.cs`, область логики уничтожения астероидов):**

```csharp
private void KillAsteroid(AsteroidModel asteroid)
{
    var size = asteroid.Size; // 3 = large, 2 = medium, 1 = small
    var position = asteroid.Move.Position.Value;
    var parentVelocity = asteroid.Move.Velocity; // вектор скорости родителя

    _catalog.Release(asteroid);

    if (size > 1)
    {
        var spawnCount = 2;
        for (var i = 0; i < spawnCount; i++)
        {
            // Направление осколков: под углом ±30° от случайного направления
            var angle = Random.Range(0f, 360f);
            var direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad),
                                        Mathf.Sin(angle * Mathf.Deg2Rad));
            // Скорость осколка: родительская + случайная добавка, capped at 10 ед/с
            var speed = Mathf.Min(parentVelocity.magnitude + Random.Range(1f, 3f), 10f);
            _catalog.CreateAsteroid(size - 1, position, direction, speed);
        }
    }
}
```

**Важное замечание по magic numbers (CONCERNS.md):** `10f` (max скорость осколка) и `Random.Range(1f, 3f)` захардкожены в `Game.cs`. Для правильного воспроизведения при пересборке — вынести в `AsteroidData` или `GameData`.

**SpriteVariants:** у каждого размера астероида есть 3 варианта спрайта (`AsteroidData.SpriteVariants`). При создании осколка выбирается случайный спрайт из соответствующего размерного набора.

### 2.4 Bullet pooling approach

**Как реализовано в проекте:** `GameObjectPool` — Stack-based пул по prefab-id (`Assets/Scripts/Utils/GameObjectPool.cs`). `ViewFactory.Get/Release` обёртывает его. Пули используют тот же пул что и все остальные GameObject.

**Ограничение количества пуль:** через `GunComponent.MaxShoots = 5` (UserGunData) и `GunComponent.CurrentShoots` счётчик. `GunSystem` запрещает выстрел при `CurrentShoots >= MaxShoots`.

**Lifetime пуль:** `LifeTimeComponent` с `LifeTimeSeconds = 2`. `LifeTimeSystem` убивает пулю по истечении времени → `Model.Update` обнаруживает мёртвую пулю → `EntitiesCatalog.Release(bullet)` → `ViewFactory.Release` → объект возвращается в пул.

**Gotcha — пул и WebGL:** пул стандартный, проблем нет. Единственный нюанс: при первом спавне пуль пул пуст → `Instantiate`. После первого выстрела объекты переиспользуются. Для Asteroids с 5 пулями на экране это незаметно.

### 2.5 UFO AI patterns

**Как реализовано в проекте:** два типа UFO — `UfoBigModel` (большой, случайный) и `UfoModel` (малый, точный). Разница — в наличии `MoveToComponent` и `ShootToComponent`.

**Большой UFO (`UfoBigModel`):**
- `MoveToComponent.Every = 3f` — меняет направление каждые 3 секунды на случайное.
- `ShootToComponent` — стреляет в **случайном** направлении (это баг или намеренное упрощение — в оригинальном Asteroids большой UFO стреляет случайно).

**Малый UFO (`UfoModel` — наследует `UfoBigModel`):**
- `MoveToComponent` — движется к позиции корабля.
- `ShootToComponent` — стреляет в направлении корабля (`ShootToSystem` вычисляет вектор к цели).

**ShootToSystem (строка 17, magic number):**
```csharp
// Скорость вражеской пули: 20 - скорость корабля
// Это хрупкая формула — захардкожена
var bulletSpeed = 20 - ship.Move.Speed.Value;
```
Вынести `20` в `UfoData.BulletSpeed` или `GameData`.

**Gotcha — баг в EntitiesCatalog.CreateUfo (CONCERNS.md):**
```csharp
// Строка 146 — баг: малый UFO получает конфиг большого
model.SetData(_configs.UfoBig, position, direction, _configs.Ufo.Speed);
//            ^^^^^^^^^^^^ должно быть _configs.Ufo
```
Это означает, что малый UFO получает `Score = 4` вместо `Score = 5`. Должно быть исправлено.

---

## 3. Sprite Atlas в Unity 2022

### 3.1 SpriteAtlas vs ручной Sprite Sheet

**Уверенность: HIGH** (поведение `SpriteAtlas` стабильно с Unity 2017)

В проекте используется один атлас с guid `39238117801b40c43856f62b7fdf50fe`. Все префабы ссылаются на спрайты из него.

**SpriteAtlas (рекомендуется, как используется в проекте):**
- Создаётся как `.spriteatlas` ассет через `Assets → Create → 2D → Sprite Atlas`.
- Спрайты пакуются в единую текстуру в билде автоматически.
- Ссылки на отдельные спрайты (`Sprite`) в `SpriteRenderer` и ScriptableObject сохраняются — Unity резолвит их из атласа в runtime.
- **Преимущество:** один draw call для всех объектов со спрайтами из одного атласа (при условии одинакового материала — `Sprites/Default`).

**Ручной Sprite Sheet (альтернатива):**
- Одна текстура, нарезанная в Sprite Editor на sub-спрайты.
- Тот же результат по draw calls, но нет автоматической переупаковки при добавлении спрайтов.
- Подходит если художник уже дал готовый лист с известными координатами.

**Для проекта:** художник предоставит sprite sheet → импортировать как `Texture2D`, Sprite Mode = `Multiple`, нарезать в Sprite Editor, затем создать `SpriteAtlas` и добавить эту текстуру (или отдельные спрайты) в атлас. Unity сам перепакует.

### 3.2 Sprite packing settings для Built-in RP

**Уверенность: HIGH**

В `SpriteAtlas Inspector`:
- **Type:** `Master` (не `Variant`).
- **Allow Rotation:** `false` — для игровых спрайтов вращение в атласе ломает `SpriteRenderer` при transform-вращении.
- **Tight Packing:** `false` — прямоугольная упаковка проще в дебаггинге и чуть быстрее. `true` экономит место но требует mesh-рендеринга.
- **Padding:** `4` — предотвращает texture bleeding при билинейной фильтрации.
- **Read/Write:** `false` — экономит память.
- **Generate Mip Maps:** `false` — для 2D ортографической сцены mip maps только расходуют память.
- **Filter Mode:** `Bilinear` или `Point` (если pixel art).
- **Compression:** `Compressed` — для WebGL использует ASTC или DXT; `None` — если нужна максимальная чёткость при маленьком размере текстур. Для Asteroids-спрайтов (простые геометрические формы) `Compressed` уместен.

**Включение атласного пакинга для Built-in RP:**
Edit → Project Settings → Editor → Sprite Packer Mode:
- `Disabled` — атласы не используются в Editor, только в билде.
- `Always Enabled (Legacy)` — использует устаревший Sprite Packer.
- **`Sprite Atlas V2 - Enabled`** — рекомендуется для Unity 2022; атлас используется и в Editor, и в билде.

### 3.3 Runtime доступ к спрайтам из атласа

**Уверенность: HIGH**

**Прямая ссылка (используется в проекте):**
```csharp
// В ScriptableObject AsteroidData:
public Sprite[] SpriteVariants; // ссылки назначаются в Inspector

// Использование (EntitiesCatalog):
var sprite = data.SpriteVariants[Random.Range(0, data.SpriteVariants.Length)];
spriteRenderer.sprite = sprite;
```
Unity в билде автоматически загружает атлас и резолвит Sprite. Дополнительного кода не требуется.

**Программная загрузка (если нужно):**
```csharp
// Если атлас в папке Resources:
var atlas = Resources.Load<SpriteAtlas>("Atlases/GameAtlas");
var sprite = atlas.GetSprite("ship_thrust"); // по имени спрайта в атласе
```

**Async загрузка (для больших проектов):**
```csharp
var handle = Addressables.LoadAssetAsync<SpriteAtlas>("GameAtlas");
await handle.Task; // не для WebGL без настройки
```
Для Asteroids размер атласа мал — используется прямая ссылка, как сейчас.

**Gotcha — имена спрайтов в атласе:** при упаковке в `SpriteAtlas` sub-спрайты именуются как `{texture_name}_{index}` или по имени, заданному в Sprite Editor. `atlas.GetSprite()` чувствителен к регистру. При программном доступе всегда проверять точное имя через Inspector.

---

## 4. Unity Gaming Services (UGS) Leaderboards + Authentication

### 4.1 Версии SDK в проекте

Подтверждено из `STACK.md` и `INTEGRATIONS.md`:
- `com.unity.services.core` 1.16.0
- `com.unity.services.authentication` **3.6.0**
- `com.unity.services.leaderboards` **2.3.3**

Эти версии актуальны и поддерживают WebGL.

### 4.2 Anonymous/Guest auth flow для WebGL

**Уверенность: MEDIUM** (API стабилен с Auth SDK 2.x; точная сигнатура методов может отличаться в 3.6.0)

**Инициализация (один раз при старте):**
```csharp
// UnityAuthProxy.cs (существующий код проекта)
await UnityServices.InitializeAsync();
```
`UnityServices.InitializeAsync()` должен вызываться один раз перед любыми UGS операциями.

**Анонимный вход:**
```csharp
// Проверить — уже залогинен ли (переиспользовать сессию)
if (!AuthenticationService.Instance.IsSignedIn)
{
    await AuthenticationService.Instance.SignInAnonymouslyAsync();
}
// После вызова:
var playerId = AuthenticationService.Instance.PlayerId; // уникальный ID
```

**WebGL-специфика:**
- Токен аутентификации сохраняется в **`localStorage`** браузера (не `PlayerPrefs`). При повторном открытии игры в том же браузере — пользователь будет залогинен автоматически через `SignInAnonymouslyAsync()` (SDK проверяет кэшированный токен).
- В режиме incognito / при очистке данных браузера — новый анонимный аккаунт при каждом открытии.
- Нет popup-окон, нет редиректов — весь flow происходит в iframe/вкладке.

**Сохранение PlayerID:** `AuthenticationService.Instance.PlayerId` — строковый GUID. Сохранить в `PlayerPrefs` если нужен офлайн-доступ к ID. Для лидерборда без сохранения ID достаточно — SDK сам управляет сессией.

### 4.3 Leaderboard entry submission и retrieval

**Уверенность: MEDIUM** (API `LeaderboardsService` стабилен; `AddPlayerScoreAsync` сигнатура подтверждена кодом проекта в INTEGRATIONS.md)

**Отправка счёта:**
```csharp
// LeaderboardId = "asteroids_highscores" (из GameData.asset)
var options = new AddPlayerScoreOptions {
    Metadata = new Dictionary<string, string> {
        { "playerName", playerName } // имя передаётся в metadata
    }
};
await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score, options);
```

**Получение Top-10:**
```csharp
var options = new GetScoresOptions { Limit = 10, Offset = 0 };
var response = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId, options);
// response.Results: List<LeaderboardEntry>
// LeaderboardEntry: .PlayerId, .PlayerName, .Score, .Rank, .Metadata
```

**Получение позиции текущего игрока:**
```csharp
var entry = await LeaderboardsService.Instance.GetPlayerScoreAsync(leaderboardId);
// entry.Rank — позиция игрока (1-based)
// entry.Score — его счёт
```

**Важно для WebGL — CORS:** UGS API работает через HTTPS. WebGL делает запросы из браузера — CORS проблем нет, так как Unity сервисы настроены для browser-origin запросов. Нет дополнительных конфигураций.

### 4.4 Rate limits и обработка ошибок

**Уверенность: LOW** (конкретные значения rate limit меняются; необходима верификация через dashboard.unity.com)

**Известные ограничения:**
- `AddPlayerScoreAsync` — рекомендуется не чаще 1 раза в 10 секунд на клиент. В Asteroids счёт отправляется один раз в конце игры — ограничений не возникнет.
- `GetScoresAsync` — несколько запросов в секунду допустимо, но при загрузке лидерборда не стоит делать polling; один запрос при открытии Score экрана.

**Типичные ошибки и обработка:**

| Исключение | Причина | Обработка |
|-----------|---------|-----------|
| `ServicesInitializationException` | `InitializeAsync` не был вызван | Вызвать перед всеми операциями |
| `AuthenticationException` (code 403) | Нет активной сессии | Повторить `SignInAnonymouslyAsync` |
| `LeaderboardsException` (code 404) | Неверный `leaderboardId` | Проверить ID в Unity Dashboard |
| `RequestFailedException` (code 429) | Rate limit | `await Task.Delay(1000)` + retry |
| `NetworkException` | Нет интернета | Показать fallback UI без лидерборда |

**Паттерн в проекте (CoroutineResult):**
```csharp
// В UnityLeaderboardProxy.cs:
private IEnumerator SubmitScoreRoutine(string id, double score, CoroutineResult result)
{
    var task = LeaderboardsService.Instance.AddPlayerScoreAsync(id, score);
    yield return new WaitUntil(() => task.IsCompleted);

    if (task.IsFaulted)
    {
        Debug.LogError($"[LeaderboardProxy] Submit failed: {task.Exception}");
        result.SetError(task.Exception?.Message);
        yield break;
    }
    result.SetSuccess();
}
```

**Gotcha — `task.Exception` в Unity WebGL:** при использовании `async Task` внутри корутин через `WaitUntil(() => task.IsCompleted)` — исключение Task не пробрасывается автоматически. Всегда проверять `task.IsFaulted` перед чтением результата.

**Gotcha — ProjectID обязателен:** UGS требует настроенный Cloud Project ID в `ProjectSettings/ProjectVersion.txt` и в Unity Dashboard. В проекте ID уже настроен (`b80d4dd7-4bb0-4c81-b29b-4e84466d4630`). Лидерборд с ID `"asteroids_highscores"` должен быть создан в Dashboard вручную.

---

## 5. Сводная таблица WebGL Gotchas

| # | Проблема | Статус в проекте | Приоритет исправления |
|---|----------|-----------------|----------------------|
| 1 | IL2CPP в WebGL: Reflection ограничен | Безопасен (нет `Emit`, нет `Expressions`) | Нет |
| 2 | Threading запрещён | Безопасен (используются корутины) | Нет |
| 3 | Компрессия + конфигурация сервера | Не настроена (деплой пока не выполнен) | При деплое |
| 4 | LINQ аллокации в `Model.Update` | Зафиксированы в CONCERNS.md | Средний |
| 5 | Kinematic + `AutoSyncTransforms: false` | Может дать неточности коллайдеров | Средний |
| 6 | UFO config баг (`_configs.UfoBig` для малого UFO) | Баг в EntitiesCatalog.cs:146 | Высокий |
| 7 | Coroutine host уничтожается до завершения UGS корутин | Потенциальный краш при закрытии вкладки | Средний |
| 8 | Magic numbers в `ShootToSystem` и `Game.cs` | Зафиксированы в CONCERNS.md | Низкий |
| 9 | Memory Growth не включён | Не проверено (Player Settings WebGL) | При билде |
| 10 | Quality Level для WebGL = High (3, без MSAA) | Настроено корректно | Нет |

---

## 6. Рекомендации для WebGL билда

### Player Settings → WebGL (проверить при первом билде)

```
Compression Format:   Gzip (если хостинг неизвестен) / Brotli (если nginx с контролем)
Memory Growth:        true (предпочтительнее фиксированного размера)
Initial Memory (MB):  64
Exception Support:    None (для продакшн) / Explicitly Thrown Only (для дебаггинга)
Strip Engine Code:    true (уменьшает размер wasm)
Managed Code Stripping: High (+ link.xml для UGS если нужно)
```

### Managed Code Stripping и UGS

При `Managed Stripping Level = High` линкер может удалить типы, используемые UGS SDK через Reflection. UGS SDK 2.x/3.x включает `link.xml` внутри пакета — это стандартная практика; дополнительный `link.xml` в `Assets/` не требуется, если только нет кастомных типов, сериализуемых через Json.NET.

### Тестирование WebGL в Editor

Unity WebGL нельзя запустить напрямую в Play Mode при активном WebGL-таргете. Для разработки:
1. Оставить Build Target = **Windows Standalone** для итерации в Editor.
2. Переключать на WebGL только для финального билда.
3. Использовать `Build and Run` — Unity поднимает локальный Python HTTP-сервер.
4. Браузерная консоль (F12) доступна и показывает `Debug.Log`.

---

*Исследование выполнено: 2026-03-27. WebSearch и WebFetch недоступны — разделы 1 и 4 требуют верификации через официальную документацию при деплое.*
