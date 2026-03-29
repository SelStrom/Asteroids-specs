# Requirements: Asteroids — Unity WebGL

**Defined:** 2026-03-27
**Core Value:** Играбельный Asteroids в браузере, точно воспроизводящий геймплей оригинала

---

## v1 Requirements

### Project Setup

- [ ] **SETUP-01**: Unity 2022.3 LTS проект создан с Built-in Render Pipeline, WebGL Player Settings, Gamma color space
- [ ] **SETUP-02**: UPM-пакеты подключены: `com.unity.services.authentication`, `com.unity.services.leaderboards`
- [x] **SETUP-03**: Embedded-пакет `com.shtl.mcp-unity` добавлен в `Packages/` с корректным `package.json`
- [x] **SETUP-04**: Единственная сцена `Main.unity` создана в `Assets/Scenes/`; переключение экранов через `SetActive()` на Canvas-объектах
- [x] **SETUP-05**: Sprite Atlas настроен (Allow Rotation = false, Padding = 4, Mip Maps = false) и содержит все игровые спрайты из предоставленного атласа

### Ship (Корабль игрока)

- [x] **SHIP-01**: Корабль вращается влево/вправо (клавиши A/D или стрелки) с мгновенным откликом
- [x] **SHIP-02**: Тяга (W или стрелка вверх) прибавляет скорость в направлении нос корабля (Ньютоновская физика — скорость накапливается)
- [x] **SHIP-03**: Без тяги корабль движется по инерции без замедления
- [x] **SHIP-04**: Максимальная скорость корабля ограничена константой
- [x] **SHIP-05**: Корабль телепортируется на противоположный край при выходе за границы экрана (wrap-around по X и Y)
- [x] **SHIP-06**: При уничтожении корабля отображается анимация взрыва, затем respawn через 2 секунды в центре экрана
- [x] **SHIP-07**: После respawn корабль неуязвим 3 секунды (мигание спрайта)
- [x] **SHIP-08**: Визуальный индикатор тяги (сопло огня) отображается только при нажатой тяге

### Shooting (Стрельба)

- [x] **SHOT-01**: Нажатие Space (или левая кнопка мыши) выпускает пулю из носа корабля
- [x] **SHOT-02**: Пуля движется в направлении носа корабля + добавляет скорость корабля
- [x] **SHOT-03**: Одновременно на экране не более 4 пуль
- [x] **SHOT-04**: Пуля исчезает через 0.8 секунды или при попадании в цель
- [ ] **SHOT-05**: Пули проходят wrap-around экрана аналогично кораблю
- [ ] **SHOT-06**: Пули объединены в Object Pool (нет динамического создания/удаления)

### Asteroids (Астероиды)

- [x] **AST-01**: Три размера астероидов: Large, Medium, Small — с соответствующими спрайтами
- [x] **AST-02**: Large астероид при уничтожении распадается на 2 Medium
- [x] **AST-03**: Medium астероид при уничтожении распадается на 2 Small
- [x] **AST-04**: Small астероид при уничтожении исчезает полностью
- [x] **AST-05**: Каждый астероид имеет случайную начальную скорость и угловое вращение
- [x] **AST-06**: Скорость Large < Medium < Small (осколки быстрее родителя)
- [x] **AST-07**: Астероиды применяют wrap-around экрана
- [x] **AST-08**: Новая волна начинается с 4 Large астероидами; каждая следующая +1 Large (макс. 12)
- [x] **AST-09**: Астероиды первой волны появляются на безопасном расстоянии от корабля (минимум 20% ширины экрана)
- [x] **AST-10**: Object Pool для всех астероидов всех трёх размеров

### UFO (НЛО)

- [x] **UFO-01**: Large UFO появляется случайно каждые 25–40 секунд с края экрана, движется прямолинейно с редкими сменами направления
- [x] **UFO-02**: Small UFO появляется после счёта 10 000, стреляет в направлении корабля (точный)
- [x] **UFO-03**: Large UFO стреляет в случайном направлении
- [x] **UFO-04**: UFO проходит wrap-around экрана
- [x] **UFO-05**: UFO исчезает после пересечения экрана (если не уничтожен)
- [x] **UFO-06**: На экране одновременно не более одного UFO

### Scoring & Progression (Счёт и прогрессия)

- [x] **PROG-01**: Очки начисляются: Large Asteroid 20, Medium 50, Small 100, Large UFO 200, Small UFO 1000
- [x] **PROG-02**: Счёт отображается в левом верхнем углу HUD в реальном времени
- [x] **PROG-03**: Игрок начинает с 3 жизнями; количество жизней отображается в HUD (иконки кораблей)
- [x] **PROG-04**: Дополнительная жизнь выдаётся каждые 10 000 очков (максимум 6 жизней)
- [x] **PROG-05**: При 0 жизнях — Game Over; игра прекращается, показывается экран Game Over
- [x] **PROG-06**: Текущая волна отображается в HUD при старте каждой новой волны (кратковременный баннер)
- [x] **PROG-07**: High Score текущей сессии сохраняется и отображается на экране Game Over
- [x] **PROG-08**: После Game Over доступна кнопка «Play Again» без перезапуска приложения

### Visual (Визуальная часть)

- [x] **VIS-01**: Все игровые объекты используют спрайты из предоставленного текстурного атласа
- [x] **VIS-02**: Фон — чёрный, без текстур (аутентичный стиль Asteroids)
- [x] **VIS-03**: Взрыв корабля: particle effect или анимация из спрайтов (минимум 4 фрейма)
- [x] **VIS-04**: Взрывы астероидов: particle effect соответствующего размера
- [x] **VIS-05**: HUD: счёт (верх-лево), жизни-иконки (верх-лево под счётом), High Score (верх-право)
- [x] **VIS-06**: Main Menu: название игры, кнопка Play, кнопка Leaderboard
- [x] **VIS-07**: Game Over экран: финальный счёт, кнопка «Submit Score», кнопка «Play Again», кнопка «Leaderboard»
- [x] **VIS-08**: Leaderboard экран: Top-10 список с именем и очками, позиция текущего игрока, кнопка «Back»

### Audio (Аудио)

- [x] **AUD-01**: Звук выстрела корабля (короткий щелчок/импульс)
- [x] **AUD-02**: Звук тяги корабля (шипение, зациклен пока зажата кнопка тяги)
- [x] **AUD-03**: Звук взрыва корабля (длинный взрыв)
- [x] **AUD-04**: Звуки взрывов астероидов: отдельные для Large, Medium, Small
- [x] **AUD-05**: Звук UFO (повторяющийся тон, пока UFO на экране)
- [x] **AUD-06**: Фоновый пульс из двух чередующихся нот (бит); темп нарастает по мере уменьшения числа астероидов на экране
- [x] **AUD-07**: Все звуки загружаются в AudioClip, воспроизводятся через AudioSource (объединены в пул или на GameManager)

### Leaderboards (Лидерборды)

- [x] **LEAD-01**: При первом запуске (или если нет сохранённого токена) выполняется анонимная (Guest) аутентификация через UGS Authentication SDK
- [x] **LEAD-02**: После Game Over, при нажатии «Submit Score», игрок вводит имя (max 16 символов) и счёт отправляется в UGS Leaderboard
- [x] **LEAD-03**: Экран лидерборда отображает Top-10 записей глобального лидерборда (PlayerEntry: имя + очки)
- [x] **LEAD-04**: Позиция текущего игрока отображается отдельно, даже если он не в Top-10
- [x] **LEAD-05**: При недоступности сети (или ошибке UGS) показывается сообщение об ошибке; игра продолжается без краша
- [x] **LEAD-06**: UGS Project ID и имя лидерборда задаются в ScriptableObject-конфигурации (не хардкод)

### MCP Package (com.shtl.mcp-unity)

- [ ] **MCP-01**: Пакет имеет корректную структуру: `package.json`, `Editor/`, `Runtime/`, `Editor~/Server/`, два `.asmdef`
- [ ] **MCP-02**: TypeScript MCP-сервер в `Editor~/Server/` запускается как `node dist/index.js` (stdio)
- [x] **MCP-03**: `Editor/McpUnityBridge.cs` запускает `HttpListener` на `localhost:8765` при загрузке Editor (`[InitializeOnLoad]`)
- [x] **MCP-04**: MCP tool `compile` — выполняет `AssetDatabase.Refresh()` и возвращает статус компиляции
- [x] **MCP-05**: MCP tool `play` — запускает Play Mode (`EditorApplication.isPlaying = true`)
- [x] **MCP-06**: MCP tool `stop` — останавливает Play Mode (`EditorApplication.isPlaying = false`)
- [x] **MCP-07**: MCP tool `get_game_state` — возвращает JSON с `{ score, wave, lives, isPlaying }` (из Runtime bridge)
- [x] **MCP-08**: MCP tool `list_scenes` — возвращает список `.unity` файлов из `AssetDatabase`
- [x] **MCP-09**: MCP tool `open_scene` — открывает сцену по пути (`EditorSceneManager.OpenScene`)
- [x] **MCP-10**: MCP tool `import_asset` — выполняет `AssetDatabase.ImportAsset(path)`
- [x] **MCP-11**: `Runtime/RuntimeBridgeProxy.cs` с `#if UNITY_EDITOR` guard — пишет состояние игры в статический буфер, доступный Editor bridge; в WebGL-билде компилируется в no-op заглушки
- [ ] **MCP-12**: `Editor~/Server/mcp.json` содержит конфиг для регистрации сервера в Claude Code: `{ "command": "node", "args": ["dist/index.js"] }`
- [ ] **MCP-13**: `Editor~/Server/README.md` с инструкцией: `npm install`, `npm run build`, регистрация в Claude Code

---

## v2 Requirements

### Gameplay Enhancements

- **V2-GAME-01**: Гиперпространство (Hyperspace) — случайная телепортация корабля с риском уничтожения
- **V2-GAME-02**: Режим Cooperative (2 игрока на одной клавиатуре)
- **V2-GAME-03**: Бонусные уровни / special waves

### Visual Enhancements

- **V2-VIS-01**: Шейдер свечения (glow) для пуль и кораблей в стиле аркадных автоматов
- **V2-VIS-02**: Screenshake при крупных взрывах

### Leaderboard Enhancements

- **V2-LEAD-01**: Региональные лидерборды (Top по стране)
- **V2-LEAD-02**: Еженедельный/ежемесячный сброс лидерборда

---

## Out of Scope

| Feature | Reason |
|---------|--------|
| Серверная часть (кроме UGS) | Нет своего бэкенда по условию задачи |
| Мобильные платформы | WebGL — единственный таргет |
| Multiplayer online | Высокая сложность, не часть оригинала |
| Сохранение прогресса между сессиями | Только онлайн-лидерборды через UGS |
| URP / HDRP | Built-in RP выбран для простоты WebGL |
| Нативные плагины | Несовместимы с WebGL |

---

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| SETUP-01..05 | Phase 1 | Pending |
| SHIP-01..08 | Phase 2 | Pending |
| SHOT-01..06 | Phase 2 | Pending |
| AST-01..10 | Phase 3 | Pending |
| UFO-01..06 | Phase 4 | Pending |
| PROG-01..08 | Phase 3–4 | Pending |
| VIS-01..08 | Phase 5 | Pending |
| AUD-01..07 | Phase 5 | Pending |
| LEAD-01..06 | Phase 6 | Pending |
| MCP-01..13 | Phase 7 | Pending |

**Coverage:**
- v1 requirements: 68 total
- Mapped to phases: 68
- Unmapped: 0 ✓

---
*Requirements defined: 2026-03-27*
*Last updated: 2026-03-27 after initial definition*
