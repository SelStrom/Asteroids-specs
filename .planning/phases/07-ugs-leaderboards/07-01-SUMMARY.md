---
phase: 07-ugs-leaderboards
plan: 01
subsystem: ugs-service
tags: [ugs, leaderboards, authentication, service]
dependency_graph:
  requires: []
  provides: [UgsService, GameData.UgsProjectId]
  affects: [ApplicationEntry, Application, GameData]
tech_stack:
  added: []
  patterns: [plain-C#-service, AudioManager-pattern, async-Task]
key_files:
  created:
    - Assets/Scripts/Services/UgsService.cs
  modified:
    - Assets/Scripts/Configs/GameData.cs
decisions:
  - "UgsService: plain C# класс без MonoBehaviour по паттерну AudioManager (D-08)"
  - "IsSignedIn проверка перед SignInAnonymouslyAsync — Pitfall 3"
  - "catch LeaderboardsException в GetPlayerScoreAsync возвращает null — Pitfall 6"
  - "GameData.UgsProjectId — документационное поле, SDK читает ID из Project Settings (Pitfall 1)"
metrics:
  duration: "43s"
  completed: "2026-03-29T00:07:30Z"
  tasks_completed: 2
  tasks_total: 2
  files_created: 1
  files_modified: 1
---

# Phase 7 Plan 01: UgsService и GameData.UgsProjectId Summary

**Одна строка:** Plain C# UgsService с guest-аутентификацией и всеми 4 leaderboard-методами плюс поле UgsProjectId в GameData.

## Результаты

Создан `UgsService` — фундамент для всей UGS-интеграции в Phase 7. Класс следует паттерну `AudioManager` (обычный C# без MonoBehaviour), что обеспечивает совместимость с WebGL (Pitfall 5: нет Task.Run).

`GameData` расширен полем `UgsProjectId` — документационным полем рядом с `LeaderboardId` (LEAD-06, D-07).

## Выполненные задачи

| Задача | Имя | Коммит | Файлы |
|--------|-----|--------|-------|
| 1 | Создать UgsService | 85c9989 | Assets/Scripts/Services/UgsService.cs (создан) |
| 2 | Расширить GameData — добавить UgsProjectId | db5cf5e | Assets/Scripts/Configs/GameData.cs (изменён) |

## Артефакты

### Assets/Scripts/Services/UgsService.cs

Предоставляет: `UgsService` — plain C# сервис с методами:
- `InitializeAsync()` — guest sign-in с проверкой IsSignedIn (LEAD-01, Pitfall 3)
- `SubmitScoreAsync(name, score)` — UpdatePlayerNameAsync перед submit (LEAD-02)
- `GetTopScoresAsync()` — возвращает `LeaderboardScoresPage` Top-10 (LEAD-03)
- `GetPlayerScoreAsync()` — возвращает null если игрок не в лидерборде (LEAD-04, Pitfall 6)

### Assets/Scripts/Configs/GameData.cs

Добавлено поле `public string UgsProjectId = ""` рядом с `LeaderboardId` (D-07, LEAD-06).
Поле документационное — SDK читает Project ID из Project Settings, не из ScriptableObject (Pitfall 1).

## Принятые решения

1. **UgsService без MonoBehaviour** — паттерн AudioManager (D-08). Все async операции через Task, оборачиваются в Coroutine в ApplicationEntry (D-09).
2. **Pitfall 3 обработан** — `if (!AuthenticationService.Instance.IsSignedIn)` перед `SignInAnonymouslyAsync()`.
3. **Pitfall 6 обработан** — `catch (LeaderboardsException)` в `GetPlayerScoreAsync()` возвращает null вместо исключения.
4. **Pitfall 1 учтён** — `UgsProjectId` в GameData только для справки разработчика, не передаётся в `InitializeAsync()`.

## Отклонения от плана

Нет — план выполнен точно как написан.

## Известные заглушки

Нет — созданные файлы полностью функциональны. UgsService готов к подключению в ApplicationEntry (следующие планы фазы 7).

## Self-Check: PASSED

- FOUND: Assets/Scripts/Services/UgsService.cs
- FOUND: Assets/Scripts/Configs/GameData.cs
- FOUND commit 85c9989: feat(07-01): создать UgsService
- FOUND commit db5cf5e: feat(07-01): добавить GameData.UgsProjectId
