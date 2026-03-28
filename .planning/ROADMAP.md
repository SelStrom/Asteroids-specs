# Roadmap: Asteroids — Unity WebGL

**Milestone:** v1.0 — Complete Game
**Total Phases:** 8
**Requirements:** 68 v1 requirements
**Granularity:** Standard
**Last updated:** 2026-03-28

---

## Phases

- [x] **Phase 1: Project Foundation** — Unity проект, структура папок, пакеты, сцены, sprite atlas (completed 2026-03-27)
- [ ] **Phase 2: MCP Basic** — TypeScript MCP-сервер, Editor HTTP-мост, Editor tools (compile/play/stop/list_scenes/open_scene/import_asset)
- [ ] **Phase 3: Core Mechanics** — движение корабля, стрельба, object pool, базовые коллизии
- [ ] **Phase 4: Asteroids & Progression** — три размера астероидов, дробление, волны, очки, жизни, Game Over
- [ ] **Phase 5: UFO & Progression Polish** — Large/Small UFO, стрельба UFO, HUD-баннер волны, экстра-жизни
- [ ] **Phase 6: Audio & Visual Polish** — все спрайты из атласа, particle effects, все SFX, фоновый пульс, UI экраны
- [ ] **Phase 7: UGS Leaderboards** — guest auth, отправка счёта, экран лидерборда, обработка ошибок
- [ ] **Phase 8: MCP Runtime** — RuntimeBridgeProxy, get_game_state, интеграция игрового состояния в MCP

---

## Phase Details

### Phase 1: Project Foundation

**Goal:** Unity-проект полностью настроен, все пакеты подключены, сцены созданы, sprite atlas импортирован — можно нажать Play без ошибок.
**Depends on:** Nothing (first phase)
**Requirements:** SETUP-01, SETUP-02, SETUP-03, SETUP-04, SETUP-05
**Success Criteria** (что должно быть ПРАВДОЙ):
  1. Проект открывается в Unity 2022.3 LTS без ошибок компиляции
  2. В Package Manager присутствуют `com.unity.services.authentication` и `com.unity.services.leaderboards`
  3. Embedded-пакет `com.shtl.mcp-unity` виден в Package Manager (embedded)
  4. Единственная сцена `Assets/Scenes/Main.unity` существует; переключение экранов через `SetActive()` на Canvas-объектах
  5. Sprite Atlas содержит все игровые спрайты, в Inspector нет предупреждений о недостающих текстурах

**Plans:** 3/2 plans complete

Plans:
- [x] 01-01-PLAN.md — ProjectSettings, Physics2D, QualitySettings, Packages/manifest.json, .gitignore
- [x] 01-02-PLAN.md — Структура папок Assets, три asmdef, embedded-пакет scaffold, сцена Main.unity
- [x] 01-03-PLAN.md — Sprite Atlas (GameAtlas.spriteAtlas) и инструкция для пользователя

---

### Phase 2: MCP Basic

**Goal:** TypeScript MCP-сервер запущен, Editor HTTP-мост работает — Claude Code может компилировать скрипты, управлять Play Mode и навигировать по сценам прямо из чата.
**Depends on:** Phase 1
**Requirements:** MCP-01, MCP-02, MCP-03, MCP-04, MCP-05, MCP-06, MCP-08, MCP-09, MCP-10, MCP-12, MCP-13
**Success Criteria** (что должно быть ПРАВДОЙ):
  1. `npm install && npm run build` в `Editor~/Server/` завершается без ошибок; `node dist/index.js` запускается
  2. `McpUnityBridge.cs` автоматически стартует `HttpListener` на `localhost:8765` при загрузке Unity Editor
  3. MCP tool `compile` вызывает `AssetDatabase.Refresh()` и возвращает статус без ошибок
  4. MCP tools `play` и `stop` переключают Play Mode в Unity Editor через HTTP-запрос к bridge
  5. MCP tools `list_scenes`, `open_scene`, `import_asset` корректно работают с AssetDatabase
  6. `mcp.json` зарегистрирован в Claude Code, сервер отображается в списке MCP-инструментов

**Plans:** 1/4 plans executed

Plans:
- [ ] 02-01-PLAN.md — TypeScript scaffold (package.json, tsconfig.json, src/index.ts с 6 MCP tools)
- [x] 02-02-PLAN.md — C# HTTP bridge (McpUnityBridge.cs, McpUnityBridge.asmdef)
- [ ] 02-03-PLAN.md — Конфигурация (.mcp.json, McpUnityRuntime.asmdef, mcp.json, README.md)
- [ ] 02-04-PLAN.md — Верификация (smoke-тест сборки + ручная проверка всех 6 tools)

---

### Phase 3: Core Mechanics

**Goal:** Игрок управляет кораблём и стреляет — движение с инерцией, wrap-around, object pool пуль, корабль уничтожается при столкновении.
**Depends on:** Phase 1 (MCP Basic желательно, но не обязательно)
**Requirements:** SHIP-01, SHIP-02, SHIP-03, SHIP-04, SHIP-05, SHIP-06, SHIP-07, SHIP-08, SHOT-01, SHOT-02, SHOT-03, SHOT-04, SHOT-05, SHOT-06
**Success Criteria** (что должно быть ПРАВДОЙ):
  1. Корабль вращается по A/D, разгоняется по W с накоплением скорости (без замедления при отпускании)
  2. Корабль и пули телепортируются на противоположный край при выходе за экран
  3. Одновременно летит не более 5 пуль (DATA_SCHEMA MaxShoots=5); новая пуля не создаётся, если лимит достигнут
  4. При гибели корабля он скрывается, через 2 секунды корабль появляется в центре с 3-секундным миганием неуязвимости
  5. Визуальный индикатор сопла виден только при зажатом W

**Plans:** 2/3 plans executed

Plans:
- [x] 03-01-PLAN.md — Инфраструктура: Utils, Configs C# классы, Model ECS core, Input Actions asset
- [x] 03-02-PLAN.md — Сущности + Системы ECS, Input layer (PlayerInput, PlayerActions)
- [ ] 03-03-PLAN.md — View layer, Application layer, Prefabs, Config assets, сцена Main.unity

---

### Phase 4: Asteroids & Progression

**Goal:** Полный игровой цикл — три размера астероидов с логикой дробления, волновый спавн, счёт, жизни и экран Game Over.
**Depends on:** Phase 3
**Requirements:** AST-01, AST-02, AST-03, AST-04, AST-05, AST-06, AST-07, AST-08, AST-09, AST-10, PROG-01, PROG-02, PROG-03, PROG-04, PROG-05, PROG-07, PROG-08
**Success Criteria** (что должно быть ПРАВДОЙ):
  1. Уничтожение Large порождает 2 Medium, Medium — 2 Small, Small — исчезает; у осколков скорость выше родителя
  2. Первая волна содержит 4 Large астероида, каждая следующая +1 (макс. 12); астероиды появляются вдали от корабля
  3. Счёт в HUD обновляется в реальном времени: Large +1, Medium +2, Small +3 (DATA_SCHEMA)
  4. HUD показывает 3 жизни-иконки при старте; каждые 10 000 очков добавляется жизнь (макс. 6)
  5. При 0 жизнях открывается экран Game Over с финальным счётом и кнопками «Play Again» и «Submit Score»

**Plans:** 2/4 plans executed

Plans:
- [x] 04-01-PLAN.md — ECS+View слой: AsteroidModel расширение, AsteroidVisual/ViewModel, EntitiesCatalog.CreateAsteroid, Model.GroupCreator
- [ ] 04-02-PLAN.md — Game логика: StartWave, SpawnFragments, счёт/жизни/экстра-жизни, Game Over, Play Again (Restart)
- [x] 04-03-PLAN.md — HUD расширение (Score/Lives/HighScore) + GameOverView/GameOverScreen
- [ ] 04-04-PLAN.md — Интеграция: Application wire-up, Unity assets (prefabs + configs), checkpoint верификация

---

### Phase 5: UFO & Progression Polish

**Goal:** Оба типа UFO появляются, стреляют и уничтожаются; HUD отображает номер волны; прогрессия полностью завершена.
**Depends on:** Phase 4
**Requirements:** UFO-01, UFO-02, UFO-03, UFO-04, UFO-05, UFO-06, PROG-06
**Success Criteria** (что должно быть ПРАВДОЙ):
  1. Large UFO появляется со случайного края экрана каждые 25–40 сек, стреляет в случайном направлении, исчезает при пересечении экрана
  2. Small UFO появляется после набора 10 000 очков и стреляет точно в корабль
  3. На экране не более одного UFO одновременно; UFO проходит wrap-around
  4. В HUD кратковременно появляется баннер «Wave N» при старте каждой новой волны

**Plans:** TBD

---

### Phase 6: Audio & Visual Polish

**Goal:** Игра выглядит и звучит как оригинал — все спрайты из атласа, particle effects, все звуковые эффекты, фоновый пульс, полноценные UI-экраны.
**Depends on:** Phase 4 (Phase 5 can run in parallel)
**Requirements:** VIS-01, VIS-02, VIS-03, VIS-04, VIS-05, VIS-06, VIS-07, VIS-08, AUD-01, AUD-02, AUD-03, AUD-04, AUD-05, AUD-06, AUD-07
**Success Criteria** (что должно быть ПРАВДОЙ):
  1. Все объекты (корабль, пули, астероиды 3 размеров, UFO) отображаются спрайтами из атласа на чёрном фоне
  2. Взрыв корабля воспроизводит анимацию из 4+ фреймов; взрывы астероидов масштабируются по размеру
  3. Выстрел, тяга, взрывы и UFO-тон воспроизводятся через AudioSource без заметных задержек
  4. Фоновый пульс ускоряется при уменьшении числа астероидов на экране
  5. Экраны Main Menu, Game Over и Leaderboard имеют полноценный layout с кнопками навигации

**Plans:** TBD

---

### Phase 7: UGS Leaderboards

**Goal:** Игрок может отправить счёт после Game Over и просмотреть глобальный Top-10; сетевые ошибки не ломают игру.
**Depends on:** Phase 6
**Requirements:** LEAD-01, LEAD-02, LEAD-03, LEAD-04, LEAD-05, LEAD-06
**Success Criteria** (что должно быть ПРАВДОЙ):
  1. При первом запуске автоматически выполняется guest-аутентификация через UGS; повторные запуски используют сохранённый токен
  2. После Game Over игрок вводит имя (макс. 16 символов) и нажимает «Submit Score» — счёт появляется в лидерборде
  3. Экран лидерборда отображает Top-10 глобально + позицию текущего игрока (даже если он не в Top-10)
  4. При ошибке сети отображается сообщение об ошибке; игра продолжает работу без краша
  5. UGS Project ID и имя лидерборда вынесены в ScriptableObject — изменяются без правки кода

**Plans:** TBD

---

### Phase 8: MCP Runtime

**Goal:** MCP-сервер получает доступ к живому состоянию игры — `get_game_state` возвращает счёт, волну и жизни из Runtime; `RuntimeBridgeProxy` не ломает WebGL-сборку.
**Depends on:** Phase 4 (игровое состояние должно существовать), Phase 2 (MCP Basic)
**Requirements:** MCP-07, MCP-11
**Success Criteria** (что должно быть ПРАВДОЙ):
  1. MCP tool `get_game_state` возвращает `{ score, wave, lives, isPlaying }` во время Editor Play Mode
  2. `RuntimeBridgeProxy.cs` компилируется без ошибок в WebGL-сборке (вся логика под `#if UNITY_EDITOR`)
  3. При вызове `get_game_state` вне Play Mode возвращается `{ isPlaying: false }` без ошибок

**Plans:** TBD

---

## Progress

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Project Foundation | 3/2 | Complete   | 2026-03-27 |
| 2. MCP Basic | 1/4 | In Progress|  |
| 3. Core Mechanics | 2/3 | In Progress|  |
| 4. Asteroids & Progression | 2/4 | In Progress|  |
| 5. UFO & Progression Polish | 0/? | Not started | - |
| 6. Audio & Visual Polish | 0/? | Not started | - |
| 7. UGS Leaderboards | 0/? | Not started | - |
| 8. MCP Runtime | 0/? | Not started | - |

---

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| SETUP-01 | Phase 1 | Complete (01-01) |
| SETUP-02 | Phase 1 | Complete (01-01) |
| SETUP-03 | Phase 1 | Pending |
| SETUP-04 | Phase 1 | Pending |
| SETUP-05 | Phase 1 | Pending |
| SHIP-01 | Phase 3 | Pending |
| SHIP-02 | Phase 3 | Pending |
| SHIP-03 | Phase 3 | Pending |
| SHIP-04 | Phase 3 | Pending |
| SHIP-05 | Phase 3 | Pending |
| SHIP-06 | Phase 3 | Pending |
| SHIP-07 | Phase 3 | Pending |
| SHIP-08 | Phase 3 | Pending |
| SHOT-01 | Phase 3 | Pending |
| SHOT-02 | Phase 3 | Pending |
| SHOT-03 | Phase 3 | Pending |
| SHOT-04 | Phase 3 | Pending |
| SHOT-05 | Phase 3 | Pending |
| SHOT-06 | Phase 3 | Pending |
| AST-01 | Phase 4 | Pending (04-01) |
| AST-02 | Phase 4 | Pending (04-02) |
| AST-03 | Phase 4 | Pending (04-02) |
| AST-04 | Phase 4 | Pending (04-02) |
| AST-05 | Phase 4 | Pending (04-01) |
| AST-06 | Phase 4 | Pending (04-01) |
| AST-07 | Phase 4 | Pending (04-01) |
| AST-08 | Phase 4 | Pending (04-02) |
| AST-09 | Phase 4 | Pending (04-04) |
| AST-10 | Phase 4 | Pending (04-04) |
| UFO-01 | Phase 5 | Pending |
| UFO-02 | Phase 5 | Pending |
| UFO-03 | Phase 5 | Pending |
| UFO-04 | Phase 5 | Pending |
| UFO-05 | Phase 5 | Pending |
| UFO-06 | Phase 5 | Pending |
| PROG-01 | Phase 4 | Pending (04-02) |
| PROG-02 | Phase 4 | Pending (04-03) |
| PROG-03 | Phase 4 | Pending (04-03) |
| PROG-04 | Phase 4 | Pending (04-02) |
| PROG-05 | Phase 4 | Pending (04-04) |
| PROG-06 | Phase 5 | Pending |
| PROG-07 | Phase 4 | Pending (04-03) |
| PROG-08 | Phase 4 | Pending (04-04) |
| VIS-01 | Phase 6 | Pending |
| VIS-02 | Phase 6 | Pending |
| VIS-03 | Phase 6 | Pending |
| VIS-04 | Phase 6 | Pending |
| VIS-05 | Phase 6 | Pending |
| VIS-06 | Phase 6 | Pending |
| VIS-07 | Phase 6 | Pending |
| VIS-08 | Phase 6 | Pending |
| AUD-01 | Phase 6 | Pending |
| AUD-02 | Phase 6 | Pending |
| AUD-03 | Phase 6 | Pending |
| AUD-04 | Phase 6 | Pending |
| AUD-05 | Phase 6 | Pending |
| AUD-06 | Phase 6 | Pending |
| AUD-07 | Phase 6 | Pending |
| LEAD-01 | Phase 7 | Pending |
| LEAD-02 | Phase 7 | Pending |
| LEAD-03 | Phase 7 | Pending |
| LEAD-04 | Phase 7 | Pending |
| LEAD-05 | Phase 7 | Pending |
| LEAD-06 | Phase 7 | Pending |
| MCP-01 | Phase 2 | Pending |
| MCP-02 | Phase 2 | Pending |
| MCP-03 | Phase 2 | Pending |
| MCP-04 | Phase 2 | Pending |
| MCP-05 | Phase 2 | Pending |
| MCP-06 | Phase 2 | Pending |
| MCP-07 | Phase 8 | Pending |
| MCP-08 | Phase 2 | Pending |
| MCP-09 | Phase 2 | Pending |
| MCP-10 | Phase 2 | Pending |
| MCP-11 | Phase 8 | Pending |
| MCP-12 | Phase 2 | Pending |
| MCP-13 | Phase 2 | Pending |

**Coverage:** 68/68 v1 requirements mapped. No orphans. ✓

---

*Roadmap created: 2026-03-27*
*Last updated: 2026-03-28 — Phase 4 plans added (04-01..04-04)*
