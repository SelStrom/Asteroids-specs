# Phase 7: UGS Leaderboards — Research

**Researched:** 2026-03-29
**Domain:** Unity Gaming Services Authentication + Leaderboards SDK, C# async/await, Unity WebGL networking
**Confidence:** HIGH (SDK уже в manifest.json, API подтверждены официальной документацией)

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

- **D-01:** InputField для имени размещается прямо на GameOverScreen — поле видно сразу, рядом с кнопкой Submit Score. Макс. 16 символов (LEAD-02).
- **D-02:** Имя сохраняется в PlayerPrefs (`"PlayerName"`) — при следующем Game Over поле уже заполнено.
- **D-03:** После успешного Submit кнопка Submit Score становится неактивной (предотвращает повторную отправку в той же сессии).
- **D-04:** Top-10 отображается как 10 статичных TextMeshProUGUI строк (без ScrollRect). Каждая строка: `«позиция. имя  счёт»`.
- **D-05:** Позиция текущего игрока отображается отдельной строкой под разделителем. Всегда видна даже если игрок не в Top-10.
- **D-06:** LeaderboardView реализует метод `Bind(LeaderboardViewModel vm)` — `LeaderboardViewModel` расширяется реальными данными (`LeaderboardEntry[]` Top10 + `PlayerEntry` current).
- **D-07:** `GameData` расширяется полем `string UgsProjectId` рядом с `LeaderboardId` (LEAD-06).
- **D-08:** Отдельный C# класс `UgsService` (не MonoBehaviour). Методы `InitializeAsync()`, `SubmitScoreAsync(name, score)`, `GetLeaderboardAsync()`.
- **D-09:** Все async операции через `Task`, оборачиваются в Coroutine только в MonoBehaviour-контексте (ApplicationEntry).
- **D-10:** При ошибке сети `UgsService` выбрасывает исключение, которое перехватывает вызывающий код — отображает текстовое сообщение об ошибке.

### Claude's Discretion

- UGS архитектура (D-08–D-10): паттерн AudioManager — обычный C# класс без MonoBehaviour.
- Coroutine vs async/await bridge: ApplicationEntry (MonoBehaviour) оборачивает Task в Coroutine при необходимости.
- Error message UI: простой TextMeshProUGUI в LeaderboardView и GameOverView, скрыт по умолчанию.

### Deferred Ideas (OUT OF SCOPE)

Ничего за рамками фазы в ходе обсуждения. Уничтожение пуль при коллайде — отложена отдельно.
</user_constraints>

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| LEAD-01 | При первом запуске выполняется анонимная Guest-аутентификация через UGS Authentication SDK | `UnityServices.InitializeAsync()` + `SignInAnonymouslyAsync()` — токен автоматически кэшируется в PlayerPrefs SDK |
| LEAD-02 | Submit Score: игрок вводит имя (max 16 символов), счёт отправляется в UGS Leaderboard | `AddPlayerScoreAsync(leaderboardId, score)` + `UpdatePlayerNameAsync(name)` перед отправкой |
| LEAD-03 | Leaderboard экран отображает Top-10 (PlayerEntry: имя + очки) | `GetScoresAsync(leaderboardId)` → `LeaderboardScoresPage.Results` (List<LeaderboardEntry>) |
| LEAD-04 | Позиция текущего игрока отображается отдельно, даже если он не в Top-10 | `GetPlayerScoreAsync(leaderboardId)` → `LeaderboardEntry` с полем `Rank` |
| LEAD-05 | При ошибке сети показывается сообщение; игра продолжается без краша | try/catch `LeaderboardsException` + `RequestFailedException` + `AuthenticationException` |
| LEAD-06 | UGS Project ID и имя лидерборда задаются в ScriptableObject, не хардкод | `GameData.UgsProjectId` (новое поле) + `GameData.LeaderboardId` (уже есть) |
</phase_requirements>

---

## Summary

Фаза 7 подключает два уже установленных UGS SDK (Authentication 3.6.0 и Leaderboards 2.3.3) к существующим стабам фазы 6. Большинство кода-заглушек уже написан — требуется расширить, а не создать с нуля.

**Ключевые технические факты:**
1. `SignInAnonymouslyAsync()` автоматически использует кэшированный токен (PlayerPrefs UGS SDK) при повторных запусках — дополнительная логика "проверить токен" НЕ нужна, метод делает это сам.
2. Имя игрока для лидерборда устанавливается через `AuthenticationService.Instance.UpdatePlayerNameAsync(name)` ПЕРЕД вызовом `AddPlayerScoreAsync`. UGS автоматически добавляет суффикс `#NNNN` — в UI следует отображать `PlayerName` из `LeaderboardEntry` как есть.
3. `GameData.UgsProjectId` хранится в ScriptableObject, но `UnityServices.InitializeAsync()` использует Project ID из **Project Settings → Services** (вшивается в билд). Поле в ScriptableObject нужно для идентификации в Editor, но НЕ передаётся программно в `InitializeAsync` — это ограничение UGS.
4. `LeaderboardScoresPage.Results` — `List<LeaderboardEntry>` где каждый `LeaderboardEntry` имеет поля: `Rank`, `PlayerId`, `PlayerName`, `Score`.

**Главная рекомендация:** Реализовать `UgsService` как plain C# класс по паттерну `AudioManager`. `ApplicationEntry` запускает `InitializeAsync()` через Coroutine (`StartCoroutine(RunAsync(UgsService.InitializeAsync()))`). Все UGS вызовы — async Task, никаких MonoBehaviour зависимостей.

---

## Standard Stack

### Core

| Библиотека | Версия | Назначение | Статус |
|-----------|--------|-----------|--------|
| `com.unity.services.authentication` | 3.6.0 | Guest sign-in, PlayerName, токен | Уже в manifest.json |
| `com.unity.services.leaderboards` | 2.3.3 | Submit/Get scores | Уже в manifest.json |
| `com.unity.services.core` | 1.16.0 | `UnityServices.InitializeAsync()` | Уже в manifest.json |

**Установка не требуется** — все пакеты уже подключены.

### Ключевые пространства имён

```csharp
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
```

---

## Architecture Patterns

### Структура UgsService

```csharp
// Паттерн AudioManager — plain C# class, не MonoBehaviour
public class UgsService
{
    private readonly string _leaderboardId;

    public UgsService(string leaderboardId)
    {
        _leaderboardId = leaderboardId;
    }

    public async Task InitializeAsync()
    {
        await UnityServices.InitializeAsync();
        // SignInAnonymouslyAsync автоматически использует кэшированный токен
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async Task SubmitScoreAsync(string playerName, int score)
    {
        // Установить имя ПЕРЕД отправкой — оно будет видно в лидерборде
        await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
        await LeaderboardsService.Instance.AddPlayerScoreAsync(_leaderboardId, (double)score);
    }

    public async Task<LeaderboardScoresPage> GetTopScoresAsync()
    {
        return await LeaderboardsService.Instance.GetScoresAsync(_leaderboardId);
    }

    public async Task<LeaderboardEntry> GetPlayerScoreAsync()
    {
        return await LeaderboardsService.Instance.GetPlayerScoreAsync(_leaderboardId);
    }
}
```

### Coroutine-обёртка в ApplicationEntry

```csharp
// Вспомогательный метод для запуска Task из Coroutine
private IEnumerator RunAsync(Task task)
{
    while (!task.IsCompleted)
    {
        yield return null;
    }
    if (task.IsFaulted)
    {
        throw task.Exception;
    }
}
```

Вызов в `Start()`:
```csharp
StartCoroutine(RunAsync(ugsService.InitializeAsync()));
```

### Навигация GameOver → Submit → Leaderboard

```
OnGameOver()
  └─ GameOverScreen.Show(score, highScore, playerName, onSubmitScore, onLeaderboard)
       ├─ InputField = PlayerPrefs["PlayerName"] (предзаполнен)
       ├─ onSubmitScore → Application.OnSubmitScore(name, score)
       │    └─ UgsService.SubmitScoreAsync(name, score) (async через coroutine)
       │         ├─ успех → disable _submitScoreButton, navigate to LeaderboardScreen
       │         └─ ошибка → показать errorText в GameOverView
       └─ onLeaderboard → navigate to LeaderboardScreen
            └─ UgsService.GetTopScoresAsync() + GetPlayerScoreAsync() → Bind(vm)
```

### LeaderboardViewModel (расширенный)

```csharp
public class LeaderboardViewModel : AbstractViewModel
{
    public Action OnBackClicked;
    // Top-10 реальные данные
    public LeaderboardEntry[] Top10Entries;   // массив до 10 элементов
    public LeaderboardEntry PlayerEntry;       // null если не подал счёт
    public string ErrorMessage;               // null если нет ошибки
}
```

### LeaderboardView.Bind(vm) — 10 статических строк

```csharp
// 10 TextMeshProUGUI поля: _entryTexts[0].._entryTexts[9]
// + _playerEntryText (отдельная строка под разделителем)
// + _errorText (скрыт по умолчанию)
public void Bind(LeaderboardViewModel vm)
{
    for (int i = 0; i < 10; i++)
    {
        if (i < vm.Top10Entries.Length)
        {
            var e = vm.Top10Entries[i];
            _entryTexts[i].text = $"{e.Rank}. {e.PlayerName}  {e.Score:N0}";
            _entryTexts[i].gameObject.SetActive(true);
        }
        else
        {
            _entryTexts[i].gameObject.SetActive(false);
        }
    }
    // Позиция текущего игрока
    if (vm.PlayerEntry != null)
    {
        _playerEntryText.text = $"#{vm.PlayerEntry.Rank} {vm.PlayerEntry.PlayerName}  {vm.PlayerEntry.Score:N0}";
    }
    // Ошибка
    _errorText.gameObject.SetActive(!string.IsNullOrEmpty(vm.ErrorMessage));
    if (_errorText.gameObject.activeSelf) { _errorText.text = vm.ErrorMessage; }
}
```

### Phase7Setup Editor скрипт

По паттерну Phase6Setup — один `[MenuItem("Asteroids/Setup Phase 7 Assets")]` вызывает:
1. `UpdateGameDataAddUgsProjectId()` — добавить поле в GameData.asset
2. `UpdateGameOverScreenAddInputField()` — добавить TMP InputField + errorText в GameOver Canvas
3. `UpdateLeaderboardScreenAddEntryTexts()` — добавить 10 TextMeshProUGUI + playerEntry + разделитель + errorText
4. `EnableLeaderboardButton()` — включить `_leaderboardButton.interactable = true` в TitleScreenView
5. `SaveAndRefresh()`

---

## Don't Hand-Roll

| Проблема | Не строить | Использовать | Почему |
|---------|-----------|-------------|--------|
| Токен/сессия анонимного игрока | Свой PlayerPrefs-кэш токена | `SignInAnonymouslyAsync()` (SDK делает это автоматически) | SDK кэширует токен в PlayerPrefs сам, повторный вызов использует кэш |
| Очередь retry сетевых запросов | Свой retry-механизм | catch + показ errorText | Лидерборды некритичны — показать ошибку достаточно |
| Сериализация/десериализация ответа API | Ручной JSON-парсинг | `LeaderboardScoresPage.Results` (SDK возвращает typed объекты) | SDK возвращает готовые C# объекты |
| Хранение playerName в PlayerPrefs | Дублировать с именем лидерборда | `PlayerPrefs.SetString("PlayerName", name)` (D-02) | Единый источник правды |
| Pagination | Сложный пагинатор | По умолчанию Limit=10 в GetScoresAsync | 10 записей — один запрос |

---

## Common Pitfalls

### Pitfall 1: UGS Project ID — ScriptableObject vs реальная инициализация

**Что пойдёт не так:** Разработчик кладёт `GameData.UgsProjectId` в ScriptableObject и пытается передать его в `UnityServices.InitializeAsync(options)`, ожидая что SDK будет использовать его вместо Project Settings.

**Почему:** UGS SDK читает Project ID из **Project Settings → Services → General Settings** (вшито в билд). `InitializationOptions` НЕ принимает `SetProjectId()`. Поле `UgsProjectId` в ScriptableObject служит только документацией/справкой для разработчика.

**Как избежать:** Убедиться что Unity Dashboard Project ID совпадает с тем, что установлен через Edit → Project Settings → Services перед билдом. `GameData.UgsProjectId` — информационное поле, не передаётся в код.

**Предупреждение:** Если `UnityServices.InitializeAsync()` бросает `ServicesInitializationException` — значит Project ID не настроен в Project Settings.

### Pitfall 2: PlayerName с суффиксом #NNNN

**Что пойдёт не так:** Игрок вводит "ACE", а в лидерборде отображается "ACE#1234" — UGS автоматически добавляет уникальный хэш-суффикс.

**Почему:** `UpdatePlayerNameAsync` автоматически дополняет имя. `LeaderboardEntry.PlayerName` содержит полное имя с суффиксом.

**Как избежать:** Отображать `LeaderboardEntry.PlayerName` как есть — это нормальное поведение UGS. Не пытаться обрезать суффикс.

**Ограничение:** Максимум имени без суффикса — по умолчанию 50 символов UGS, но D-01 ограничивает до 16 через `InputField.characterLimit = 16` — что корректно.

### Pitfall 3: Повторный вызов SignInAnonymouslyAsync без проверки IsSignedIn

**Что пойдёт не так:** Вызов `SignInAnonymouslyAsync()` когда игрок уже залогинен бросает исключение.

**Почему:** SDK не идемпотентен для повторного входа без явного SignOut.

**Как избежать:** Всегда проверять `AuthenticationService.Instance.IsSignedIn` перед вызовом.

```csharp
if (!AuthenticationService.Instance.IsSignedIn)
{
    await AuthenticationService.Instance.SignInAnonymouslyAsync();
}
```

### Pitfall 4: async Task в non-MonoBehaviour без обработки исключений

**Что пойдёт не так:** Необработанное исключение в `Task` "проглатывается" — краша нет, но и ошибка не видна.

**Почему:** Unity не перехватывает исключения из завершённых Task автоматически.

**Как избежать:** В `RunAsync` Coroutine проверять `task.IsFaulted` и логировать/пробрасывать. Все публичные методы UgsService оборачивать в try/catch на стороне вызывающего (Application.cs).

### Pitfall 5: WebGL и async/await thread context

**Что пойдёт не так:** Некоторые паттерны async/await не работают в WebGL из-за однопоточности.

**Почему:** WebGL не поддерживает настоящие потоки. UGS SDK использует `UnityWebRequest` внутри — это совместимо с WebGL, но только если код выполняется в главном потоке.

**Как избежать:** Не использовать `Task.Run()` или `ConfigureAwait(false)` с последующим доступом к Unity API. Использовать Coroutine-обёртку из ApplicationEntry или `async void` только в MonoBehaviour (но предпочтительнее Coroutine).

### Pitfall 6: GetPlayerScoreAsync бросает исключение если игрок не подавал счёт

**Что пойдёт не так:** Вызов `GetPlayerScoreAsync()` для игрока без записи в лидерборде → `LeaderboardsException` с кодом NotFound.

**Почему:** Если игрок ещё не отправил счёт, его нет в лидерборде.

**Как избежать:** Оборачивать в try/catch отдельно. При ошибке NotFound — PlayerEntry = null, `_playerEntryText.text = "—"`.

### Pitfall 7: Unity Editor требует реального UGS Project ID

**Что пойдёт не так:** В Play Mode в Editor UGS-запросы падают с ошибкой аутентификации.

**Почему:** UGS нужен реальный Project ID, связанный с Unity Dashboard.

**Как избежать:** Перед работой с Phase 7 убедиться что: 1) Unity Dashboard создан проект, 2) Project ID подключён через Edit → Project Settings → Services, 3) Leaderboard с именем из `GameData.LeaderboardId` создан в Dashboard.

---

## Code Examples

Верифицированные паттерны из официальной документации и SDK:

### Полная инициализация UGS

```csharp
// Источник: docs.unity.com/ugs + medium.com UGS tutorial
public async Task InitializeAsync()
{
    try
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }
    catch (AuthenticationException ex)
    {
        // Ошибка аутентификации — пробросить для отображения в UI
        throw;
    }
    catch (RequestFailedException ex)
    {
        // Ошибка сети — пробросить для отображения в UI
        throw;
    }
}
```

### Submit Score с именем игрока

```csharp
// Источник: docs.unity3d.com/Packages/com.unity.services.leaderboards@2.0/api/
public async Task SubmitScoreAsync(string playerName, int score)
{
    await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
    await LeaderboardsService.Instance.AddPlayerScoreAsync(_leaderboardId, (double)score);
}
```

### Получить Top-10 и позицию игрока

```csharp
// Источник: docs.unity3d.com/Packages/com.unity.services.leaderboards@2.0/api/
public async Task<LeaderboardScoresPage> GetTopScoresAsync()
{
    // По умолчанию Limit=10, Offset=0 — возвращает Top-10
    return await LeaderboardsService.Instance.GetScoresAsync(_leaderboardId);
}

public async Task<LeaderboardEntry> GetPlayerScoreAsync()
{
    try
    {
        return await LeaderboardsService.Instance.GetPlayerScoreAsync(_leaderboardId);
    }
    catch (LeaderboardsException)
    {
        // Игрок не подавал счёт — вернуть null
        return null;
    }
}
```

### Coroutine-обёртка для Task (ApplicationEntry)

```csharp
// Источник: паттерн проекта, ApplicationEntry.cs
private static IEnumerator RunAsync(Task task)
{
    while (!task.IsCompleted)
    {
        yield return null;
    }
    if (task.IsFaulted && task.Exception != null)
    {
        throw task.Exception.InnerException ?? task.Exception;
    }
}
```

### Обработка ошибок в Application.cs

```csharp
private IEnumerator InitUgsCoroutine()
{
    var task = _ugsService.InitializeAsync();
    yield return RunAsync(task);
    if (task.IsFaulted)
    {
        Debug.LogWarning("[UGS] Инициализация не удалась: " + task.Exception?.Message);
        // UGS недоступен — кнопки лидерборда остаются выключены
    }
}
```

---

## State of the Art

| Старый подход | Текущий подход | Изменён | Влияние |
|--------------|----------------|---------|---------|
| Ручной HTTP к UGS REST API | `LeaderboardsService.Instance.AddPlayerScoreAsync()` | SDK 2.0+ | Не нужен HttpClient |
| Отдельная проверка кэшированного токена | `SignInAnonymouslyAsync()` сам проверяет и переиспользует | SDK 3.x | Упрощает инициализацию |
| `GetLeaderboardScoresAsync` (старое имя) | `GetScoresAsync` | SDK 2.0 | Обновить имена методов |

**Устаревшее:**
- Префикс `GetLeaderboardScoresAsync` — устарел, сейчас `GetScoresAsync`
- `AddScoreAsync` — устарел, сейчас `AddPlayerScoreAsync`

---

## Open Questions

1. **UGS Project ID программная передача**
   - Что знаем: `UnityServices.InitializeAsync()` не принимает ProjectId через options
   - Что неясно: Как D-07 (хранить UgsProjectId в ScriptableObject) сочетается с тем, что SDK читает его из Project Settings
   - Рекомендация: `GameData.UgsProjectId` — только документационное поле для справки разработчика. Реальный ID настраивается через Project Settings единожды. Плановщик должен включить задачу "убедиться что Project Settings настроен".

2. **LeaderboardEntry.Score тип: double vs int**
   - Что знаем: `AddPlayerScoreAsync` принимает `double`, `LeaderboardEntry.Score` — `double`
   - Что неясно: Нужно ли округлять при отображении
   - Рекомендация: Приводить к `(int)entry.Score` при отображении — игровые очки всегда целые.

3. **Имя игрока в лидерборде без UpdatePlayerNameAsync**
   - Что знаем: Без `UpdatePlayerNameAsync` `LeaderboardEntry.PlayerName` может быть "Unknown" или PlayerId
   - Рекомендация: Всегда вызывать `UpdatePlayerNameAsync(playerName)` перед `AddPlayerScoreAsync`.

---

## Environment Availability

| Зависимость | Требуется для | Доступна | Версия | Fallback |
|------------|--------------|---------|--------|---------|
| Unity 2022.3 LTS | Компиляция C#, Editor | Предполагается | 2022.3.x | — |
| UGS Authentication SDK | LEAD-01 | ✓ | 3.6.0 (в manifest) | — |
| UGS Leaderboards SDK | LEAD-02..04 | ✓ | 2.3.3 (в manifest) | — |
| UGS Services Core SDK | InitializeAsync | ✓ | 1.16.0 (в manifest) | — |
| UGS Dashboard + Project | Реальная аутентификация | Требует ручной настройки | — | Play Mode без реального Project ID провалит sign-in |
| Интернет-соединение (runtime) | Лидерборды | ✓ (dev) | — | LEAD-05: показ сообщения об ошибке |

**Блокирующие зависимости без fallback:**
- **UGS Dashboard**: Перед Phase 7 нужно: создать Unity Cloud Project в Dashboard, подключить через Edit → Project Settings → Services, создать Leaderboard с ID из `GameData.LeaderboardId`. Без этого sign-in упадёт с `ServicesInitializationException`.

---

## Validation Architecture

> nyquist_validation = true в config.json — секция обязательна.

### Test Framework

| Свойство | Значение |
|----------|---------|
| Framework | Unity Test Framework 1.1.33 (EditMode/PlayMode) |
| Конфиг-файл | Нет отдельного — через Unity Test Runner |
| Быстрый запуск | Unity Test Runner → EditMode |
| Полный набор | Unity Test Runner → All Tests |

### Требования → Тесты

| ID | Поведение | Тип теста | Как проверить | Автоматизируемо |
|----|----------|----------|--------------|----------------|
| LEAD-01 | Guest sign-in при запуске, повторный — использует кэш | Manual PlayMode | Запустить игру, проверить Console (нет ошибки auth) | Manual |
| LEAD-02 | Submit Score отправляет счёт в UGS | Manual PlayMode | Открыть Dashboard → Leaderboard → проверить запись | Manual |
| LEAD-03 | Top-10 отображается на экране лидерборда | Manual PlayMode | Нажать Leaderboard, проверить 10 строк | Manual |
| LEAD-04 | Позиция игрока видна под разделителем | Manual PlayMode | Отправить счёт, открыть лидерборд, найти свою строку | Manual |
| LEAD-05 | Ошибка сети → errorText, нет краша | Manual PlayMode | Отключить сеть, открыть лидерборд → увидеть errorText | Manual |
| LEAD-06 | Project ID и LeaderboardId в ScriptableObject | EditMode inspect | Проверить GameData.asset в Inspector | Manual |

**Примечание:** UGS SDK требует реального сетевого соединения и аккаунта — автоматизированное тестирование без mock невозможно в рамках данной фазы. Все тесты — manual PlayMode.

### Wave 0 Gaps

Нет — автоматизированные тесты не применимы (UGS требует реальный сервис). Верификация через Manual PlayMode согласно Success Criteria.

---

## Project Constraints (from CLAUDE.md)

Локальный CLAUDE.md отсутствует в репозитории. Применяются глобальные правила из `~/.claude/CLAUDE.md`:

- **Язык**: Все комментарии и документация — на русском языке
- **Фигурные скобки**: Всегда использовать `{}` даже для однострочных `if`/`else`/`for` (K&R стиль — открывающая на той же строке, как в остальном коде проекта)
- **Стиль**: Следовать K&R-стилю как в существующем коде (ApplicationEntry.cs, Application.cs)

---

## Sources

### Primary (HIGH confidence)
- `com.unity.services.leaderboards@2.3/changelog` — подтверждены названия методов и возвращаемые типы
- `docs.unity3d.com/Packages/com.unity.services.leaderboards@2.0/api/ILeaderboardsService` — подтверждены сигнатуры `GetScoresAsync`, `GetPlayerScoreAsync`, `AddPlayerScoreAsync`
- `Packages/manifest.json` проекта — подтверждены версии SDK

### Secondary (MEDIUM confidence)
- [Manage player names — Unity Docs](https://docs.unity.com/ugs/en-us/manual/authentication/manual/player-name-management) — `UpdatePlayerNameAsync` + суффикс #NNNN
- [Authentication sessions — Unity Docs](https://docs.unity.com/ugs/en-us/manual/authentication/manual/session-management) — автокэш токена
- [Medium: Implementing a Leaderboard in Unity](https://medium.com/@mkrtchyan.vanik/master-the-game-implementing-a-leaderboard-in-unity-b94900c2622a) — верифицированный полный пример с API

### Tertiary (LOW confidence)
- Обсуждения Unity Forums об ошибке "Player Name Showing as Unknown" — подтверждают необходимость `UpdatePlayerNameAsync` перед submit

---

## Metadata

**Confidence breakdown:**
- Standard Stack: HIGH — SDK версии подтверждены из manifest.json проекта
- Architecture: HIGH — паттерны UgsService подтверждены официальной документацией SDK
- Pitfalls: HIGH — подтверждены официальными discussions и документацией
- Validation: HIGH — manual-only тесты обоснованы природой UGS (требует реальный сервис)

**Research date:** 2026-03-29
**Valid until:** 2026-06-01 (SDK стабильные, UGS LTS-ветки)
