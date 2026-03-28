---
id: SEED-002
status: dormant
planted: 2026-03-28
planted_during: v1.0 / Phase 04 asteroids-progression
trigger_when: любая фаза с Play Again, restart, или новой игрой; QA/polish milestone
scope: Small
---

# SEED-002: Убедиться, что игровой стейт полностью очищен при старте новой игры

## Why This Matters

При нажатии "Play Again" вызывается `Game.Restart()`, который сбрасывает score, lives, waveNumber.
Но могут оставаться «призраки» из предыдущей сессии:
- Объекты в пуле (bullets, asteroids), которые не были возвращены корректно
- Визуальные объекты в сцене, не связанные с живыми ModelEntity
- `_asteroidCount` начинается с 0 в StartWave, но если до CleanUp() оставались «висячие» коллбэки — может дать неверный счёт сразу
- ActionScheduler планировщик — `CleanUp()` вызывается, но надо проверить, что все scheduled actions отменены
- GameObjectPool — содержит active-объекты; если Release() не вызвался для всех → утечка

Если что-то не сброшено, вторая и последующие игры будут вести себя иначе, чем первая.

## When to Surface

**Trigger:** Любая фаза с Play Again / Restart логикой, или перед финальным QA-проходом.

Этот seed должен быть представлен при `/gsd:new-milestone` если:
- Milestone включает gameplay polish / QA
- Упоминается Play Again, restart, или повторное прохождение
- Есть баг-репорт о "странном поведении во второй игре"

## Scope Estimate

**Small** — аудит + точечные фиксы. Вероятно 1 quick task:
1. Пройтись по `Game.Restart()` → `Stop()` → `_catalog.Reset()` → `_model.CleanUp()`
2. Убедиться что Pool не содержит активных объектов после CleanUp
3. Сыграть 2 игры подряд и сравнить начальное состояние

## Breadcrumbs

- `Assets/Scripts/Application/Game.cs:94` — `Restart()` — сброс State + Start
- `Assets/Scripts/Application/Game.cs:83` — `Stop()` — отписка от событий
- `Assets/Scripts/Application/EntitiesCatalog.cs:196` — `Reset()` — очистка словарей (Pool не трогается!)
- `Assets/Scripts/Model/Model.cs:82` — `CleanUp()` — очищает _entities, _newEntities, ActionScheduler
- `Assets/Scripts/Application/Application.cs:106` — `OnPlayAgain()` — точка входа
- `Assets/Scripts/Utils/GameObjectPool.cs` — пул объектов, не имеет явного Reset()

## Notes

Обнаружено во время Phase 04 UAT. Пользователь заметил потенциальную проблему при тестировании
Play Again. Код выглядит в целом правильным (CleanUp + Reset + Start), но пул и визуальные
объекты не прошли сквозную проверку при повторном запуске.
