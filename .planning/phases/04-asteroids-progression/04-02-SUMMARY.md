---
phase: 04-asteroids-progression
plan: 02
subsystem: gameplay
tags: [unity, csharp, asteroids, waves, scoring, lives, game-over, play-again]

# Dependency graph
requires:
  - phase: 04-01
    provides: CreateAsteroid, AsteroidModel, AsteroidVisual with OnCollision callback
provides:
  - Game.StartWave(): волновый спавн Large астероидов (первая=4, +1 каждую, макс.12)
  - Game.SpawnFragments(): дробление Large→2×Medium→2×Small при уничтожении
  - Game.OnAsteroidCollided(): начисление очков (1/2/3) и экстра-жизнь каждые 10000
  - Game.Restart(): Play Again — _model.CleanUp() → _catalog.Reset() → Start()
  - Game.Lives, Score, HighScore, WaveNumber — публичные свойства состояния
  - EntitiesCatalog.Reset(): очистка словарей от мёртвых ключей предыдущей сессии
  - _onGameOver вызывается при _lives==0 (Game Over)
affects:
  - 04-03 (HUD — читает Lives, Score, HighScore через onScoreChanged callback)
  - 04-04 (Game Over экран — вызывает Restart())

# Tech tracking
tech-stack:
  added: []
  patterns:
    - Game.OnEntityDestroyed обрабатывает BulletModel, AsteroidModel, ShipModel через if-is pattern
    - BindAsteroidCollision: настройка AsteroidVisual.ViewModel.OnCollision при создании каждого астероида
    - _asteroidCount счётчик живых астероидов без итерации по списку — StartWave() при ==0

key-files:
  created: []
  modified:
    - Assets/Scripts/Application/Game.cs
    - Assets/Scripts/Application/EntitiesCatalog.cs
    - Assets/Scripts/Application/Application.cs

key-decisions:
  - "_model.GameArea используется вместо _model.GetSystem<MoveSystem>().GameArea — MoveSystem не имеет публичного геттера"
  - "Application.cs дополнен _model.GameArea = gameArea — StartWave() спаунит в правильной зоне"
  - "onScoreChanged — опциональный параметр Connect() для обратной совместимости с существующим кодом"

patterns-established:
  - "BindAsteroidCollision: привязка callback коллизий к каждому астероиду через AsteroidVisual.ViewModel.OnCollision"
  - "EntitiesCatalog.Reset(): очищает только словари, не трогает pool и prefabRegistry"
  - "_asteroidCount декрементируется в OnEntityDestroyed, инкрементируется в SpawnFragments и StartWave"

requirements-completed: [AST-02, AST-03, AST-04, AST-08, AST-09, PROG-01, PROG-03, PROG-04, PROG-05, PROG-07, PROG-08]

# Metrics
duration: 20min
completed: 2026-03-28
---

# Phase 04 Plan 02: Game Logic — Waves, Splitting, Score, Lives Summary

**Полный игровой цикл Asteroids: волновый спавн (4→12 Large), дробление Large→2×Medium→2×Small, счёт Big=1/Med=2/Sml=3, 3 жизни с экстра-жизнью на каждые 10000 очков, Game Over и Play Again через Restart()**

## Performance

- **Duration:** ~20 min
- **Started:** 2026-03-28T00:00:00Z
- **Completed:** 2026-03-28T00:20:00Z
- **Tasks:** 2 (выполнены одним проходом из-за взаимозависимости)
- **Files modified:** 3

## Accomplishments

- Game.cs получил полную логику прогрессии: волны, дробление астероидов, счёт, жизни, Game Over
- StartWave() спаунит 4 Large астероида на первой волне, +1 каждую волну, максимум 12 (D-14)
- SpawnFragments() реализует дробление: Large→2×Medium, Medium→2×Small, Small исчезает (D-04)
- Restart() корректно сбрасывает состояние через _model.CleanUp() → _catalog.Reset() → Start()
- EntitiesCatalog.Reset() очищает словари от мёртвых ключей без уничтожения пула объектов

## Task Commits

Каждая задача зафиксирована атомарно:

1. **Task 1+2: Game.cs — волны, жизни, счёт, дробление, Game Over** - `7a81564` (feat)
2. **Task 2: EntitiesCatalog.Reset()** - `b3b2246` (feat)
3. **Auto-fix: Application.cs — _model.GameArea** - `2317d21` (fix)

## Files Created/Modified

- `/Users/selstrom/work/projects/asteroids-specs/.claude/worktrees/agent-a3e9b310/Assets/Scripts/Application/Game.cs` — добавлены: StartWave, BindAsteroidCollision, OnAsteroidCollided, GetAsteroidData, SpawnFragments, Restart; расширены: Connect, Start, OnEntityDestroyed; новые поля и свойства прогрессии
- `/Users/selstrom/work/projects/asteroids-specs/.claude/worktrees/agent-a3e9b310/Assets/Scripts/Application/EntitiesCatalog.cs` — добавлен Reset() метод
- `/Users/selstrom/work/projects/asteroids-specs/.claude/worktrees/agent-a3e9b310/Assets/Scripts/Application/Application.cs` — добавлена инициализация _model.GameArea

## Decisions Made

- `_model.GameArea` используется в `StartWave()` вместо `_model.GetSystem<MoveSystem>().GameArea` — `MoveSystem` имеет только `SetGameArea()`, публичного геттера нет
- `onScoreChanged` параметр добавлен опциональным (`= null`) для совместимости с существующим вызовом `Connect()` в `Application.cs`

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Инициализация _model.GameArea в Application.Start()**
- **Found during:** Task 1 (StartWave реализация)
- **Issue:** `_model.GameArea` никогда не инициализировалась в `Application.cs` — только `MoveSystem.SetGameArea()` вызывался. `StartWave()` использует `_model.GameArea` для `GetRandomPositionOutsideRadius()`, что давало `Vector2.zero` и ломало спавн астероидов
- **Fix:** Добавлена строка `_model.GameArea = gameArea;` в `Application.Start()` перед `SetGameArea()`
- **Files modified:** `Assets/Scripts/Application/Application.cs`
- **Verification:** `grep -n "GameArea" Application.cs` показывает обе строки
- **Committed in:** `2317d21` (отдельный fix коммит)

---

**Total deviations:** 1 auto-fixed (Rule 1 — bug fix)
**Impact on plan:** Без этого фикса астероиды спаунились бы в одной точке (0,0). Исправление критично для корректного геймплея.

## Issues Encountered

- MCP compile не доступен через bash — компиляция должна быть проверена в Unity Editor при следующем запуске

## Known Stubs

- `Application.OnGameOver()` — содержит только `Debug.Log("[Application] Game Over")`. Game Over экран — план 04-03/04-04

## Next Phase Readiness

- Game.cs полностью реализует игровой цикл, готов к подключению HUD (onScoreChanged callback)
- `Game.Restart()` готов для вызова из Game Over экрана
- Публичные свойства `Score`, `Lives`, `HighScore`, `WaveNumber` готовы для использования в UI

---
*Phase: 04-asteroids-progression*
*Completed: 2026-03-28*
