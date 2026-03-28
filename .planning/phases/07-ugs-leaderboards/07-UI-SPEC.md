---
phase: 7
slug: ugs-leaderboards
status: draft
shadcn_initialized: false
preset: none
created: 2026-03-29
---

# Phase 7 — UI Design Contract: UGS Leaderboards

> Визуальный и интеракционный контракт для Phase 7. Генерируется gsd-ui-researcher, проверяется gsd-ui-checker.

---

## Design System

| Свойство | Значение |
|----------|---------|
| Tool | none (Unity Canvas + TextMeshPro — не веб-фреймворк) |
| Preset | не применимо |
| Component library | Unity UI (uGUI) + TextMeshPro |
| Icon library | Спрайты из GameAtlas (SpriteAtlas, Sub-sprites) |
| Font | Встроенный TMP-шрифт проекта (Liberation Sans SDF или аналог) |
| Rendering | Unity WebGL, Canvas Overlay режим, Single Scene (Main.unity) |

**shadcn Gate:** Не применимо. Проект — Unity WebGL, не React/Next.js/Vite.

---

## Spacing Scale

Все значения в Unity Units (пиксели Canvas, RectTransform.sizeDelta / offsetMin / offsetMax).
Масштаб кратен 4:

| Token | Value | Usage |
|-------|-------|-------|
| xs | 4px | Зазоры между иконками жизней, internal padding текста |
| sm | 8px | Отступы между строками лидерборда |
| md | 16px | Отступы от краёв панели до содержимого |
| lg | 24px | Зазор между заголовком и списком записей |
| xl | 32px | Отступ между блоком Top-10 и строкой текущего игрока |
| 2xl | 48px | Зазор между заголовком экрана и первым элементом |
| 3xl | 64px | Не используется в этой фазе |

**Исключения:**
- Иконки жизней в HUD: 20×20px (из HudVisual.cs — D-08 фазы 3/4)
- InputField высота имени игрока: 40px (тактильная область для WebGL/мыши)
- Кнопки: минимальная высота 40px (область клика мышью в браузере)

---

## Typography

Unity Canvas: TextMeshProUGUI. Все размеры — font size в единицах TMP (соответствуют пикселям при Scale Factor = 1).

| Role | Size | Weight | Line Height | Usage |
|------|------|--------|-------------|-------|
| Display | 36px | Bold (700) | 1.2 | Заголовок экрана: "LEADERBOARD", "GAME OVER" |
| Heading | 24px | Bold (700) | 1.2 | Подзаголовки, счёт игрока ("Score: N", "Best: N") |
| Body | 18px | Regular (400) | 1.5 | Строки Top-10: "1. ACE#1234  42 000" |
| Label | 14px | Regular (400) | 1.5 | Вспомогательный текст: errorText, placeholder |

**Принципы (источник: REQUIREMENTS.md VIS-02, VIS-06, VIS-07, VIS-08):**
- Весь текст белый (#FFFFFF) на чёрном фоне — аутентичный аркадный стиль
- Моноширинный или близкий к нему шрифт предпочтителен для выравнивания колонок счёта
- Числа счёта форматируются с разделителем тысяч: `{score:N0}` (C# format, источник: RESEARCH.md)

---

## Color

Аутентичная цветовая схема Asteroids: чёрный фон, белые элементы.

| Role | Value | Usage |
|------|-------|-------|
| Dominant (60%) | #000000 | Фон всех экранов, Canvas Background |
| Secondary (30%) | #1A1A1A | Панель / Container лидерборда (если нужна визуальная граница) |
| Accent (10%) | #FFFFFF | Весь текст, кнопки, разделители |
| Disabled | #555555 | Кнопки в неактивном состоянии (submitScoreButton после отправки, leaderboardButton до Phase 7) |
| Error | #FF4444 | Только errorText (LEAD-05) — при ошибке сети |

**Accent зарезервирован исключительно для:**
- Текст записей лидерборда
- Активные кнопки (Play Again, Back, Submit Score до отправки)
- Заголовки экранов
- Разделитель между Top-10 и строкой текущего игрока

**Disabled (#555555) зарезервирован для:**
- `_submitScoreButton` после успешной отправки счёта (D-03)
- `_leaderboardButton` в GameOverScreen до отправки / при ошибке UGS

**Error (#FF4444) зарезервирован исключительно для:**
- `_errorText` в GameOverView (ошибка submit)
- `_errorText` в LeaderboardView (ошибка загрузки)

---

## Component Inventory

Перечень UI-элементов, которые исполнитель должен создать или расширить в Phase 7.

### GameOverScreen — расширить (стаб Phase 6)

| Элемент | Тип | Новый / Существующий | Описание |
|---------|-----|---------------------|---------|
| `_scoreText` | TextMeshProUGUI | Существующий | "Score: N" — Heading 24px |
| `_highScoreText` | TextMeshProUGUI | Существующий | "Best: N" — Heading 24px |
| `_playerNameInput` | TMP_InputField | **Новый** | Ввод имени, max 16 символов, предзаполнен из PlayerPrefs["PlayerName"] (D-01, D-02) |
| `_submitScoreButton` | Button | Существующий | Активировать (сейчас interactable=false). После успешного submit — interactable=false (D-03) |
| `_leaderboardButton` | Button | Существующий | Активировать (сейчас interactable=false) |
| `_playAgainButton` | Button | Существующий | Уже активна |
| `_errorText` | TextMeshProUGUI | **Новый** | Скрыт по умолчанию; #FF4444; Label 14px; отображается при ошибке UGS |

**Layout GameOverScreen (сверху вниз, вертикальный стек, отступ md=16px между элементами):**
```
[GAME OVER]            — Display 36px, белый, по центру
[Score: N]             — Heading 24px, по центру
[Best: N]              — Heading 24px, по центру
[InputField: имя]      — 40px высота, ширина 240px, по центру, Label 18px
[Submit Score]         — кнопка, 160px×40px
[Leaderboard]          — кнопка, 160px×40px
[Play Again]           — кнопка, 160px×40px
[errorText]            — Label 14px, #FF4444, скрыт по умолчанию, по центру
```

### LeaderboardScreen — расширить (стаб Phase 6)

| Элемент | Тип | Новый / Существующий | Описание |
|---------|-----|---------------------|---------|
| `_titleText` | TextMeshProUGUI | Существующий | "LEADERBOARD" — Display 36px |
| `_placeholderText` | TextMeshProUGUI | Существующий | Удалить или скрыть после заполнения |
| `_entryTexts[0..9]` | TextMeshProUGUI[10] | **Новый** | 10 статичных строк Top-10; Body 18px (D-04) |
| `_separatorLine` | Image (горизонтальная) | **Новый** | Белая линия 1px высотой, ширина панели, отделяет Top-10 от строки игрока |
| `_playerEntryText` | TextMeshProUGUI | **Новый** | Строка текущего игрока под разделителем; Body 18px (D-05) |
| `_errorText` | TextMeshProUGUI | **Новый** | Скрыт по умолчанию; #FF4444; Label 14px |
| `_backButton` | Button | Существующий | "BACK" — активна всегда |

**Layout LeaderboardScreen (сверху вниз):**
```
[LEADERBOARD]          — Display 36px, по центру, отступ сверху 2xl=48px
                         отступ снизу lg=24px
[1. ACE#1234  42 000]  — Body 18px, левое выравнивание, отступ sm=8px между строками
[2. ...]
...
[10. ...]
[───────────────────]  — разделитель, отступ xl=32px сверху
[#42 YOU#5678  1 200]  — Body 18px, позиция текущего игрока, отступ lg=24px
[errorText]            — Label 14px, #FF4444, скрыт по умолчанию
[BACK]                 — кнопка, 160px×40px, отступ 2xl=48px снизу
```

### TitleScreen — изменить (активировать кнопку)

| Элемент | Тип | Изменение |
|---------|-----|-----------|
| `_leaderboardButton` | Button | interactable=true (Phase7Setup включает, D-11 Phase 6 снимается) |

---

## Interaction Contract

### InputField — ввод имени игрока

| Свойство | Значение | Источник |
|---------|---------|---------|
| Placeholder text | "Введите имя" | Default |
| Max character count | 16 | LEAD-02, D-01 |
| Content type | Alphanumeric | Default (ограничение UGS ≤50 символов без суффикса) |
| Предзаполнение | PlayerPrefs.GetString("PlayerName", "") | D-02 |
| OnEndEdit / OnSubmit | Сохранить в PlayerPrefs["PlayerName"] | D-02 |
| После успешного Submit | ReadOnly = true или interactable = false | D-03 |

### Submit Score — кнопка

| Состояние | Визуал | Условие |
|-----------|--------|---------|
| Активна (ожидание) | Белый текст, белая граница | По умолчанию при открытии GameOverScreen |
| Загрузка (async) | Текст "..." или disabled | Во время UgsService.SubmitScoreAsync |
| Успех | interactable=false, цвет #555555 | После успешной отправки (D-03) |
| Ошибка | interactable=true, errorText видим | При исключении UGS |

### Leaderboard — состояния загрузки

| Состояние | UI | Описание |
|-----------|----|---------|
| Загрузка | `_placeholderText` или "Загрузка..." | Пока UgsService.GetTopScoresAsync выполняется |
| Данные получены | 10 строк + строка игрока | Bind(vm) заполняет все поля |
| Ошибка сети | errorText видим, строки скрыты или пусты | LEAD-05 |
| Нет записи игрока | `_playerEntryText.text = "—"` | GetPlayerScoreAsync → null (Pitfall 6) |

---

## Copywriting Contract

| Element | Copy | Источник |
|---------|------|---------|
| Кнопка Submit Score | "Submit Score" | REQUIREMENTS.md VIS-07 |
| Кнопка Leaderboard (GameOver) | "Leaderboard" | REQUIREMENTS.md VIS-07 |
| Кнопка Leaderboard (Title) | "Leaderboard" | REQUIREMENTS.md VIS-06 |
| Кнопка Back | "Back" | REQUIREMENTS.md VIS-08 |
| Кнопка Play Again | "Play Again" | REQUIREMENTS.md VIS-07, PROG-08 |
| Заголовок экрана лидерборда | "LEADERBOARD" | REQUIREMENTS.md VIS-08 |
| InputField placeholder | "Введите имя" | Default — не определено в upstream |
| Пустое состояние (нет Top-10) | "Нет записей. Сыграйте и отправьте счёт!" | Default (LEAD-03 предполагает данные после хотя бы одной записи) |
| Пустая позиция игрока (не в Top-10) | "—" | Из RESEARCH.md — Pitfall 6 |
| Загрузка лидерборда | "Загрузка..." | Default |
| Ошибка сети (GameOver) | "Ошибка отправки. Проверьте соединение." | D-10, LEAD-05 |
| Ошибка сети (Leaderboard) | "Ошибка загрузки. Проверьте соединение." | D-10, LEAD-05 |
| Успех Submit | (кнопка становится неактивной, без отдельного toast) | D-03 |

**Формат строки лидерборда:**
```
{rank}. {PlayerName}  {score:N0}
```
Пример: `3. ACE#1234  42 000`

**Формат строки текущего игрока:**
```
#{rank} {PlayerName}  {score:N0}
```
Пример: `#42 YOU#5678  1 200`

**Важно:** UGS автоматически добавляет суффикс `#NNNN` к имени — отображать `LeaderboardEntry.PlayerName` как есть (RESEARCH.md Pitfall 2).

**Деструктивных действий в этой фазе нет.** Повторная отправка предотвращается через interactable=false (D-03), не через диалог подтверждения.

---

## Registry Safety

| Registry | Blocks Used | Safety Gate |
|----------|-------------|-------------|
| shadcn official | не применимо (не веб-проект) | не требуется |
| Unity Package Registry | com.unity.services.authentication@3.6.0, com.unity.services.leaderboards@2.3.3, com.unity.services.core@1.16.0 | Уже в manifest.json — ревью не требуется |
| Сторонние реестры | нет | не применимо |

---

## Checker Sign-Off

- [ ] Dimension 1 Copywriting: PASS
- [ ] Dimension 2 Visuals: PASS
- [ ] Dimension 3 Color: PASS
- [ ] Dimension 4 Typography: PASS
- [ ] Dimension 5 Spacing: PASS
- [ ] Dimension 6 Registry Safety: PASS

**Approval:** pending

---

## Pre-Population Sources

| Источник | Решений использовано |
|---------|---------------------|
| CONTEXT.md (Decisions D-01..D-10) | 10 |
| RESEARCH.md (Architecture Patterns, Pitfalls) | 6 |
| REQUIREMENTS.md (LEAD-01..06, VIS-06..08) | 9 |
| Существующий код (LeaderboardView.cs, GameOverView.cs, HudVisual.cs) | 5 |
| User input (этот сеанс) | 0 |

---

*Phase: 07-ugs-leaderboards*
*UI-SPEC created: 2026-03-29*
