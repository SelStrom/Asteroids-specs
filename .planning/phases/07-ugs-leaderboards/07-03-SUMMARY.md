---
phase: 07-ugs-leaderboards
plan: "03"
subsystem: application-wiring
tags: [ugs, leaderboard, application, coroutine, navigation]
dependency_graph:
  requires: [07-01, 07-02]
  provides: [full-ugs-flow, leaderboard-navigation]
  affects: [Application.cs, ApplicationEntry.cs, TitleScreen.cs]
tech_stack:
  added: []
  patterns: [RunAsync-coroutine-wrapper, PlayerPrefs-name-persistence, graceful-ugs-error-handling]
key_files:
  created: []
  modified:
    - Assets/Scripts/Application/Application.cs
    - Assets/Scripts/Application/ApplicationEntry.cs
    - Assets/Scripts/Application/IApplicationComponent.cs
    - Assets/Scripts/Application/Screens/TitleScreen.cs
    - Assets/Scripts/View/TitleScreenView.cs
decisions:
  - "IApplicationComponent расширен StartCoroutine — Application не MonoBehaviour, но запускает Coroutine через entry"
  - "InitUgsCoroutine использует прямой while(!task.IsCompleted) без RunAsync — graceful обработка ошибок без throw"
  - "OnLeaderboardBack скрывает LeaderboardGo — согласованность с остальными экранами"
metrics:
  duration: "5 min"
  completed: "2026-03-29T00:12:47Z"
  tasks_completed: 2
  files_modified: 5
---

# Phase 7 Plan 03: Application UGS Wire-up Summary

**One-liner:** Полный UGS wire-up — инициализация, submit score, навигация GameOver→Leaderboard через Coroutine-паттерн.

## Objective

Подключить UgsService к Application и ApplicationEntry: инициализация при старте, передача callbacks в экраны, навигация GameOver→Submit→Leaderboard, открытие Leaderboard из TitleScreen.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Расширить TitleScreen — onLeaderboard callback и активация кнопки | f35ce6d | TitleScreen.cs, TitleScreenView.cs |
| 2 | Wire-up Application.cs и ApplicationEntry.cs | 302de33 | Application.cs, ApplicationEntry.cs, IApplicationComponent.cs |

## What Was Built

### Task 1: TitleScreen расширен

- `TitleScreen.Connect()` принимает опциональный `Action onLeaderboard = null`
- `TitleScreenViewModel.OnLeaderboardClicked` устанавливается из callback
- `TitleScreenView.OnConnected()`: кнопка Leaderboard `interactable = ViewModel?.OnLeaderboardClicked != null`
- Кнопка автоматически активируется когда Application передаёт `OnLeaderboardOpen` callback

### Task 2: Application + ApplicationEntry полностью подключены

**IApplicationComponent** — добавлен `Coroutine StartCoroutine(IEnumerator routine)`. Позволяет Application (не MonoBehaviour) запускать корутины через entry-компонент.

**Application.cs:**
- Поле `private UgsService _ugsService`
- `Connect()` принимает `UgsService ugsService = null`
- `TitleScreen.Connect()` вызывается с `OnLeaderboardOpen` callback
- `OnGameOver()` расширен: `PlayerPrefs.GetString("PlayerName")` + `GameOverScreen.Show(...)` с полными callbacks
- `OnLeaderboardOpen()`: скрывает GameOver/Title, показывает Leaderboard с данными UGS (или пустой при недоступности)
- `SubmitScoreCoroutine()`: сохраняет имя в PlayerPrefs, ждёт `SubmitScoreAsync`, на успех — `NotifySubmitSuccess()` + автоматический переход к Leaderboard, на ошибку — `NotifySubmitError()`
- `OnLeaderboardBack()`: скрывает LeaderboardGo + показывает TitleScreen

**ApplicationEntry.cs:**
- Создаёт `UgsService` с `_configs.LeaderboardId` в `Awake()`
- Передаёт `_ugsService` в `Application.Connect()`
- `Start()` запускает `StartCoroutine(InitUgsCoroutine())` + `_application.Start()`
- `InitUgsCoroutine()`: graceful await без throw, логирует успех/ошибку
- `RunAsync()`: static Coroutine-обёртка для Task (паттерн D-09, бросает при ошибке)

## Navigation Flow

```
TitleScreen → [Play] → Game → [GameOver] → GameOverScreen
                                               ↓ [Submit]
                                         SubmitScoreCoroutine
                                               ↓ success
                                         LeaderboardScreen ← [Back] → TitleScreen
                                               ↑
TitleScreen → [Leaderboard] ──────────────────┘
```

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] InitUgsCoroutine без throw в RunAsync**

- **Found during:** Task 2 код-ревью
- **Issue:** `RunAsync()` бросает исключение при faulted task. Если `InitUgsCoroutine` использует `yield return RunAsync(task)`, код после `yield return` не выполнится при ошибке — `task.IsFaulted` не проверится.
- **Fix:** `InitUgsCoroutine` использует прямой `while (!task.IsCompleted) { yield return null; }` вместо `RunAsync`. `RunAsync` оставлен как статический метод для случаев, где нужен throw-upon-fault паттерн.
- **Files modified:** Assets/Scripts/Application/ApplicationEntry.cs
- **Commit:** 302de33

**2. [Rule 2 - Missing] Добавить StartCoroutine в IApplicationComponent**

- **Found during:** Task 2 — Application.cs вызывает `_entry.StartCoroutine()`, но `IApplicationComponent` не объявляет метод
- **Issue:** Код не скомпилируется — интерфейс не имеет `StartCoroutine`
- **Fix:** Добавлен `Coroutine StartCoroutine(IEnumerator routine)` в IApplicationComponent. MonoBehaviour реализует автоматически.
- **Files modified:** Assets/Scripts/Application/IApplicationComponent.cs
- **Commit:** 302de33

## Requirements Coverage

| Requirement | Status | Implementation |
|-------------|--------|----------------|
| LEAD-01 | Covered | `InitUgsCoroutine()` — guest sign-in при старте |
| LEAD-02 | Covered | `SubmitScoreCoroutine()` + `PlayerPrefs` имя |
| LEAD-03 | Covered | `OnLeaderboardOpen()` → `ShowWithDataCoroutine()` |
| LEAD-04 | Covered | `ShowWithDataCoroutine()` включает `GetPlayerScoreAsync()` |
| LEAD-05 | Covered | `NotifySubmitError()`, `NotifySubmitSuccess()`, логирование |

## Known Stubs

Нет — все callbacks подключены к реальным UGS операциям.

## Self-Check: PASSED

- `f35ce6d` существует (Task 1 commit)
- `302de33` существует (Task 2 commit)
- Application.cs содержит все требуемые методы
- ApplicationEntry.cs содержит InitUgsCoroutine и RunAsync
- IApplicationComponent.cs содержит StartCoroutine
