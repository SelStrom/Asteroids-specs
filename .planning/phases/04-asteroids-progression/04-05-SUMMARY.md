---
phase: 04-asteroids-progression
plan: 05
subsystem: ui, gameplay
tags: [unity, hud, bullet, collision, gap-closure]

requires:
  - phase: 04-04
    provides: Application слой с HUD, Game Over экраном и полным игровым циклом

provides:
  - Уничтожение пули при первом попадании в астероид (bullet.Kill() в OnAsteroidCollided)
  - HUD обновляется в реальном времени: счёт, жизни, HighScore, координаты, угол, скорость, лазер
  - ShipViewModel передаётся в HudVisual после Game.Start()
  - HUD растянут на весь экран с корректными anchor/pivot для всех элементов
  - Unity UI skill для предотвращения повторных ошибок с anchor/pivot

affects: [verification, uat]

tech-stack:
  added: []
  patterns: [Resources.FindObjectsOfTypeAll для неактивных объектов, pivot совпадает с anchor corner]

key-files:
  created:
    - .claude/skills/unity-ui/SKILL.md
  modified:
    - Assets/Scripts/Application/Game.cs
    - Assets/Scripts/Application/Application.cs
    - Assets/Scripts/View/HudVisual.cs
    - Assets/Scripts/View/ShipVisual.cs
    - Assets/Scripts/Application/EntitiesCatalog.cs
    - Assets/Editor/Phase4Setup.cs

key-decisions:
  - "bullet.Kill() вызывается в OnAsteroidCollided — пуля явно убивается при попадании, не зависит от lifetime"
  - "ShipViewModel хранится в Game._shipViewModel и передаётся через Game.ShipViewModel геттер"
  - "GameScreen.Connect(hudVisual, null) вызывается ДО Game.Start() — иначе начальный onScoreChanged теряется"
  - "GameScreen.Connect(hudVisual, game.ShipViewModel) вызывается ПОСЛЕ Game.Start() для debug полей"
  - "Hud растянут на весь экран (stretch anchor); элементы прибиты к углам с корректным pivot"
  - "Resources.FindObjectsOfTypeAll<HudVisual>() вместо GameObject.Find() для неактивных объектов"
  - "Проектный skill unity-ui создан для предотвращения повторных ошибок anchor/pivot"

patterns-established:
  - "Unity UI: pivot всегда совпадает с anchor corner (верх-лево → pivot (0,1))"
  - "Unity: для поиска неактивных объектов — Resources.FindObjectsOfTypeAll<T>() с фильтром по scene"
  - "Game слой: ShipViewModel экспортируется через публичный геттер, не передаётся напрямую"

requirements-completed: [AST-01, AST-02, AST-03, AST-04, AST-05, AST-06, AST-07, AST-08, AST-09, AST-10, PROG-01, PROG-02, PROG-03, PROG-04, PROG-05, PROG-07, PROG-08]

duration: 90min
completed: 2026-03-28
---

# Plan 04-05: Gap Closure — Bullet Destroy + HUD Fix Summary

**Два UAT-блокера закрыты: пуля уничтожается при попадании в астероид, HUD обновляет счёт/жизни/debug поля в реальном времени**

## Performance

- **Duration:** ~90 min
- **Completed:** 2026-03-28
- **Tasks:** 3 (2 auto + 1 human-verify)
- **Files modified:** 6

## Accomplishments

- `Game.OnAsteroidCollided` теперь вызывает `hitBullet.Kill()` — пуля исчезает при первом попадании
- `HudVisual.Update()` читает все 5 полей ShipViewModel: координаты, угол, скорость, лазер счётчик, перезарядка
- `ShipViewModel` расширен полями `Speed`, `LaserCount`, `LaserReloadTime` с биндингами в EntitiesCatalog
- `Application.OnGameStart()` вызывает `GameScreen.Connect()` до и после `Game.Start()` для корректной инициализации
- HUD растянут на весь Canvas; элементы score/lives/highscore прибиты к углам с правильным anchor+pivot
- Unity UI skill (`.claude/skills/unity-ui/SKILL.md`) создан для предотвращения повторных UI-ошибок
- `Phase4Setup` использует `Resources.FindObjectsOfTypeAll<HudVisual>()` для поиска неактивных GameObject

## Task Commits

1. **Task 1: bullet.Kill() в OnAsteroidCollided** — `336fc75` (fix)
2. **Task 2: диагностические логи в Phase4Setup** — `ba70475` (fix)
3. **HUD layout redesign + unity-ui skill** — `ed91e8b` (fix)
4. **ShipViewModel передан в HudVisual** — `f4cd9af` (fix)
5. **Speed/LaserCount/LaserReloadTime в ShipViewModel** — (текущая сессия)
6. **Task 3: Human Verify — approved** ✓

## Files Created/Modified

- `Assets/Scripts/Application/Game.cs` — bullet.Kill() + ShipViewModel геттер
- `Assets/Scripts/Application/Application.cs` — двойной Connect порядок (до/после Start)
- `Assets/Scripts/View/HudVisual.cs` — Update() читает angle/speed/laser поля
- `Assets/Scripts/View/ShipVisual.cs` — ShipViewModel расширен Speed/LaserCount/LaserReloadTime
- `Assets/Scripts/Application/EntitiesCatalog.cs` — биндинги Speed/Laser в CreateShip()
- `Assets/Editor/Phase4Setup.cs` — FindObjectsOfTypeAll + layout redesign + диагностика
- `.claude/skills/unity-ui/SKILL.md` — проектный skill: правила anchor/pivot/inactive search

## Decisions Made

- `hitBullet.Kill()` вызывается явно после `asteroid.Kill()` в том же методе, не через lifecycle
- `_gameScreen.Connect(null)` до Start() чтобы не потерять начальный `onScoreChanged`, затем реконнект с реальным ShipViewModel
- Speed/Laser биндятся в EntitiesCatalog рядом с остальными биндингами корабля

## Deviations from Plan

### Auto-fixed Issues

**1. HUD элементы перекрывались из-за неверного pivot**
- **Issue:** anchor (0,1) + pivot (0.5,0.5) → элементы наполовину уходили за экран
- **Fix:** pivot=(0,1) для верх-лево элементов, pivot=(1,1) для верх-право; Hud растянут на весь экран
- **Committed in:** ed91e8b

**2. ShipViewModel не передавался в HudVisual**
- **Issue:** `_gameScreen.Connect(_hudVisual, null)` → debug поля (angle, speed, etc.) не обновлялись
- **Fix:** Game экспортирует ShipViewModel; Application вызывает Connect дважды — до и после Start
- **Committed in:** f4cd9af

**3. Speed/Laser не были в ShipViewModel**
- **Issue:** HudVisual.Update() пытался читать _ship.Speed но поля не существовало
- **Fix:** Добавлены поля + биндинги в EntitiesCatalog
- **Committed in:** текущая сессия

---

**Total deviations:** 3 auto-fixed (UI layout, ShipViewModel wiring, ViewModel completeness)
**Impact on plan:** Все фиксы необходимы для правильной работы HUD. Scope не расширен.

## Issues Encountered

- `GameObject.Find()` не находит неактивные объекты — заменён на `Resources.FindObjectsOfTypeAll<HudVisual>()`
- CS0128 duplicate `hlg` переменная в Phase4Setup после ручного редактирования — удалён дубликат (`1fee944`)

## Next Phase Readiness

- Phase 04 полностью завершена: ECS астероиды, игровой цикл, HUD, Game Over, bullet destroy
- Готово к финальной верификации фазы

---
*Phase: 04-asteroids-progression*
*Completed: 2026-03-28*
