---
phase: 07-ugs-leaderboards
plan: 02
subsystem: ui
tags: [unity, csharp, ugs, leaderboards, mvvm, tmpro, input-field]

# Dependency graph
requires:
  - phase: 06-audio-visual-polish
    provides: LeaderboardView стаб и GameOverView стаб для расширения
provides:
  - LeaderboardViewModel с Top10Entries, PlayerEntry, ErrorMessage
  - LeaderboardView.Bind() для отображения реальных данных UGS
  - LeaderboardScreen.ShowWithDataCoroutine() для загрузки через UgsService
  - GameOverViewModel с OnSubmitScore, OnLeaderboardClicked, InitialPlayerName
  - GameOverView с TMP_InputField для имени игрока (characterLimit=16, PlayerPrefs)
  - GameOverScreen.Show() с расширенной сигнатурой (playerName, onSubmitScore, onLeaderboard)
affects: [07-03-wire-up, 07-04-setup]

# Tech tracking
tech-stack:
  added: [TMP_InputField для ввода имени игрока]
  patterns: [Bind() паттерн для обновления View после асинхронной загрузки, IEnumerator-coroutine для async Task в WebGL]

key-files:
  created: []
  modified:
    - Assets/Scripts/View/LeaderboardView.cs
    - Assets/Scripts/Application/Screens/LeaderboardScreen.cs
    - Assets/Scripts/View/GameOverView.cs
    - Assets/Scripts/Application/Screens/GameOverScreen.cs

key-decisions:
  - "LeaderboardScreen использует IEnumerator ShowWithDataCoroutine вместо async/await — WebGL ограничивает threading (Pitfall 5)"
  - "GameOverView._playerNameInput: onEndEdit сохраняет в PlayerPrefs — D-02 автосохранение без кнопки Apply"
  - "GameOverView._submitScoreButton interactable=true в Phase 7 — стаб Phase 6 был false, теперь активна"
  - "OnSubmitSuccess() деактивирует кнопку и InputField — D-03 предотвращает двойную отправку"

patterns-established:
  - "Bind(vm): отдельный метод для обновления View данными после подключения — вызывается из Coroutine"
  - "NotifySubmit*/OnSubmit* пара: Screen делегирует во View без прямого доступа к UI компонентам"

requirements-completed: [LEAD-02, LEAD-03, LEAD-04, LEAD-05]

# Metrics
duration: 2min
completed: 2026-03-29
---

# Phase 07 Plan 02: Расширение View и Screen слоёв для лидербордов и GameOver

**LeaderboardView.Bind() с Top10/PlayerEntry/ErrorMessage и GameOverView с TMP_InputField для ввода имени игрока (characterLimit=16, PlayerPrefs предзаполнение)**

## Performance

- **Duration:** 2 мин
- **Started:** 2026-03-29T00:07:00Z
- **Completed:** 2026-03-29T00:08:33Z
- **Tasks:** 3
- **Files modified:** 4

## Accomplishments

- LeaderboardViewModel расширен реальными данными: Top10Entries[], PlayerEntry, ErrorMessage
- LeaderboardView.Bind() реализован — отображает до 10 строк Top-10 и строку текущего игрока
- LeaderboardScreen.ShowWithDataCoroutine() загружает данные через UgsService параллельно (GetTopScoresAsync + GetPlayerScoreAsync)
- GameOverView получила TMP_InputField с characterLimit=16, предзаполнением и сохранением в PlayerPrefs
- GameOverScreen.Show() расширен параметрами playerName, onSubmitScore, onLeaderboard
- OnSubmitSuccess()/OnSubmitError() для обратной связи из Application после отправки счёта

## Task Commits

1. **Task 1: Расширить LeaderboardView и LeaderboardViewModel** - `c9c0db0` (feat)
2. **Task 2: Расширить LeaderboardScreen — добавить ShowWithDataCoroutine()** - `c44b48b` (feat)
3. **Task 3: Расширить GameOverView и GameOverViewModel + GameOverScreen** - `ef355cd` (feat)

## Files Created/Modified

- `Assets/Scripts/View/LeaderboardView.cs` — LeaderboardViewModel расширен, Bind() реализован, _entryTexts[10], _playerEntryText, _errorText добавлены
- `Assets/Scripts/Application/Screens/LeaderboardScreen.cs` — ShowWithDataCoroutine(UgsService) добавлен, обработка ошибок LEAD-05
- `Assets/Scripts/View/GameOverView.cs` — GameOverViewModel расширен, TMP_InputField, OnSubmitSuccess/Error, OnLeaderboardClicked
- `Assets/Scripts/Application/Screens/GameOverScreen.cs` — Show() с расширенной сигнатурой, NotifySubmitSuccess/Error

## Decisions Made

- LeaderboardScreen использует IEnumerator (Coroutine) вместо async/await — WebGL Pitfall 5: threading ограничен, Task.Run запрещён
- GameOverView сохраняет имя в PlayerPrefs через onEndEdit — автосохранение без явной кнопки (D-02)
- _submitScoreButton и _leaderboardButton стали interactable=true — стабы Phase 6 имели false

## Deviations from Plan

None — план выполнен точно как написан.

## Issues Encountered

None.

## Known Stubs

- `LeaderboardView._entryTexts` — SerializeField, массив не заполнен в Prefab/Scene (требует Phase7Setup для добавления 10 TextMeshProUGUI объектов в иерархию)
- `LeaderboardView._playerEntryText` — SerializeField, не подключён в сцене до Phase7Setup
- `GameOverView._playerNameInput` — SerializeField TMP_InputField, не подключён в Canvas до Phase7Setup

Эти стабы намеренны — подключение wire-up и Setup выполняются в Plans 03 и 04.

## Next Phase Readiness

- View и Screen слои готовы к подключению UgsService в Plan 03 (wire-up в Application.cs)
- Plan 04 (Phase7Setup) должен добавить UI элементы в Canvas: 10 строк для Top-10, _playerEntryText, TMP_InputField
- UgsService (Plan 01, wave 1) должен быть создан для компиляции LeaderboardScreen

---
*Phase: 07-ugs-leaderboards*
*Completed: 2026-03-29*
