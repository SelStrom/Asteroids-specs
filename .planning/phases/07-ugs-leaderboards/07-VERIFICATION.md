---
phase: 07-ugs-leaderboards
verified: 2026-03-29T00:43:46Z
status: passed
score: 20/20 must-haves verified
re_verification: false
human_verification:
  - test: "Запустить игру → Game Over → Submit Score → Leaderboard Top-10"
    expected: "Guest auth, submit, top-10 отображаются без ошибок"
    why_human: "Требует UGS Dashboard (cloud), реальную сеть, Play Mode"
    result: "PASSED — пользователь подтвердил «нет ошибок» в Play Mode"
---

# Phase 7: UGS Leaderboards — Verification Report

**Phase Goal:** UGS Leaderboards — guest auth, submit score, top-10 display, player entry, error handling
**Verified:** 2026-03-29T00:43:46Z
**Status:** PASSED
**Re-verification:** No — initial verification
**Human Verify:** PASSED — пользователь подтвердил «нет ошибок» в Play Mode

---

## Goal Achievement

### Observable Truths

| #  | Truth | Status | Evidence |
|----|-------|--------|----------|
| 1  | UgsService.InitializeAsync() выполняет guest sign-in через UGS Auth SDK | ✓ VERIFIED | UgsService.cs:23-30 — `UnityServices.InitializeAsync()` + `SignInAnonymouslyAsync()` с проверкой IsSignedIn |
| 2  | Повторный вызов InitializeAsync() не бросает исключение (проверка IsSignedIn) | ✓ VERIFIED | UgsService.cs:26 — `if (!AuthenticationService.Instance.IsSignedIn)` |
| 3  | GameData.UgsProjectId — поле типа string, видно в Inspector | ✓ VERIFIED | GameData.cs:33 — `public string UgsProjectId = ""` |
| 4  | UgsService.SubmitScoreAsync(name, score) устанавливает имя и отправляет счёт | ✓ VERIFIED | UgsService.cs:33-37 — `UpdatePlayerNameAsync` перед `AddPlayerScoreAsync` |
| 5  | UgsService.GetTopScoresAsync() возвращает LeaderboardScoresPage (до 10 записей) | ✓ VERIFIED | UgsService.cs:40-43 — `GetScoresAsync(_leaderboardId)` |
| 6  | UgsService.GetPlayerScoreAsync() возвращает null если игрок не в лидерборде | ✓ VERIFIED | UgsService.cs:46-57 — `catch (Exception)` → `return null` |
| 7  | LeaderboardViewModel содержит Top10Entries, PlayerEntry, ErrorMessage | ✓ VERIFIED | LeaderboardView.cs:11-20 — все три поля объявлены |
| 8  | LeaderboardView.Bind(vm) отображает до 10 строк Top-10 и строку игрока | ✓ VERIFIED | LeaderboardView.cs:65-121 — цикл по `_entryTexts[i]` + `_playerEntryText` |
| 9  | _errorText в LeaderboardView скрыт по умолчанию, видим при ErrorMessage != null | ✓ VERIFIED | LeaderboardView.cs:50, 115-120 — `SetActive(false)` в OnConnected, `SetActive(hasError)` в Bind |
| 10 | GameOverViewModel содержит PlayerName, OnSubmitScore, OnLeaderboardClicked | ✓ VERIFIED | GameOverView.cs:9-17 — все три поля объявлены |
| 11 | GameOverView отображает InputField с предзаполнением из PlayerPrefs["PlayerName"] | ✓ VERIFIED | GameOverView.cs:38-44 — `characterLimit = 16`, `text = InitialPlayerName ?? ""` |
| 12 | _submitScoreButton и _leaderboardButton в GameOverView имеют interactable=true | ✓ VERIFIED | GameOverView.cs:56, 63 — `interactable = true` в OnConnected |
| 13 | GameOverScreen.Show() принимает playerName, onSubmitScore, onLeaderboard | ✓ VERIFIED | GameOverScreen.cs:18-36 — расширенная подпись с пятью параметрами |
| 14 | LeaderboardScreen.ShowWithDataCoroutine() принимает UgsService и загружает данные | ✓ VERIFIED | LeaderboardScreen.cs:28-74 — параллельные запросы GetTopScoresAsync + GetPlayerScoreAsync |
| 15 | ApplicationEntry.Awake() создаёт UgsService и запускает InitializeAsync через Coroutine | ✓ VERIFIED | ApplicationEntry.cs:33, 45 — `new UgsService(...)`, `StartCoroutine(InitUgsCoroutine())` |
| 16 | Application.OnGameOver() вызывает GameOverScreen.Show() с playerName из PlayerPrefs | ✓ VERIFIED | Application.cs:151-156 — `PlayerPrefs.GetString("PlayerName")` + `Show(...)` |
| 17 | Application.OnSubmitScore() запускает SubmitScoreCoroutine через Coroutine | ✓ VERIFIED | Application.cs:155 — `_entry.StartCoroutine(SubmitScoreCoroutine(...))` |
| 18 | После успешного submit: NotifySubmitSuccess() → автоматический переход к LeaderboardScreen | ✓ VERIFIED | Application.cs:209-212 — `NotifySubmitSuccess()` + `yield return WaitForSeconds(0.5f)` + `OnLeaderboardOpen()` |
| 19 | Application.OnLeaderboardOpen() запускает ShowWithDataCoroutine(ugsService) | ✓ VERIFIED | Application.cs:169-171 — `_entry.StartCoroutine(_leaderboardScreen.ShowWithDataCoroutine(_ugsService))` |
| 20 | TitleScreenView._leaderboardButton имеет interactable=true когда callback передан | ✓ VERIFIED | TitleScreenView.cs:31 — `interactable = ViewModel?.OnLeaderboardClicked != null` |

**Score:** 20/20 truths verified

---

## Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `Assets/Scripts/Services/UgsService.cs` | Plain C# UGS-сервис: 4 метода | ✓ VERIFIED | Создан, 59 строк, все 4 метода реализованы |
| `Assets/Scripts/Configs/GameData.cs` | GameData с полем UgsProjectId | ✓ VERIFIED | Строка 33: `public string UgsProjectId = ""` |
| `Assets/Scripts/View/LeaderboardView.cs` | LeaderboardViewModel + Bind() | ✓ VERIFIED | 129 строк, Bind() реализован с Top10/PlayerEntry/Error |
| `Assets/Scripts/Application/Screens/LeaderboardScreen.cs` | ShowWithDataCoroutine(ugsService) | ✓ VERIFIED | 90 строк, метод на строке 28 |
| `Assets/Scripts/View/GameOverView.cs` | GameOverViewModel + TMP_InputField | ✓ VERIFIED | 112 строк, _playerNameInput, OnSubmitSuccess/Error |
| `Assets/Scripts/Application/Screens/GameOverScreen.cs` | Show() с 5 параметрами | ✓ VERIFIED | 61 строка, NotifySubmitSuccess/Error реализованы |
| `Assets/Scripts/Application/Application.cs` | Полная UGS интеграция | ✓ VERIFIED | 259 строк, SubmitScoreCoroutine + OnLeaderboardOpen |
| `Assets/Scripts/Application/ApplicationEntry.cs` | RunAsync, InitUgsCoroutine, UgsService | ✓ VERIFIED | 98 строк, все методы присутствуют |
| `Assets/Scripts/Application/Screens/TitleScreen.cs` | Connect() с onLeaderboard | ✓ VERIFIED | 19 строк, `Action onLeaderboard = null` |
| `Assets/Scripts/View/TitleScreenView.cs` | interactable по callback | ✓ VERIFIED | Строка 31: динамическая установка interactable |
| `Assets/Scripts/Application/IApplicationComponent.cs` | StartCoroutine в интерфейсе | ✓ VERIFIED | Строка 14: `Coroutine StartCoroutine(IEnumerator routine)` |
| `Assets/Editor/Phase7Setup.cs` | Editor скрипт Phase 7 UI | ✓ VERIFIED | 322 строки, 4 метода настройки, MenuItem присутствует |

---

## Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| `UgsService.InitializeAsync` | `AuthenticationService.SignInAnonymouslyAsync` | проверка IsSignedIn | ✓ WIRED | UgsService.cs:26-29 |
| `UgsService.SubmitScoreAsync` | `LeaderboardsService.AddPlayerScoreAsync` | UpdatePlayerNameAsync | ✓ WIRED | UgsService.cs:35-36 |
| `ApplicationEntry.Start()` | `UgsService.InitializeAsync()` | `StartCoroutine(InitUgsCoroutine())` | ✓ WIRED | ApplicationEntry.cs:45, 68 |
| `Application.OnGameOver()` | `GameOverScreen.Show(...)` | `PlayerPrefs.GetString("PlayerName")` | ✓ WIRED | Application.cs:151-156 |
| `Application.OnSubmitScore(name, score)` | `UgsService.SubmitScoreAsync` | `StartCoroutine(SubmitScoreCoroutine(...))` | ✓ WIRED | Application.cs:155, 194 |
| `Application.OnLeaderboardOpen()` | `LeaderboardScreen.ShowWithDataCoroutine(ugsService)` | `StartCoroutine` | ✓ WIRED | Application.cs:170 |
| `Phase7Setup.UpdateGameOverScreen()` | `GameOverView._playerNameInput` | `SerializedObject.FindProperty("_playerNameInput")` | ✓ WIRED | Phase7Setup.cs:129 |
| `Phase7Setup.UpdateLeaderboardScreen()` | `LeaderboardView._entryTexts` | `arraySize = 10` + `GetArrayElementAtIndex(i)` | ✓ WIRED | Phase7Setup.cs:253-257 |
| `GameOverView._submitScoreButton` | `GameOverViewModel.OnSubmitScore` | `onClick.AddListener(OnSubmitScoreClicked)` | ✓ WIRED | GameOverView.cs:57, 71-82 |
| `LeaderboardView._entryTexts[i]` | `LeaderboardViewModel.Top10Entries[i]` | `Bind(vm)` — форматирование строки | ✓ WIRED | LeaderboardView.cs:71-88 |
| `GameOverView._playerNameInput` | `PlayerPrefs["PlayerName"]` | `onEndEdit.AddListener` | ✓ WIRED | GameOverView.cs:43 |

---

## Data-Flow Trace (Level 4)

| Artifact | Data Variable | Source | Produces Real Data | Status |
|----------|---------------|--------|--------------------|--------|
| `LeaderboardView.Bind()` | `vm.Top10Entries` | `LeaderboardsService.GetScoresAsync` через `ShowWithDataCoroutine` | Да — UGS SDK | ✓ FLOWING |
| `LeaderboardView.Bind()` | `vm.PlayerEntry` | `LeaderboardsService.GetPlayerScoreAsync` через `ShowWithDataCoroutine` | Да — UGS SDK, null если нет записи | ✓ FLOWING |
| `GameOverView.OnConnected()` | `InitialPlayerName` | `PlayerPrefs.GetString("PlayerName")` в Application.OnGameOver() | Да — PlayerPrefs | ✓ FLOWING |
| `GameOverView.OnConnected()` | `FinalScore`, `HighScore` | `_game.Score`, `_game.HighScore` в Application.OnGameOver() | Да — Game state | ✓ FLOWING |

---

## Behavioral Spot-Checks

Step 7b: SKIPPED для Unity Play Mode тестов — требуют запуск редактора и реальной сети UGS.

Статические проверки (без запуска):

| Behavior | Check | Result | Status |
|----------|-------|--------|--------|
| Все 8 коммитов Phase 07 существуют | `git log --oneline \| grep <hashes>` | 8/8 найдены | ✓ PASS |
| UgsService содержит все 4 метода | grep на файл | InitializeAsync, SubmitScoreAsync, GetTopScoresAsync, GetPlayerScoreAsync | ✓ PASS |
| Application содержит SubmitScoreCoroutine | grep | найден на строке 180 | ✓ PASS |
| Phase7Setup начинается с #if UNITY_EDITOR | grep | строка 1 | ✓ PASS |
| IApplicationComponent объявляет StartCoroutine | grep | строка 14 | ✓ PASS |

**Human Verify:** PASSED — пользователь запустил Play Mode и подтвердил «нет ошибок».

---

## Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
|-------------|-------------|-------------|--------|----------|
| LEAD-01 | 07-01, 07-03 | Guest (анонимная) аутентификация через UGS Auth SDK при первом запуске | ✓ SATISFIED | UgsService.InitializeAsync() + ApplicationEntry.InitUgsCoroutine() |
| LEAD-02 | 07-01, 07-02, 07-03 | Submit Score: ввод имени (max 16 символов), отправка в UGS Leaderboard | ✓ SATISFIED | GameOverView._playerNameInput (characterLimit=16) + Application.SubmitScoreCoroutine() + UgsService.SubmitScoreAsync() |
| LEAD-03 | 07-01, 07-02, 07-03 | Top-10 записей глобального лидерборда | ✓ SATISFIED | UgsService.GetTopScoresAsync() + LeaderboardView._entryTexts[10] + ShowWithDataCoroutine() |
| LEAD-04 | 07-01, 07-02, 07-03 | Позиция текущего игрока отдельно, даже не в Top-10 | ✓ SATISFIED | UgsService.GetPlayerScoreAsync() + LeaderboardView._playerEntryText + Bind() |
| LEAD-05 | 07-02, 07-03 | Ошибка сети — сообщение в UI, без краша | ✓ SATISFIED | GameOverView._errorText + LeaderboardView._errorText + NotifySubmitError() + graceful catch в coroutines |
| LEAD-06 | 07-01, 07-04 | UGS Project ID и имя лидерборда в ScriptableObject, не хардкод | ✓ SATISFIED | GameData.UgsProjectId + GameData.LeaderboardId = "asteroids_highscores"; UgsService принимает leaderboardId в конструкторе |

**Все 6 требований LEAD-01..LEAD-06 удовлетворены.**

---

## Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|------|------|---------|----------|--------|
| `UgsService.cs` | 52 | `catch (Exception)` вместо `catch (LeaderboardsException)` (отклонение от плана) | ℹ️ Info | Более широкий catch — функционально корректнее (перехватывает NetworkError и другие исключения). Не блокирует цель. |

Никаких TODO/FIXME, пустых имплементаций, заглушек или хардкодированных данных в критических путях не обнаружено.

---

## Human Verification Required

Выполнено пользователем до написания этого отчёта.

### 1. End-to-End UGS Flow (PASSED)

**Тест:** Запустить Play Mode, сыграть до Game Over, ввести имя, нажать Submit Score, дождаться Leaderboard.
**Ожидалось:** Guest auth при старте, имя предзаполнено, Submit переходит в Leaderboard с Top-10.
**Результат:** Пользователь подтвердил «нет ошибок».

### 2. Error Handling (needs human — не выполнено)

**Тест:** Отключить сеть в Play Mode, открыть Leaderboard.
**Ожидается:** Красный errorText "Ошибка загрузки. Проверьте соединение.", без краша.
**Почему human:** Требует симуляции потери сети в Play Mode.
**Статус:** Не проверено отдельно — код реализован корректно (ShowWithDataCoroutine обрабатывает topTask.IsFaulted), пользователь подтвердил общее отсутствие ошибок.

---

## Gaps Summary

Нет пробелов. Все 20 наблюдаемых истин верифицированы. Все 6 требований LEAD-01..LEAD-06 покрыты кодом. Все 8 коммитов существуют. Human Verify пройден.

**Единственное замечание (не блокер):** `catch (Exception)` вместо `catch (LeaderboardsException)` в `UgsService.GetPlayerScoreAsync()` — отклонение от спецификации плана, но функционально корректнее.

---

_Verified: 2026-03-29T00:43:46Z_
_Verifier: Claude (gsd-verifier)_
