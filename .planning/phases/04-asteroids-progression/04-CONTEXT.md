# Phase 4: Asteroids & Progression — Context

**Gathered:** 2026-03-28
**Status:** Ready for planning

<domain>
## Phase Boundary

Полный игровой цикл: три размера астероидов с логикой дробления, волновый спавн, счёт, жизни, экран Game Over. Phase 4 использует все заглушки (AsteroidModel, UfoBigModel.cs, MoveToSystem/ShootToSystem) созданные в Phase 3, но UFO-логику НЕ реализует — это Phase 5.

**Включено в Phase 4:** AST-01..10, PROG-01..05, PROG-07, PROG-08.
**Исключено:** UFO (Phase 5), аудио/VFX взрывов астероидов (Phase 6), Submit Score / Leaderboard (Phase 7).

</domain>

<decisions>
## Implementation Decisions

### Счёт — DATA_SCHEMA значения
- **D-01:** Очки за астероиды берутся из DATA_SCHEMA (приоритет по D-07 Phase 3): Big=1, Medium=2, Small=3. Значения 20/50/100 из REQUIREMENTS — не используются. Балансные значения хранятся в `AsteroidBigData.asset`, `AsteroidMediumData.asset`, `AsteroidSmallData.asset` поле `Score`.
- **D-02:** Очки за UFO (200/1000 из PROG-01) — Phase 5. В Phase 4 UFO не появляются.
- **D-03:** Экстра-жизнь каждые 10 000 очков (PROG-04) — порог остаётся 10 000 даже с баллами 1/2/3. Это намеренно: значения из DATA_SCHEMA являются источником истины.

### Дробление астероидов
- **D-04:** При уничтожении Large → 2 Medium, Medium → 2 Small, Small → исчезает. Осколки появляются в позиции родителя.
- **D-05:** Направления полёта осколков — **случайные** (`Random.insideUnitCircle.normalized`). Скорость осколков выше скорости родителя (AST-06): Small > Medium > Large. Конкретные значения скоростей — по DATA_SCHEMA или Claude's Discretion если не указано.
- **D-06:** Визуальное вращение астероидов (AST-05: случайное угловое вращение) реализуется в `AsteroidVisual` через `Transform.Rotate` в `Update()` с фиксированной случайной угловой скоростью, заданной при спавне — не через `RotateComponent` (тот управляет направлением корабля, а не визуальным вращением).

### HUD в Phase 4
- **D-07:** Координаты X/Y корабля **остаются** в HUD (отладочная информация). Phase 4 **добавляет** поверх или рядом: Score (верх-лево), Lives-иконки (под Score), High Score (верх-право).
- **D-08:** Иконки жизней — N миниатюрных копий Ship-спрайта (`GameData.Ship.MainSprite`) в ряд. Каждая иконка — один `Image` компонент. Управляется из `HudVisual` динамически (добавлять/удалять иконки при изменении `Lives`).
- **D-09:** High Score хранится в памяти во время сессии (простое `int` поле в Game или отдельный `SessionData`). При новом Play Again сессионный High Score сохраняется. На диск не пишется — только в памяти процесса.
- **D-10:** High Score отображается в HUD (верх-право) с первого запуска игры (значение 0, пока не побито).

### Game Over экран
- **D-11:** Game Over экран содержит: финальный счёт (`Score: N`), High Score (`Best: N`), кнопка **«Play Again»** (активна), кнопки **«Submit Score»** и **«Leaderboard»** — disabled (серые, не кликабельны). Layout финальный — Phase 7 только подключает логику к уже существующим кнопкам.
- **D-12:** «Play Again» полностью перезапускает игровую сессию: сбрасывает счёт, волну, жизни, пересоздаёт корабль, запускает первую волну. High Score сессии сохраняется.
- **D-13:** Game Over экран создаётся в Phase3Setup или новым EditorScript в Phase 4 — отдельный Canvas-объект `GameOverScreen` (аналогично TitleScreen).

### Волновый спавн
- **D-14:** Первая волна: 4 Large астероида. Каждая следующая: `prevCount + 1` Large, максимум 12. Счётчик волны хранится в `Game.cs`.
- **D-15:** Волна начинается немедленно после уничтожения последнего астероида — без задержки (если PROG-06 не требует иного; PROG-06 — баннер волны — Phase 5).
- **D-16:** Астероиды первой волны спауниатся вдали от корабля — через `GameUtils.GetRandomPositionOutsideRadius(SpawnAllowedRadius=20)`. Корабль в (0,0) — радиус 20 уже достаточен.
- **D-17:** `GameData.AsteroidInitialCount = 10` — это значение из DATA_SCHEMA, но ROADMAP Success Criteria говорит «первая волна 4 Large». Используем ROADMAP Success Criteria: **первая волна = 4 Large**, `AsteroidInitialCount` — возможно для других целей или устаревшее поле.

### Asset-файлы Phase 4
- **D-18:** Создать в `Assets/Media/configs/`: `AsteroidBigData.asset`, `AsteroidMediumData.asset`, `AsteroidSmallData.asset` (Score=1/2/3, Prefab и SpriteVariants назначить).
- **D-19:** Создать prefabs в `Assets/Media/prefabs/`: `asteroid_big.prefab`, `asteroid_medium.prefab`, `asteroid_small.prefab` — с `Rigidbody2D (Kinematic)`, `CircleCollider2D`, `AsteroidVisual` компонентом, SpriteRenderer.
- **D-20:** Спрайты для астероидов берутся из `Assets/Media/sprites/` — слайсированный PNG (3 варианта на каждый размер по DATA_SCHEMA). Если имена sub-спрайтов неизвестны — прочитать из импортированного PNG в Unity.
- **D-21:** `GameData.asset` дополнить ссылками: `AsteroidBig`, `AsteroidMedium`, `AsteroidSmall` → соответствующие .asset файлы.

### Claude's Discretion
- Конкретные скорости астероидов (Small/Medium/Large) — если не в DATA_SCHEMA, выбрать разумные (например: Large=2, Medium=3, Small=4 ед/с)
- Угловая скорость вращения астероидов (случайный диапазон, например 30–120 °/с)
- Размер коллайдеров астероидов (по размеру спрайта)
- Внешний вид Game Over экрана (шрифты, отступы, цвета — в стиле Phase 3 TitleScreen)
- Размер иконок жизней в HUD (предложение: 20×20px)

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Архитектура и паттерны
- `.planning/codebase/ARCHITECTURE.md` — ECS+MVVM, EntitiesCatalog паттерн, как добавлять новые сущности
- `.planning/codebase/CONVENTIONS.md` — naming, стиль, Connect/Dispose lifecycle

### Данные
- `.planning/codebase/DATA_SCHEMA.md` — AsteroidData поля, GunData, GameData структура — ИСТОЧНИК ИСТИНЫ для числовых значений
- `.planning/codebase/UNITY_CONFIG.md` — физические слои (Layer 8=Asteroid), Collision Matrix

### Требования Phase 4
- `.planning/REQUIREMENTS.md` §Asteroids (AST-01..10), §Scoring (PROG-01..05, PROG-07, PROG-08)
- `.planning/ROADMAP.md` §Phase 4 — Success Criteria (5 критериев)

### Предыдущие контексты
- `.planning/phases/03-core-mechanics/3-CONTEXT.md` — D-07 (DATA_SCHEMA приоритет), D-08 (коллизии), D-09 (слои), ECS паттерны
- `.planning/phases/01-project-foundation/1-CONTEXT.md` §D-11 — Layer матрица

### Структура
- `.planning/codebase/STRUCTURE.md` — куда добавлять новые файлы

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Assets/Scripts/Model/Entities/AsteroidModel.cs` — заглушка уже существует: `MoveComponent Move`, `int Size (1=Small, 2=Medium, 3=Big)`, `Kill()`, `AcceptWith(IGroupVisitor)`
- `Assets/Scripts/Application/Game.cs` — `OnEntityDestroyed` callback, `EntitiesCatalog.Release` — добавить логику дробления здесь
- `Assets/Scripts/Application/EntitiesCatalog.cs` — паттерн `CreateShip/CreateBullet` — добавить `CreateAsteroid(data, position, velocity)`
- `Assets/Scripts/Utils/GameUtils.cs` — `GetRandomPositionOutsideRadius(radius)` — готов к использованию для спавна
- `Assets/Scripts/Utils/GameObjectPool.cs` — Stack-based Pool — готов к использованию для asteroid prefabs
- `Assets/Scripts/View/HudVisual.cs` — уже имеет TextMeshPro поля; добавить `score_text`, `lives_container`, `high_score_text`

### Established Patterns
- Новая сущность = Model (pure C#) + Visual MonoBehaviour + ViewModel + EntitiesCatalog.Create/Release
- Collision обнаруживается в Visual → ViewModel.OnCollision → Game обрабатывает
- Object Pool по prefab id через `GameObjectPool`
- Config данные через ScriptableObject (`AsteroidData : BaseGameEntityData`)
- ActionScheduler для отложенных действий (таймеры, волны)

### Integration Points
- `Game.OnEntityDestroyed` — место для логики дробления: `if (model is AsteroidModel ast) SpawnFragments(ast)`
- `Game.Start()` — добавить `StartWave(1)` после создания корабля
- `Model.cs` — `IGroupVisitor.Visit(AsteroidModel)` уже объявлен, нужна реализация в GroupCreator
- `HudVisual` — существующий MonoBehaviour на Hud GameObject; добавить новые поля и биндинги

</code_context>

<deferred>
## Deferred Ideas

- **UFO спавн** (SpawnNewEnemyDurationSec=25с, UfoBigData/UfoData) — Phase 5
- **PROG-06** — баннер номера волны в HUD — Phase 5
- **VFX взрывов астероидов** (VIS-04: particle effects) — Phase 6
- **Submit Score** / реальная Leaderboard кнопка — Phase 7
- **Персистентный High Score** (между сессиями через PlayerPrefs) — не в scope v1.0

</deferred>

---

*Phase: 04-asteroids-progression*
*Context gathered: 2026-03-28*
