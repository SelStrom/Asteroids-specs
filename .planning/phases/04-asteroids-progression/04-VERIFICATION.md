---
phase: 04-asteroids-progression
verified: 2026-03-28T14:00:00Z
status: human_needed
score: 5/5 must-haves verified
human_verification:
  - test: "Счёт в HUD обновляется в реальном времени при уничтожении астероидов"
    expected: "В левом верхнем углу HUD после уничтожения Large отображается +1, Medium +2, Small +3"
    why_human: "UAT-тест 4 (04-UAT.md) был пройден после закрытия gap в plan 04-05; REQUIREMENTS.md всё ещё показывает PROG-02 как [ ] — требует финального подтверждения человека"
  - test: "HUD показывает 3 иконки жизней при старте игры"
    expected: "Три миниатюрные иконки корабля видны в HUD под счётом после нажатия Play"
    why_human: "Поля _livesContainer и _lifeIconSprite назначены в сцене через Phase4Setup, но отображение иконок зависит от правильности anchor/pivot UI-элементов"
  - test: "Game Over экран появляется при потере последней жизни"
    expected: "GameOverScreen становится видимым, показывает Score: N и Best: N, кнопка Play Again активна"
    why_human: "Тесты 5–6 в 04-UAT.md помечены 'skipped — будут покрыты автотестами'; человеческая верификация не проводилась"
  - test: "Play Again перезапускает игру без перезапуска приложения"
    expected: "Счёт обнуляется до 0, волна 1, 3 жизни; High Score сохраняется между сессиями"
    why_human: "Тест 7 в 04-UAT.md помечен 'skipped'; Restart() логика реализована в коде, но конечное поведение не подтверждено человеком"
---

# Фаза 4: Asteroids & Progression — Отчёт о верификации

**Цель фазы:** Полный игровой цикл — три размера астероидов с логикой дробления, волновый спавн, счёт, жизни и экран Game Over.
**Верифицировано:** 2026-03-28T14:00:00Z
**Статус:** human_needed
**Повторная верификация:** Нет — первичная верификация

---

## Достижение цели

### Наблюдаемые истины

| №  | Истина | Статус | Доказательство |
|----|--------|--------|----------------|
| 1  | Уничтожение Large порождает 2 Medium; Medium → 2 Small; Small исчезает | ✓ VERIFIED | `SpawnFragments()` в Game.cs: `childSize = asteroid.Size - 1`, цикл `for (i=0; i<2)` при `childSize > 0` |
| 2  | Первая волна = 4 Large; каждая следующая +1 (макс. 12); вдали от корабля | ✓ VERIFIED | `StartWave()`: `Mathf.Min(3 + _waveNumber, 12)`; `GetRandomPositionOutsideRadius(shipPos, _configs.SpawnAllowedRadius, gameArea)` |
| 3  | Осколки быстрее родителя (AST-06): Small(4) > Medium(3) > Large(2) | ✓ VERIFIED | `SpawnFragments()`: `childSpeed = childSize == 1 ? 4f : 3f`; Large спаун 2f в `StartWave()` |
| 4  | Счёт растёт при уничтожении по DATA_SCHEMA (Big=1, Med=2, Sml=3) | ✓ VERIFIED | `OnAsteroidCollided()`: `_model.Score += data.Score`; configs: AsteroidBigData.Score=1, Medium=2, Small=3 |
| 5  | Игрок начинает с 3 жизней; при 0 вызывается _onGameOver | ✓ VERIFIED | `Start()`: `_lives = 3`; `OnEntityDestroyed()`: `_lives--`, `if (_lives <= 0) { _isRunning = false; _onGameOver?.Invoke(); }` |
| 6  | Экстра-жизнь каждые 10 000 очков (макс. 6) | ✓ VERIFIED | `OnAsteroidCollided()`: `while (_model.Score >= _nextBonusLifeScore && _lives < 6) { _lives++; _nextBonusLifeScore += 10000; }` |
| 7  | Play Again вызывает Restart() — сброс волны/счёта/жизней с сохранением HighScore | ✓ VERIFIED | `Restart()`: `Stop()` → `_model.CleanUp()` → `_catalog.Reset()` → `Start()`; `_highScore` не сбрасывается |
| 8  | HUD отображает Score, HighScore, иконки жизней | ✓ VERIFIED | `HudVisual.UpdateHud()` реализован; `_scoreText`, `_highScoreText`, `_livesContainer`, `_lifeIconSprite` назначены в сцене (строки 2489–2492 Main.unity) |
| 9  | Game Over экран показывает Score/HighScore, Play Again активна, Submit/Leaderboard disabled | ✓ VERIFIED | `GameOverView.OnConnected()`: `_submitScoreButton.interactable = false`; `GameOverScreen.Show()` передаёт `finalScore` и `highScore` |
| 10 | Астероиды телепортируются при выходе за границы (AST-07) | ✓ VERIFIED | `MoveSystem.Update()`: `WrapAxis()` применяется к позиции — все сущности в MoveSystem получают wrap-around, включая астероиды |
| 11 | Пуля уничтожается при попадании в астероид | ✓ VERIFIED | `OnAsteroidCollided()`: `if (hitModel is BulletModel hitBullet) { hitBullet.Kill(); }` |

**Счёт: 11/11 истин подтверждено статически**

---

### Обязательные артефакты

| Артефакт | Ожидается | Статус | Детали |
|----------|-----------|--------|--------|
| `Assets/Scripts/Model/Entities/AsteroidModel.cs` | AsteroidModel с AngularSpeed, Size | ✓ VERIFIED | Содержит `public float AngularSpeed { get; set; }`, `public int Size { get; set; }` |
| `Assets/Scripts/View/AsteroidVisual.cs` | AsteroidViewModel + AsteroidVisual MonoBehaviour | ✓ VERIFIED | Оба класса присутствуют, `OnCollisionEnter2D`, `Update()` с `Transform.Rotate`, `IEntityView` |
| `Assets/Scripts/Application/EntitiesCatalog.cs` | CreateAsteroid, Reset | ✓ VERIFIED | `CreateAsteroid(AsteroidData, Vector2, Vector2)` и `Reset()` реализованы |
| `Assets/Scripts/Application/Game.cs` | StartWave, SpawnFragments, Lives, Restart | ✓ VERIFIED | Все методы и поля присутствуют |
| `Assets/Scripts/View/HudVisual.cs` | UpdateHud(score, lives, highScore) + иконки жизней | ✓ VERIFIED | `UpdateHud()` и `UpdateLivesIcons()` реализованы, 20×20px иконки |
| `Assets/Scripts/Application/Screens/GameScreen.cs` | UpdateHud(score, lives, highScore) | ✓ VERIFIED | Делегирует в `_hudVisual.UpdateHud()` |
| `Assets/Scripts/View/GameOverView.cs` | GameOverViewModel + GameOverView с 3 кнопками | ✓ VERIFIED | Кнопки Submit/Leaderboard disabled, Play Again активна |
| `Assets/Scripts/Application/Screens/GameOverScreen.cs` | Show(finalScore, highScore), Hide() | ✓ VERIFIED | Оба метода реализованы, `_view.gameObject.SetActive(true/false)` |
| `Assets/Scripts/Application/Application.cs` | Wire-up с GameOverScreen, OnScoreChanged | ✓ VERIFIED | `OnGameOver()`, `OnPlayAgain()`, `OnScoreChanged()` реализованы |
| `Assets/Scripts/Application/ApplicationEntry.cs` | _gameOverView, _gameOverGo SerializeField | ✓ VERIFIED | Оба поля присутствуют; Connect передаёт их в Application |
| `Assets/Media/prefabs/asteroid_big.prefab` | Prefab с AsteroidVisual, Layer=8, Rigidbody2D Kinematic | ✓ VERIFIED | `m_Layer: 8`, `m_BodyType: 1`, `m_GravityScale: 0`, GUID AsteroidVisual = `a7b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7` |
| `Assets/Media/prefabs/asteroid_medium.prefab` | Prefab среднего астероида | ✓ VERIFIED | Файл существует |
| `Assets/Media/prefabs/asteroid_small.prefab` | Prefab малого астероида | ✓ VERIFIED | Файл существует |
| `Assets/Media/configs/AsteroidBigData.asset` | Score=1, Prefab ссылка, SpriteVariants[3] | ✓ VERIFIED | Score=1, Prefab GUID присвоен, 3 спрайта в SpriteVariants |
| `Assets/Media/configs/AsteroidMediumData.asset` | Score=2, SpriteVariants[3] | ✓ VERIFIED | Score=2, 3 спрайта (fileIDs 5/6/7) |
| `Assets/Media/configs/AsteroidSmallData.asset` | Score=3, SpriteVariants[3] | ✓ VERIFIED | Score=3, 3 спрайта (используются medium-варианты per D-20) |

---

### Верификация ключевых связей

| От | До | Через | Статус | Детали |
|----|-----|-------|--------|--------|
| `AsteroidVisual.OnCollisionEnter2D` | `Game.OnAsteroidCollided` | `AsteroidViewModel.OnCollision` callback | ✓ WIRED | `BindAsteroidCollision()`: `av.ViewModel.OnCollision = col => OnAsteroidCollided(asteroid, col)` |
| `Game.OnEntityDestroyed(AsteroidModel)` | `EntitiesCatalog.CreateAsteroid` | `SpawnFragments()` | ✓ WIRED | `SpawnFragments()` вызывает `_catalog.CreateAsteroid(childData, parentPos, vel)` |
| `Game.StartWave` | `EntitiesCatalog.CreateAsteroid` | цикл for | ✓ WIRED | `for (var i = 0; i < asteroidCount; i++) { var asteroid = _catalog.CreateAsteroid(...) }` |
| `Game.Restart` | `EntitiesCatalog.Reset` | прямой вызов | ✓ WIRED | `_catalog.Reset()` вызывается между `_model.CleanUp()` и `Start()` |
| `ApplicationEntry._gameOverView` | `GameOverScreen.Connect` | `Application.Connect` | ✓ WIRED | `_gameOverScreen.Connect(_gameOverView, OnPlayAgain)` в `Application.Connect()` |
| `Game._onGameOver` | `GameOverScreen.Show(score, highScore)` | `Application.OnGameOver` | ✓ WIRED | `_gameOverScreen.Show(_game.Score, _game.HighScore)` |
| `GameOverView.OnPlayAgainClicked` | `Game.Restart()` | `Application.OnPlayAgain` | ✓ WIRED | `_game.Restart()` в `OnPlayAgain()` |
| `Application.OnScoreChanged` | `GameScreen.UpdateHud` | `_onScoreChanged` callback | ✓ WIRED | `Game.Connect(..., OnScoreChanged)`; `OnScoreChanged(score, lives)` → `_gameScreen.UpdateHud(score, lives, _game.HighScore)` |
| `HudVisual._scoreText` | сцена | `[SerializeField]` Inspector | ✓ WIRED | Main.unity строка 2489: `_scoreText: {fileID: 1463065024}` |
| `HudVisual._livesContainer` | сцена | `[SerializeField]` Inspector | ✓ WIRED | Main.unity строка 2491: `_livesContainer: {fileID: 1765027187}` |
| `ModelFactory.Create<AsteroidModel>` | `Model.AddEntity` | `_modelFactory.Create<T>()` | ✓ WIRED | `ModelFactory.cs:15`: `_model.AddEntity(model)` вызывается при каждом `Create<T>()` |

---

### Трассировка потоков данных (Level 4)

| Артефакт | Переменная | Источник | Реальные данные | Статус |
|----------|-----------|---------|-----------------|--------|
| `HudVisual._scoreText` | `score` | `Game._model.Score` → `_onScoreChanged` → `GameScreen.UpdateHud` → `HudVisual.UpdateHud` | `_model.Score += data.Score` где `data.Score` из ScriptableObject | ✓ FLOWING |
| `HudVisual._livesContainer` | `lives` | `Game._lives` → `_onScoreChanged` → `UpdateLivesIcons(lives)` | `_lives` декрементируется в `OnEntityDestroyed(ShipModel)` | ✓ FLOWING |
| `GameOverView._scoreText` | `FinalScore` | `_game.Score` → `GameOverScreen.Show(finalScore, highScore)` | `_model.Score` — реальный счёт | ✓ FLOWING |
| `AsteroidVisual` позиция | `Position` | `AsteroidModel.Move.Position` → `EventBindingContext` → `AsteroidViewModel.Position` → `transform.position` | `MoveSystem.Update()` → `Move.Position.Value` | ✓ FLOWING |
| `AsteroidVisual` спрайт | `Sprite` | `data.SpriteVariants[Random.Range(...)]` → `vm.Sprite.Value` | Реальные спрайты из атласа в SpriteVariants | ✓ FLOWING |

---

### Поведенческие проверки

| Поведение | Команда | Результат | Статус |
|-----------|---------|-----------|--------|
| AsteroidModel.AngularSpeed присутствует | `grep "public float AngularSpeed" Assets/Scripts/Model/Entities/AsteroidModel.cs` | Строка найдена | ✓ PASS |
| AsteroidVisual выполняет вращение | `grep "Transform.Rotate.*_angularSpeed" Assets/Scripts/View/AsteroidVisual.cs` | `transform.Rotate(0f, 0f, _angularSpeed * Time.deltaTime)` | ✓ PASS |
| CreateAsteroid регистрирует prefabs | `grep "RegisterPrefab.*Asteroid" Assets/Scripts/Application/EntitiesCatalog.cs` | 3 строки RegisterPrefab | ✓ PASS |
| Пуля убивается при попадании | `grep "hitBullet.Kill" Assets/Scripts/Application/Game.cs` | `if (hitModel is BulletModel hitBullet) { hitBullet.Kill(); }` | ✓ PASS |
| Asteroid configs Score значения | `grep "Score:" Assets/Media/configs/Asteroid*Data.asset` | Big=1, Medium=2, Small=3 | ✓ PASS |
| Assets в GameData.asset | `grep "AsteroidBig\|AsteroidMedium\|AsteroidSmall" Assets/Media/configs/GameData.asset` | 3 GUID-ссылки найдены | ✓ PASS |
| Wrap-around в MoveSystem | `grep "WrapAxis" Assets/Scripts/Model/Systems/MoveSystem.cs` | Метод существует и применяется к X/Y | ✓ PASS |

---

### Покрытие требований

| Требование | Источник-план | Описание | Статус | Доказательство |
|-----------|--------------|----------|--------|---------------|
| AST-01 | 04-01, 04-04, 04-05 | Три размера астероидов: Large, Medium, Small | ✓ SATISFIED | Три prefab-а + AsteroidData configs; Model.Size=1/2/3 |
| AST-02 | 04-02 | Large при уничтожении → 2 Medium | ✓ SATISFIED | `SpawnFragments()`: childSize=asteroid.Size-1; 2 осколка |
| AST-03 | 04-02 | Medium при уничтожении → 2 Small | ✓ SATISFIED | Та же логика `SpawnFragments()` |
| AST-04 | 04-02 | Small при уничтожении исчезает полностью | ✓ SATISFIED | `if (childSize <= 0) { return; }` — нет осколков |
| AST-05 | 04-01 | Случайная начальная скорость и угловое вращение | ✓ SATISFIED | `Random.insideUnitCircle.normalized` + `AngularSpeed = Random.Range(30f, 120f) * ±1` |
| AST-06 | 04-02 | Large < Medium < Small по скорости | ✓ SATISFIED | Large=2, Medium=3, Small=4 ед/с |
| AST-07 | 04-04 | Wrap-around астероидов | ✓ SATISFIED | MoveSystem.WrapAxis применяется ко всем сущностям в MoveSystem, включая астероиды |
| AST-08 | 04-02 | Волна 1 = 4 Large; +1 каждую волну; макс. 12 | ✓ SATISFIED | `Mathf.Min(3 + _waveNumber, 12)` |
| AST-09 | 04-02, 04-04 | Безопасная дистанция при спавне (20% ширины) | ✓ SATISFIED | `GetRandomPositionOutsideRadius(shipPos, _configs.SpawnAllowedRadius, gameArea)`; SpawnAllowedRadius=20 |
| AST-10 | 04-01 | Object Pool для всех астероидов | ✓ SATISFIED | `GameObjectPool` используется через `_pool.Get<AsteroidVisual>(data.Prefab)` в `CreateAsteroid()` |
| PROG-01 | 04-02 | Очки за астероиды (реализованы через DATA_SCHEMA 1/2/3, не 20/50/100) | ⚠ PARTIAL | REQUIREMENTS.md фиксирует 20/50/100; реализация использует DATA_SCHEMA (1/2/3). D-01 в CONTEXT.md намеренно отдаёт приоритет DATA_SCHEMA. Расхождение — известное решение по дизайну, не дефект. UFO-очки (200/1000) — Phase 5. |
| PROG-02 | 04-03, 04-05 | Счёт в левом верхнем углу HUD в реальном времени | ? NEEDS HUMAN | Реализация присутствует в коде и в сцене, но REQUIREMENTS.md показывает `[ ]`. UAT-тест 4 был issue, закрыт в 04-05. Требует финального подтверждения. |
| PROG-03 | 04-02, 04-03 | 3 жизни при старте, иконки корабля в HUD | ✓ SATISFIED | `_lives = 3` в `Start()`; `UpdateLivesIcons(lives)` создаёт Image-компоненты с `_lifeIconSprite` |
| PROG-04 | 04-02 | Экстра-жизнь каждые 10 000 очков (макс. 6) | ✓ SATISFIED | `while (_model.Score >= _nextBonusLifeScore && _lives < 6) { _lives++; _nextBonusLifeScore += 10000; }` |
| PROG-05 | 04-02, 04-04 | Game Over при 0 жизнях | ✓ SATISFIED | `if (_lives <= 0) { _isRunning = false; _onGameOver?.Invoke(); }` |
| PROG-07 | 04-02, 04-03, 04-04 | High Score текущей сессии на экране Game Over | ✓ SATISFIED | `_highScore` обновляется при Game Over; передаётся в `GameOverScreen.Show(score, highScore)` |
| PROG-08 | 04-02, 04-03, 04-04 | Play Again без перезапуска приложения | ✓ SATISFIED | `Restart()`: `_model.CleanUp()` → `_catalog.Reset()` → `Start()` — без перезагрузки сцены |

---

### Антипаттерны

| Файл | Строка | Паттерн | Серьёзность | Влияние |
|------|-------|---------|-------------|---------|
| `Assets/Scripts/Application/ModelFactory.cs` | 21 | `// TODO @a.shatalov: model pool` | ℹ Info | Release() пустой — model pool не реализован. Не блокирует Phase 4: модели переиспользуются через factory (создание новых), view-pool работает корректно через GameObjectPool. |
| `Assets/Scripts/Application/Game.cs` | 190 | `Debug.Log("[Game] Laser fired")` в `OnUserLaserFired` | ℹ Info | Лазер — заглушка (Phase 3 — лог вместо логики). Не относится к требованиям Phase 4. |
| `Assets/Media/configs/AsteroidSmallData.asset` | 18-20 | SpriteVariants используют sprite IDs из того же PNG что и medium | ⚠ Warning | Известное решение: SUMMARY 04-04 документирует "AsteroidSmall использует medium-спрайты — PNG не содержит small-вариантов". Функционально работает, визуально Small имеет те же спрайты что и Medium. |

---

### Требования, нуждающиеся в верификации человеком

#### 1. Счёт в HUD обновляется в реальном времени (PROG-02)

**Тест:** Нажать Play → Play на TitleScreen. Уничтожить Large астероид (Space). Наблюдать HUD в левом верхнем углу.
**Ожидается:** Счёт виден и меняется с 0 на 1 немедленно. При уничтожении Medium — +2, Small — +3.
**Почему нужен человек:** REQUIREMENTS.md показывает PROG-02 как `[ ]` несмотря на то что 04-UAT.md фиксирует issue как "закрытое" в plan 04-05. Требует визуальной проверки в Unity Editor.

#### 2. HUD показывает 3 иконки жизней при старте

**Тест:** После нажатия Play в TitleScreen проверить HUD под счётом.
**Ожидается:** Три маленькие иконки корабля (20×20px) видны слева под строкой счёта.
**Почему нужен человек:** `UpdateLivesIcons()` создаёт GameObject-ы динамически при `_lifeIconSprite != null`. Поле назначено в сцене (fileID ссылка существует), но корректность назначения и видимость зависят от Unity Inspector.

#### 3. Game Over экран при 0 жизнях

**Тест:** Позволить кораблю столкнуться с астероидом 3 раза.
**Ожидается:** Появляется экран GameOverScreen с текстом "Score: N" и "Best: N". Кнопка "Play Again" кликабельна. Submit Score и Leaderboard — серые (disabled).
**Почему нужен человек:** Тест 5–6 в 04-UAT.md помечены "skipped — будет покрыт автотестами". UI-взаимодействие не может быть проверено статически.

#### 4. Play Again сохраняет High Score между сессиями

**Тест:** Набрать N очков → Game Over → Play Again → Game Over снова (с меньшим счётом).
**Ожидается:** Best на Game Over экране отображает старый более высокий счёт.
**Почему нужен человек:** `_highScore` — поле в памяти C# класса. Статически подтверждено что он не сбрасывается в `Restart()`, но только запуск в Unity Editor может подтвердить реальное поведение.

---

### Краткое резюме

**5/5 success criteria из ROADMAP.md верифицированы статически через код:**

1. ✓ Дробление: Large→2 Medium→2 Small→исчезает; осколки быстрее (2/3/4 ед/с)
2. ✓ Волны: 4 Large первая, +1 каждую (макс 12); безопасная дистанция через `GetRandomPositionOutsideRadius`
3. ✓ Счёт в HUD обновляется через `_onScoreChanged` callback → `GameScreen.UpdateHud` → `HudVisual.UpdateHud`; поля назначены в сцене
4. ✓ 3 иконки жизней при старте; экстра-жизнь каждые 10 000 (макс 6)
5. ✓ Game Over экран с Score/HighScore; Play Again через `Restart()` без перезагрузки приложения

**Замечание по PROG-01:** Требование описывает очки 20/50/100 для астероидов, но реализация намеренно использует 1/2/3 из DATA_SCHEMA (D-01 в CONTEXT.md). Это известное проектное решение с документированным обоснованием — не дефект.

**Замечание по AsteroidSmall спрайтам:** Используются спрайты от Medium-размера (нет отдельных Small-спрайтов в PNG). Задокументировано в SUMMARY 04-04 как известное ограничение ресурсов.

**Четыре пункта требуют финального подтверждения человека** (визуальная проверка в Unity Editor Play Mode), особенно PROG-02 который REQUIREMENTS.md не отмечает как выполненный.

---

_Верифицировано: 2026-03-28T14:00:00Z_
_Верификатор: Claude (gsd-verifier)_
