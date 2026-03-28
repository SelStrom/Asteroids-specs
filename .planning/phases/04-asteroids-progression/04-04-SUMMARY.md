---
phase: 04-asteroids-progression
plan: "04"
subsystem: application-integration
tags: [application, game-over, asteroids, prefabs, unity-assets, scriptable-objects]
dependency_graph:
  requires: [04-02, 04-03]
  provides: [full-game-cycle, asteroid-prefabs, asteroid-configs, game-over-ui]
  affects: [Assets/Scripts/Application/Application.cs, Assets/Scripts/Application/ApplicationEntry.cs, Assets/Media/prefabs, Assets/Media/configs]
tech_stack:
  added: [Phase4Setup EditorScript, AsteroidData ScriptableObject assets]
  patterns: [Application wire-up pattern, EditorScript asset creation pattern]
key_files:
  created:
    - Assets/Media/prefabs/asteroid_big.prefab
    - Assets/Media/prefabs/asteroid_medium.prefab
    - Assets/Media/prefabs/asteroid_small.prefab
    - Assets/Media/configs/AsteroidBigData.asset
    - Assets/Media/configs/AsteroidMediumData.asset
    - Assets/Media/configs/AsteroidSmallData.asset
    - Assets/Editor/Phase4Setup.cs
    - Assets/Scripts/View/AsteroidVisual.cs.meta
  modified:
    - Assets/Scripts/Application/Application.cs
    - Assets/Scripts/Application/ApplicationEntry.cs
    - Assets/Media/configs/GameData.asset
decisions:
  - "YAML prefabs созданы с фиксированными GUID — Unity перезапишет при импорте через Phase4Setup"
  - "AsteroidSmall использует medium-спрайты — PNG не содержит small-вариантов"
  - "Game.Connect вызывается в OnGameStart() а не в Application.Start() — чтобы передать onScoreChanged callback"
metrics:
  duration: "~25 минут"
  completed: "2026-03-28"
  tasks_completed: 2
  files_changed: 16
---

# Phase 4 Plan 04: Интеграция Application и Unity Assets Summary

**Одна строка:** Полная интеграция игрового цикла — Application.cs расширен GameOverScreen/OnScoreChanged, созданы 3 prefab астероидов с Layer=8 и 3 AsteroidData конфига (Score: 1/2/3), GameData обновлён ссылками.

## Что сделано

### Задача 1: Application.cs + ApplicationEntry.cs с GameOverScreen

Application.cs расширен:
- Новые поля: `_gameOverView`, `_gameOverGo`, `_gameOverScreen`, `_gameScreen`
- `Connect()` принимает `gameOverView` и `gameOverGo` параметры
- `OnGameStart()`: инициализирует Game с `onScoreChanged` callback, создаёт GameScreen, показывает HUD
- `OnGameOver()`: показывает GameOverScreen через `_gameOverScreen.Show(score, highScore)`
- `OnPlayAgain()`: скрывает GameOverScreen, вызывает `_game.Restart()`, обновляет HUD
- `OnScoreChanged(int, int)`: обновляет HUD при изменении счёта/жизней

ApplicationEntry.cs расширен:
- `[SerializeField] private GameOverView _gameOverView`
- `[SerializeField] private GameObject _gameOverGo`
- `Connect()` передаёт оба поля в Application

**Отклонение:** `Game.Connect()` перенесён из `Application.Start()` в `OnGameStart()` — чтобы `onScoreChanged` callback передавался при каждом запуске игры (включая Restart).

### Задача 2: Unity Assets

**Prefabs** (YAML формат, Layer=8, Rigidbody2D Kinematic, GravityScale=0, useFullKinematicContacts=1):
- `asteroid_big.prefab` — CircleCollider2D radius=0.4 (спрайт 69x70px @ 100ppu)
- `asteroid_medium.prefab` — CircleCollider2D radius=0.22 (спрайт 43x43px @ 100ppu)
- `asteroid_small.prefab` — CircleCollider2D radius=0.15 (Claude's Discretion)

**AsteroidData configs:**
- `AsteroidBigData.asset` — Score: 1
- `AsteroidMediumData.asset` — Score: 2
- `AsteroidSmallData.asset` — Score: 3

**GameData.asset** обновлён:
- `AsteroidBig` → ссылка на AsteroidBigData.asset
- `AsteroidMedium` → ссылка на AsteroidMediumData.asset
- `AsteroidSmall` → ссылка на AsteroidSmallData.asset

**Phase4Setup.cs** создан как EditorScript (`Assets/Editor/`) для заполнения SpriteVariants в configs, обновления Prefab ссылок в configs и добавления GameOverScreen в сцену через `Asteroids/Setup Phase 4 Assets`.

## Checkpoint: Требуется верификация

Пользователь должен:
1. Открыть Unity Editor (убедиться что проект компилируется)
2. Выполнить `Asteroids > Setup Phase 4 Assets` для заполнения SpriteVariants и GameOver UI в сцене
3. Нажать Play и проверить полный игровой цикл

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Game.Connect перенесён в OnGameStart()**
- **Found during:** Задача 1
- **Issue:** В исходном Application.cs `_game.Connect()` вызывался в `Start()` без `onScoreChanged`. При повторном вызове `OnGameStart()` (Play Again) connect не перевызывался, callback не обновлялся.
- **Fix:** `Game.Connect()` теперь вызывается в `OnGameStart()` с полным набором callbacks
- **Files modified:** `Assets/Scripts/Application/Application.cs`
- **Commit:** 202f8df

**2. [Rule 2 - Missing] AsteroidVisual.cs.meta**
- **Found during:** Задача 2
- **Issue:** AsteroidVisual.cs не имела .meta файла — Unity не создала GUID, невозможно ссылаться на скрипт в prefab YAML
- **Fix:** Создан .meta файл с фиксированным GUID `a7b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7`
- **Files modified:** `Assets/Scripts/View/AsteroidVisual.cs.meta`
- **Commit:** d1f6fec

**3. [Rule 2 - Missing] Phase4Setup.cs обновляет вместо пересоздания**
- **Found during:** Задача 2
- **Issue:** Оригинальный план предполагал пересоздание .asset файлов, что удалило бы GUID и нарушило ссылки
- **Fix:** Phase4Setup.cs использует `UpdateOrCreateAsteroidConfig()` — обновляет существующие, создаёт только если отсутствуют
- **Commit:** d1f6fec

## Known Stubs

- `AsteroidBigData.SpriteVariants = []` — пустой массив; Phase4Setup.cs заполнит при запуске `Setup Phase 4 Assets` в Editor
- `AsteroidMediumData.SpriteVariants = []` — аналогично
- `AsteroidSmallData.SpriteVariants = []` — аналогично
- `asteroid_*.prefab` имеют `m_Sprite: {fileID: 0}` — Phase4Setup.cs назначит спрайты при запуске
- GameOverScreen в сцене не добавлен через YAML (нужен Phase4Setup.cs в Editor)
- ApplicationEntry поля `_gameOverView`, `_gameOverGo` не назначены в Inspector (нужен Phase4Setup.cs)

**Эти стабы разрешаются одним запуском `Asteroids > Setup Phase 4 Assets` в Unity Editor.**

## Commits

| Task | Commit | Files |
|------|--------|-------|
| Task 1: Application.cs + ApplicationEntry.cs | 202f8df | Application.cs, ApplicationEntry.cs |
| Task 2: Unity assets (prefabs + configs) | d1f6fec | 14 файлов: 3 prefabs, 3 assets, GameData, Phase4Setup.cs |

## Self-Check: PASSED

Files exist:
- `/Users/selstrom/work/projects/asteroids-specs/Assets/Scripts/Application/Application.cs` - FOUND
- `/Users/selstrom/work/projects/asteroids-specs/Assets/Scripts/Application/ApplicationEntry.cs` - FOUND
- `/Users/selstrom/work/projects/asteroids-specs/Assets/Media/prefabs/asteroid_big.prefab` - FOUND
- `/Users/selstrom/work/projects/asteroids-specs/Assets/Media/prefabs/asteroid_medium.prefab` - FOUND
- `/Users/selstrom/work/projects/asteroids-specs/Assets/Media/prefabs/asteroid_small.prefab` - FOUND
- `/Users/selstrom/work/projects/asteroids-specs/Assets/Media/configs/AsteroidBigData.asset` - FOUND
- `/Users/selstrom/work/projects/asteroids-specs/Assets/Media/configs/AsteroidMediumData.asset` - FOUND
- `/Users/selstrom/work/projects/asteroids-specs/Assets/Media/configs/AsteroidSmallData.asset` - FOUND

Commits exist:
- 202f8df - FOUND
- d1f6fec - FOUND
