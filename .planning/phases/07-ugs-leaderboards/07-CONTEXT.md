# Phase 7: UGS Leaderboards - Context

**Gathered:** 2026-03-29
**Status:** Ready for planning

<domain>
## Phase Boundary

Подключить реальные UGS Leaderboards: guest-аутентификация при запуске, отправка счёта из GameOverScreen, отображение Top-10 в LeaderboardScreen. Сетевые ошибки не ломают игру.

Success Criteria:
1. При первом запуске автоматически выполняется guest-аутентификация через UGS; повторные запуски используют сохранённый токен
2. После Game Over игрок вводит имя (макс. 16 символов) и нажимает «Submit Score» — счёт появляется в лидерборде
3. Экран лидерборда отображает Top-10 глобально + позицию текущего игрока (даже если он не в Top-10)
4. При ошибке сети отображается сообщение об ошибке; игра продолжает работу без краша
5. UGS Project ID и имя лидерборда вынесены в ScriptableObject — изменяются без правки кода

Requirements: LEAD-01, LEAD-02, LEAD-03, LEAD-04, LEAD-05, LEAD-06

</domain>

<decisions>
## Implementation Decisions

### Ввод имени игрока

- **D-01:** InputField для имени размещается прямо на GameOverScreen — поле видно сразу, рядом с кнопкой Submit Score. Макс. 16 символов (LEAD-02).
- **D-02:** Имя сохраняется в PlayerPrefs (`"PlayerName"`) — при следующем Game Over поле уже заполнено.
- **D-03:** После успешного Submit кнопка Submit Score становится неактивной (предотвращает повторную отправку в той же сессии).

### LeaderboardView Layout

- **D-04:** Top-10 отображается как 10 статичных TextMeshProUGUI строк (без ScrollRect). Каждая строка: `«позиция. имя  счёт»` — например `«3. ACE  42 000»`.
- **D-05:** Позиция текущего игрока отображается отдельной строкой под разделителем — `«№? ИМЯ  счёт»`. Всегда видна даже если игрок не в Top-10.
- **D-06:** LeaderboardView реализует метод `Bind(LeaderboardViewModel vm)` — `LeaderboardViewModel` расширяется реальными данными (`LeaderboardEntry[]` Top10 + `PlayerEntry` current).

### UGS Project ID

- **D-07:** `GameData` расширяется полем `string UgsProjectId` рядом с `LeaderboardId` (LEAD-06). Поле обязательно заполнить перед Phase 7 работой.

### UGS Архитектура (Claude's Discretion)

- **D-08:** Отдельный C# класс `UgsService` (не MonoBehaviour, как паттерн AudioManager). Содержит методы `InitializeAsync()`, `SubmitScoreAsync(name, score)`, `GetLeaderboardAsync()`. Вызывается из Application.cs через `async Task` → `StartCoroutine` в ApplicationEntry.
- **D-09:** `UgsService` не зависит от Unity lifecycle — все async операции через `Task`, оборачиваются в Coroutine только там, где нужен callback в Unity-контекст.
- **D-10:** При ошибке сети `UgsService` возвращает `null` / выбрасывает исключение которое перехватывает вызывающий код — отображает текстовое сообщение об ошибке в UI (LEAD-05).

### Claude's Discretion

- UGS архитектура (D-08–D-10): Паттерн AudioManager — обычный C# класс без MonoBehaviour.
- Coroutine vs async/await bridge: ApplicationEntry (MonoBehaviour) оборачивает Task в Coroutine при необходимости.
- Error message UI: простой TextMeshProUGUI в LeaderboardView и GameOverView, скрыт по умолчанию.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Requirements
- `.planning/REQUIREMENTS.md` §Leaderboards — LEAD-01..LEAD-06 с точными формулировками

### Existing Code (стабы фазы 6)
- `Assets/Scripts/Application/Screens/LeaderboardScreen.cs` — стаб готов, ждёт реальных данных
- `Assets/Scripts/View/LeaderboardView.cs` — LeaderboardViewModel с заглушками EntryNames[]/EntryScores[]
- `Assets/Scripts/Application/Screens/GameOverScreen.cs` — Show(finalScore, highScore), нужно расширить
- `Assets/Scripts/View/GameOverView.cs` — _submitScoreButton и _leaderboardButton interactable=false
- `Assets/Scripts/View/TitleScreenView.cs` — _leaderboardButton interactable=false, ждёт включения
- `Assets/Scripts/Application/Application.cs` — OnLeaderboardBack() готов, нужно добавить OnSubmitScore() и навигацию к LeaderboardScreen из GameOver

### Config
- `Assets/Scripts/Configs/GameData.cs` — LeaderboardId уже есть, добавить UgsProjectId (D-07)
- `Packages/manifest.json` — com.unity.services.authentication@3.6.0 и com.unity.services.leaderboards@2.3.3 уже подключены

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `LeaderboardScreen` + `LeaderboardView` + `LeaderboardViewModel` — стабы Phase 6, расширить реальными данными
- `GameOverScreen.Show(int, int)` — нужно добавить имя игрока и onSubmitScore callback
- `AbstractScreen` / `AbstractWidgetView<TViewModel>` — паттерн для новых экранов
- `GameData` ScriptableObject — единый конфиг-хаб, добавить UgsProjectId

### Established Patterns
- Экраны: AbstractScreen (логика) + AbstractWidgetView (UI) + ViewModel (данные)
- Сервисы: обычные C# классы без MonoBehaviour (паттерн AudioManager)
- Callbacks из Game через Application.cs (OnGameOver, OnPlayAgain и т.д.)
- `ApplicationEntry.cs` — MonoBehaviour точка входа, запускает Application

### Integration Points
- `Application.OnGameOver()` — добавить отображение InputField имени, передачу onSubmitScore в GameOverScreen
- `Application.Connect()` — добавить TitleScreen кнопку Leaderboard (OnLeaderboardClicked)
- `Application.Start()` — добавить инициализацию UgsService (async через coroutine)
- `TitleScreen.Connect()` — передать OnLeaderboardClicked callback

</code_context>

<specifics>
## Specific Ideas

- GameOver экран: InputField с именем + кнопка Submit сразу видны, без popup
- LeaderboardView: 10 строк + отдельная строка текущего игрока под чертой
- Имя сохраняется в PlayerPrefs — не нужно вводить каждый раз
- После Submit кнопка Submit Score становится серой (disabled)

</specifics>

<deferred>
## Deferred Ideas

Ничего не выходило за рамки фазы в ходе обсуждения.

### Reviewed Todos (не попали в скоуп)
- «Уничтожать пулю при коллайде с астероидом или НЛО» — геймплейная задача, не относится к Phase 7 (лидерборды). Отложена.

</deferred>

---

*Phase: 07-ugs-leaderboards*
*Context gathered: 2026-03-29*
