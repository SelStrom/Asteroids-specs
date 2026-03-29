---
phase: 05-ufo-progression-polish
verified: 2026-03-28T21:00:00Z
status: human_needed
score: 14/14 must-haves verified
re_verification: null
gaps: []
human_verification:
  - test: "Large UFO появляется с края экрана через 25-40 сек"
    expected: "UFO спаунится с левого или правого края, движется прямолинейно, пересекает экран и исчезает"
    why_human: "Таймер ActionScheduler вызывается в runtime; статически невозможно проверить появление через нужный интервал"
  - test: "Large UFO стреляет в случайном направлении (UFO-03)"
    expected: "Пули вылетают из UFO в случайных направлениях во время его движения"
    why_human: "OnUfoGunShooting вызывается через цепочку ActionScheduler→GunSystem; нужен Play Mode"
  - test: "Small UFO стреляет точно в корабль (UFO-02)"
    expected: "При score >= 10000 появляется Small UFO, пули летят в направлении текущей позиции корабля"
    why_human: "Требует набора 10000 очков и визуального наблюдения в Play Mode"
  - test: "UFO исчезает после пересечения экрана без уничтожения (UFO-05)"
    expected: "После wrap-around UFO достигает стартового края и удаляется без начисления очков"
    why_human: "CheckUfoExit использует _activeUfoWrapped флаг — поведение проверяется только в Play Mode"
  - test: "Не более одного UFO одновременно (UFO-06)"
    expected: "TrySpawnUfo не создаёт UFO если _ufoActive == true; два UFO не появляются одновременно"
    why_human: "Флаговая логика проверяется только при наблюдении нескольких циклов спауна в Play Mode"
  - test: "Баннер WAVE N виден при старте каждой волны (PROG-06)"
    expected: "\"WAVE 1\" появляется при старте игры на 2.5 секунды, затем скрывается; \"WAVE 2\" при следующей волне"
    why_human: "ShowWaveBanner → HudVisual.ShowWaveBanner → SetActive(true) + скрытие через ActionScheduler; нужен Play Mode"
  - test: "UFO уничтожается от пули игрока, даёт очки (Large 200, Small 1000) (PROG-01)"
    expected: "При попадании пули игрока в UFO: UFO исчезает, счёт увеличивается на 200 или 1000"
    why_human: "OnUfoCollided → ufo.Kill() → OnEntityDestroyed → Score += data.Score; нужна визуальная проверка коллизии"
---

# Phase 5: UFO Progression Polish — Verification Report

**Phase Goal:** Оба типа UFO появляются, стреляют и уничтожаются; HUD отображает номер волны; прогрессия полностью завершена.
**Verified:** 2026-03-28T21:00:00Z
**Status:** human_needed
**Re-verification:** No — initial verification

---

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | UfoVisual компонент существует и биндит Position через ReactiveValue | VERIFIED | `Assets/Scripts/View/UfoVisual.cs` — полная реализация, не заглушка; ReactiveValue Position/Sprite bind в OnConnected() |
| 2 | ShootToComponent содержит Action<UfoModel> OnShoot для Small UFO | VERIFIED | `Assets/Scripts/Model/Components/ShootToComponent.cs` строка 10: `public Action<UfoModel> OnShoot;` |
| 3 | UFO prefabs существуют с UfoVisual компонентом | VERIFIED | `ufo_big.prefab` и `ufo_small.prefab` — 164 строки каждый; GUID `d1c575604f4c948ffa404f5902608ea9` совпадает с `UfoVisual.cs.meta` |
| 4 | ScriptableObject конфиги UfoBigData/UfoSmallData существуют | VERIFIED | Оба файла присутствуют в `Assets/Media/configs/` |
| 5 | GameData содержит ссылки на UfoBig и Ufo | VERIFIED | `EntitiesCatalog.Connect()` строки 39-40: `RegisterPrefab(_configs.UfoBig.Prefab)` и `RegisterPrefab(_configs.Ufo.Prefab)` — ссылки используются |
| 6 | HUD содержит wave_banner_text (скрыт по умолчанию) | VERIFIED | `Main.unity` строка 2692: `m_Name: wave_banner_text`, строка 2697: `m_IsActive: 0` |
| 7 | HudVisual содержит ShowWaveBanner/HideWaveBanner | VERIFIED | `Assets/Scripts/View/HudVisual.cs` строки 68-79: оба метода реализованы с null-guard |
| 8 | GroupCreator.Visit(UfoBigModel) регистрирует UFO в системах | VERIFIED | `Model.cs` строки 117-127: MoveSystem + GunSystem для всех UFO; ShootToSystem + MoveToSystem для UfoModel |
| 9 | MoveToSystem реализован (не заглушка) | VERIFIED | `MoveToSystem.cs` — 17 строк, полная логика: direction * distance, ufo.Move.Position.Value обновляется |
| 10 | ShootToSystem реализован (не заглушка) | VERIFIED | `ShootToSystem.cs` — 17 строк, полная логика: Timer -= deltaTime, OnShoot?.Invoke(ufo) |
| 11 | EntitiesCatalog.CreateUfoBig / CreateUfoSmall существуют и подключены | VERIFIED | `EntitiesCatalog.cs` строки 172-241: оба метода с полным паттерном Model+View+ViewModel+Register |
| 12 | Game.cs содержит полную UFO логику (спаун, стрельба, уничтожение) | VERIFIED | `Game.cs` строки 31-567: все методы присутствуют (ScheduleUfoSpawn, SpawnUfoBig, SpawnUfoSmall, CheckUfoExit, OnUfoGunShooting, OnUfoSmallShoot, OnUfoCollided, UpdateUfoTarget) |
| 13 | Application.cs вызывает _game.Update и передаёт wave banner callbacks | VERIFIED | `Application.cs` строки 92-94, 130: `_game?.Update(deltaTime)` и `onWaveBannerShow: wave => _hudVisual?.ShowWaveBanner(wave)` |
| 14 | UFO префабы зарегистрированы в пуле при старте | VERIFIED | `EntitiesCatalog.Connect()` строки 39-40: RegisterPrefab для обоих UFO prefabs |

**Score:** 14/14 truths verified (статически)

---

### Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `Assets/Scripts/View/UfoVisual.cs` | UfoViewModel + UfoVisual MonoBehaviour | VERIFIED | Полная реализация: ReactiveValue биндинг, IEntityView, OnCollisionEnter2D |
| `Assets/Scripts/Model/Components/ShootToComponent.cs` | ShootToComponent с Action<UfoModel> OnShoot | VERIFIED | ShootInterval, Timer, OnShoot — все поля присутствуют |
| `Assets/Scripts/View/HudVisual.cs` | ShowWaveBanner(int) и HideWaveBanner() | VERIFIED | Оба метода с null-guard, _waveBannerText field |
| `Assets/Editor/Phase5Setup.cs` | Editor setup для UFO assets и HUD | VERIFIED | MenuItem "Asteroids/Setup Phase 5 Assets", все методы CreateUfoPrefabs/CreateUfoConfigs/UpdateGameData/SetupWaveBanner |
| `Assets/Media/prefabs/ufo_big.prefab` | Large UFO prefab с UfoVisual | VERIFIED | 164 строки, GUID UfoVisual подтверждён в m_Script |
| `Assets/Media/prefabs/ufo_small.prefab` | Small UFO prefab с UfoVisual | VERIFIED | 164 строки, GUID UfoVisual подтверждён |
| `Assets/Media/configs/UfoBigData.asset` | Large UFO ScriptableObject конфиг | VERIFIED | Файл существует (создан Phase5Setup) |
| `Assets/Media/configs/UfoSmallData.asset` | Small UFO ScriptableObject конфиг | VERIFIED | Файл существует (создан Phase5Setup) |
| `Assets/Scripts/Model/Systems/MoveToSystem.cs` | Движение Small UFO к цели | VERIFIED | direction * distance, ufo.Move.Position.Value — не заглушка |
| `Assets/Scripts/Model/Systems/ShootToSystem.cs` | Таймер стрельбы + OnShoot | VERIFIED | shootTo.Timer -= deltaTime, OnShoot?.Invoke(ufo) — не заглушка |
| `Assets/Scripts/Model/Model.cs` | GroupCreator.Visit(UfoBigModel) полный | VERIFIED | MoveSystem+GunSystem+ShootToSystem+MoveToSystem; комментарий "Phase 4 заглушка" остался только в RegisterSystem строках 29-30 (информационный, не мешает) |
| `Assets/Scripts/Application/EntitiesCatalog.cs` | CreateUfoBig/CreateUfoSmall/Release | VERIFIED | Все методы полностью реализованы; Release ветка UfoVisual строка 261 |
| `Assets/Scripts/Application/Game.cs` | Полная UFO game-логика + wave banner | VERIFIED | Update(dt), ScheduleUfoSpawn, SpawnUfoBig/Small, CheckUfoExit, ShowWaveBanner/HideWaveBanner |
| `Assets/Scripts/Application/Application.cs` | _game.Update + wave banner wire-up | VERIFIED | OnUpdate строки 128-131, OnGameStart строки 92-94 |

---

### Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| `ShootToSystem.cs` | `ShootToComponent.cs` | `shootTo.OnShoot?.Invoke(ufo)` | WIRED | Строка 14: прямой вызов callback |
| `EntitiesCatalog.cs` | `UfoVisual.cs` | `_pool.Get<UfoVisual>(prefab)` | WIRED | Строки 188 и 225: Get<UfoVisual> для обоих UFO типов |
| `EntitiesCatalog.cs` | `Model.cs` | `_modelFactory.Create<UfoBigModel>()` | WIRED | Строка 181 в CreateUfoBig; ModelFactory.Create → Model.AddEntity → GroupCreator.Visit |
| `Game.cs` | `EntitiesCatalog.cs` | `_catalog.CreateUfoBig() / CreateUfoSmall()` | WIRED | Строки 430 и 454 в SpawnUfoBig/SpawnUfoSmall |
| `Game.cs` | `HudVisual.cs` | `_onWaveBannerShow?.Invoke(wave)` | WIRED | Game строка 558; Application строка 93 lambda: `_hudVisual?.ShowWaveBanner(wave)` |
| `Application.cs` | `Game.cs` | `_game.Update(deltaTime)` | WIRED | Application строка 130: `_game?.Update(deltaTime)` |
| `Phase5Setup.cs` | `GameData.asset` | `FindProperty("UfoBig").objectReferenceValue` | WIRED | Phase5Setup.cs строки 359-365: FindProperty("UfoBig") и FindProperty("Ufo") с ApplyModifiedProperties |

---

### Data-Flow Trace (Level 4)

| Artifact | Data Variable | Source | Produces Real Data | Status |
|----------|--------------|--------|--------------------|--------|
| `UfoVisual.cs` | `vm.Position` | `bind.From(model.Move.Position)` в EntitiesCatalog | `model.Move.Position.Value` обновляется MoveSystem каждый кадр | FLOWING |
| `UfoVisual.cs` | `vm.Sprite` | Не устанавливается в CreateUfoBig/CreateUfoSmall | UfoData не содержит Sprite поля; SpriteRenderer на prefab сохраняет спрайт из Editor при инстанцировании | INFO — спрайт из prefab SpriteRenderer, не из ViewModel |
| `HudVisual.cs` (_waveBannerText) | `_waveBannerText.text` | `ShowWaveBanner(int wave)` → `$"WAVE {wave}"` | Game._waveNumber инкрементируется в StartWave() | FLOWING |
| `Game.cs` (_ufoActive) | `_ufoActive` флаг | SpawnUfoBig/SpawnUfoSmall устанавливают true; OnEntityDestroyed/CheckUfoExit сбрасывают | Реальный игровой флаг, не статика | FLOWING |

---

### Behavioral Spot-Checks

Фаза 5 производит Unity-специфичный runtime код (MonoBehaviours, ScriptableObjects, Play Mode логика). Запуск без Unity Editor невозможен. Spot-checks ПРОПУЩЕНЫ — нет runnable entry points без Unity Play Mode.

---

### Requirements Coverage

| Requirement | Source Plan | Описание | Status | Evidence |
|-------------|------------|----------|--------|---------|
| UFO-01 | 05-02, 05-03, 05-04 | Large UFO появляется каждые 25-40 сек с края экрана | SATISFIED (runtime verify needed) | Game.ScheduleUfoSpawn → Random.Range(25f,40f) → TrySpawnUfo → SpawnUfoBig |
| UFO-02 | 05-01, 05-02, 05-03, 05-04 | Small UFO после 10000 очков, стреляет в корабль | SATISFIED (runtime verify needed) | TrySpawnUfo: `if (_model.Score >= 10000) SpawnUfoSmall()`; OnUfoSmallShoot: dir = (shipPos-ufoPos).normalized |
| UFO-03 | 05-01, 05-03, 05-04 | Large UFO стреляет в случайном направлении | SATISFIED (runtime verify needed) | OnUfoGunShooting: `randomDir = Random.insideUnitCircle.normalized` |
| UFO-04 | 05-03 | UFO проходит wrap-around экрана | SATISFIED | MoveSystem (уже реализован для всех сущностей) регистрируется для UfoBigModel через GroupCreator.Visit |
| UFO-05 | 05-04 | UFO исчезает после пересечения экрана | SATISFIED (runtime verify needed) | CheckUfoExit: wrap-around detection + `_activeUfo.Kill()` без очков |
| UFO-06 | 05-04 | Не более одного UFO одновременно | SATISFIED (runtime verify needed) | TrySpawnUfo: `if (!_isRunning || _ufoActive) { return; }` |
| PROG-06 | 05-02, 05-04 | Текущая волна отображается в HUD при старте волны | SATISFIED (runtime verify needed) | StartWave() → ShowWaveBanner() → `_onWaveBannerShow?.Invoke(_waveNumber)` → HudVisual.ShowWaveBanner |

**Итого:** 7/7 требований имеют реализацию в коде. Все 7 требуют подтверждения в Play Mode.

---

### Orphaned Requirements

REQUIREMENTS.md Traceability секция относит UFO-01..06 к Phase 4 (в таблице), однако фактическая реализация выполнена в Phase 5. Расхождение только документальное — само REQUIREMENTS.md помечает UFO-01..06 как `[x]` (выполнены). Orphaned requirements отсутствуют.

---

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|------|------|---------|----------|--------|
| `Model.cs` | 29-30 | Комментарий "Phase 4 заглушка" у RegisterSystem(new ShootToSystem()) и RegisterSystem(new MoveToSystem()) | Info | Устаревший комментарий — системы теперь полностью реализованы; не влияет на функциональность |

Никаких реальных заглушек, empty-реализаций или TODO/FIXME в коде Phase 5 не обнаружено.

---

### Human Verification Required

#### 1. Large UFO появляется и пересекает экран (UFO-01, UFO-04, UFO-05)

**Test:** В Unity Editor нажать Play → пройти TitleScreen → ждать 25-40 секунд
**Expected:** UFO появляется с левого или правого края экрана, движется прямолинейно, телепортируется при wrap-around, исчезает после второго пересечения края
**Why human:** ActionScheduler-таймеры и MoveSystem-обновление позиции работают только в Play Mode

#### 2. Large UFO стреляет случайно (UFO-03)

**Test:** Наблюдать UFO в Play Mode пока он движется по экрану
**Expected:** Периодически (примерно раз в 1.5 сек) из UFO вылетают пули в разных направлениях
**Why human:** ScheduleUfoShoot рекурсивно вызывается через ActionScheduler; визуальное подтверждение

#### 3. Small UFO стреляет в корабль (UFO-02)

**Test:** Набрать 10000+ очков → дождаться следующего спауна UFO
**Expected:** Появляется меньший UFO, его пули летят в текущем направлении корабля
**Why human:** Требует достижения порогового счёта и наблюдения за прицеливанием

#### 4. Не более одного UFO (UFO-06)

**Test:** Наблюдать несколько циклов спауна UFO (несколько минут Play Mode)
**Expected:** На экране никогда не появляются два UFO одновременно
**Why human:** Флаговая логика _ufoActive проверяется только при длительном наблюдении

#### 5. Баннер WAVE N (PROG-06)

**Test:** Запустить Play Mode → нажать Play на TitleScreen
**Expected:** В центре экрана появляется текст "WAVE 1" на ~2.5 секунды, затем исчезает; при следующей волне — "WAVE 2"
**Why human:** HudVisual.ShowWaveBanner → SetActive(true) + ActionScheduler для скрытия

#### 6. UFO уничтожается от пули и даёт очки (PROG-01)

**Test:** В Play Mode выстрелить в движущийся UFO
**Expected:** UFO исчезает, счёт увеличивается на 200 (Large) или 1000 (Small)
**Why human:** Коллизионная система Unity + OnUfoCollided → Kill() → OnEntityDestroyed → Score

---

### Gaps Summary

Gaps отсутствуют. Вся кодовая инфраструктура Phase 5 реализована корректно:

- **Plan 01:** UfoVisual.cs и ShootToComponent полностью реализованы по эталонному паттерну
- **Plan 02:** Phase5Setup.cs создаёт assets через Editor; HudVisual расширен wave banner методами
- **Plan 03:** MoveToSystem и ShootToSystem реализованы (не заглушки); EntitiesCatalog создаёт и освобождает UFO
- **Plan 04:** Game.cs содержит полную UFO логику (12 новых методов); Application.cs передаёт callbacks и вызывает Game.Update
- **Plan 05:** Phase5Setup запущен — все runtime assets (prefabs, ScriptableObjects) созданы в Unity Editor; wave_banner_text добавлен в сцену и скрыт по умолчанию

Все 7 требований (UFO-01..06, PROG-06) имеют полную реализацию в коде. Верификация требует подтверждения в Unity Play Mode.

---

_Verified: 2026-03-28T21:00:00Z_
_Verifier: Claude (gsd-verifier)_
