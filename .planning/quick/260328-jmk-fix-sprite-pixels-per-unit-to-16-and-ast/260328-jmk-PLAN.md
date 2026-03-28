---
phase: quick
plan: 260328-jmk
type: execute
wave: 1
depends_on: []
files_modified:
  - Assets/Media/sprites/asteroids.png.meta
  - Assets/Scripts/Application/Game.cs
autonomous: true
requirements: []

must_haves:
  truths:
    - "Спрайты в игре отображаются с pixelsPerUnit=16 (не 100)"
    - "Большой астероид при попадании пули дробится на два средних"
    - "Средний астероид при попадании пули дробится на два малых"
    - "Малый астероид при попадании пули исчезает без осколков"
  artifacts:
    - path: "Assets/Media/sprites/asteroids.png.meta"
      provides: "spritePixelsToUnits: 16"
    - path: "Assets/Scripts/Application/Game.cs"
      provides: "Корректная логика дробления — баг исправлен"
  key_links:
    - from: "OnAsteroidCollided"
      to: "SpawnFragments via OnEntityDestroyed"
      via: "asteroid.Kill() → model.OnEntityDestroyed → SpawnFragments"
---

<objective>
Два исправления: (1) pixelsPerUnit спрайтов 100→16, (2) баг: большой астероид не дробится при попадании пули.

Purpose: Спрайты с PPU=100 отображаются слишком маленькими для игрового поля; дробление астероидов — ключевая механика Asteroids.
Output: Исправленный .meta файл и логика коллизий/дробления.
</objective>

<execution_context>
@$HOME/.claude/get-shit-done/workflows/execute-plan.md
@$HOME/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/STATE.md
@Assets/Media/sprites/asteroids.png.meta
@Assets/Scripts/Application/Game.cs
@Assets/Scripts/Application/EntitiesCatalog.cs
@Assets/Scripts/View/AsteroidVisual.cs
</context>

<tasks>

<task type="auto">
  <name>Задача 1: Исправить pixelsPerUnit в asteroids.png.meta</name>
  <files>Assets/Media/sprites/asteroids.png.meta</files>
  <action>
В файле `Assets/Media/sprites/asteroids.png.meta` заменить значение поля `spritePixelsToUnits` с `100` на `16`.

Строка для замены (строка 87):
  spritePixelsToUnits: 100
Заменить на:
  spritePixelsToUnits: 16

Это единственный sprite sheet проекта — содержит все спрайты: asteroid_big_*, asteroid_medium_*, ship, ship_idle, ship_throttle, bullet, bullet_particle, ufo_big.

Других .meta файлов для спрайтов нет (проверено: в Assets/Media/sprites/ только asteroids.png.meta).

После изменения Unity автоматически переимпортирует текстуру при следующем открытии редактора.
  </action>
  <verify>
    <automated>grep "spritePixelsToUnits" /Users/selstrom/work/projects/asteroids-specs/Assets/Media/sprites/asteroids.png.meta</automated>
  </verify>
  <done>grep выводит `spritePixelsToUnits: 16`</done>
</task>

<task type="auto">
  <name>Задача 2: Исправить баг дробления астероидов</name>
  <files>Assets/Scripts/Application/Game.cs</files>
  <action>
**Диагностика бага:**

В `Game.OnAsteroidCollided` вызывается `asteroid.Kill()`, что через `model.OnEntityDestroyed` → `OnEntityDestroyed` → `SpawnFragments` создаёт дочерние астероиды. Цепочка выглядит корректно. Однако есть две проблемы:

**Проблема А — отсутствие фильтрации коллизии:**
`OnAsteroidCollided` вызывается при ЛЮБОМ столкновении астероида (с пулей, с кораблём, с другим астероидом). При столкновении корабля с астероидом срабатывают оба коллбэка: `OnShipCollided` И `OnAsteroidCollided`. Нужно фильтровать — дробить астероид только если столкновение с пулей (не с кораблём).

**Проблема Б — возможный двойной вызов:**
Если два объекта одновременно сталкиваются с одним астероидом — `OnAsteroidCollided` может быть вызван дважды. Первая проверка `asteroid.IsDead()` защищает от этого, но только если `Kill()` синхронный (он синхронный, значит ОК).

**Исправление:**

В методе `OnAsteroidCollided` добавить проверку: дробить астероид только при столкновении с пулей (BulletVisual).

```csharp
private void OnAsteroidCollided(AsteroidModel asteroid, Collision2D col)
{
    if (!_isRunning || asteroid.IsDead()) { return; }

    // Дробить только при попадании пули (не при столкновении с кораблём)
    var hitModel = _catalog.GetModelByGo(col.gameObject);
    if (hitModel is not BulletModel) { return; }

    // Начислить очки (D-01: DATA_SCHEMA значения)
    var data = GetAsteroidData(asteroid.Size);
    _model.Score += data.Score; // Big=1, Medium=2, Small=3

    // Проверить экстра-жизнь (PROG-04: каждые 10 000 очков, макс. 6)
    while (_model.Score >= _nextBonusLifeScore && _lives < 6)
    {
        _lives++;
        _nextBonusLifeScore += 10000;
    }

    // Уничтожить астероид
    asteroid.Kill();

    // Уведомить HUD
    _onScoreChanged?.Invoke(_model.Score, _lives);
}
```

**Важно:** `_catalog.GetModelByGo(col.gameObject)` использует существующий `_goToModel` словарь в `EntitiesCatalog`. Метод `GetModelByGo` уже реализован в `EntitiesCatalog`.

**Дополнительная проверка настройки Layer Collision Matrix:**
STATE.md фиксирует: `m_LayerCollisionMatrix: открытая матрица — финальная настройка через Unity Editor`. Если Layer Collision Matrix в ProjectSettings не настроена так, чтобы слой Bullet сталкивался со слоем Asteroid — `OnCollisionEnter2D` вообще не будет вызываться. Исполнителю нужно через MCP Unity проверить слои gameObjects в сцене:

1. Проверить слой bullet prefab и asteroid prefab через MCP.
2. Если bullet/asteroid на Default слое — коллизии работают (Default сталкивается с Default).
3. Если они на разных слоях — проверить TagManager.asset и настроить матрицу.

Проверить через MCP: `get_gameobject_info` для bullet.prefab и asteroid_big.prefab — посмотреть layer.
  </action>
  <verify>
    <automated>grep -n "hitModel is not BulletModel\|GetModelByGo(col.gameObject)" /Users/selstrom/work/projects/asteroids-specs/Assets/Scripts/Application/Game.cs</automated>
  </verify>
  <done>
  - grep находит строки с фильтрацией по BulletModel
  - В Unity: запуск игры, выстрел в большой астероид — появляются два средних
  - Столкновение корабля с астероидом НЕ дробит астероид (только убивает корабль)
  </done>
</task>

</tasks>

<verification>
1. `grep "spritePixelsToUnits" Assets/Media/sprites/asteroids.png.meta` → выводит `16`
2. Код Game.cs компилируется без ошибок (проверить через MCP Unity: compile)
3. В Play Mode: пуля попадает в большой астероид → 2 средних астероида появляются
4. В Play Mode: корабль сталкивается с астероидом → корабль гибнет, астероид НЕ дробится (остаётся целым или исчезает по другой логике)
</verification>

<success_criteria>
- spritePixelsToUnits: 16 в asteroids.png.meta
- OnAsteroidCollided фильтрует по BulletModel
- Дробление большой→2 средних работает в Play Mode
- Проект компилируется без ошибок
</success_criteria>

<output>
После завершения создать `.planning/quick/260328-jmk-fix-sprite-pixels-per-unit-to-16-and-ast/260328-jmk-SUMMARY.md`
</output>
