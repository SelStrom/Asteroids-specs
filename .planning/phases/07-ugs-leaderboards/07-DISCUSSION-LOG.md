# Phase 7: UGS Leaderboards - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-03-29
**Phase:** 07-ugs-leaderboards
**Areas discussed:** Ввод имени игрока, LeaderboardView layout, UGS Project ID

---

## Ввод имени игрока

| Option | Description | Selected |
|--------|-------------|----------|
| InputField прямо на GameOverScreen | Поле ввода имени видно сразу на экране Game Over, рядом с кнопкой Submit Score. Нажал Submit — имя взято из поля. | ✓ |
| Popup при нажатии Submit | Кнопка Submit открывает модальный диалог с InputField. Чище UI, отдельный флоу. | |
| Автоимя из UGS | Использовать автогенерированный player name из UGS Authentication — имя не вводится вручную. | |

**User's choice:** InputField прямо на GameOverScreen

---

## Сохранение имени между сессиями

| Option | Description | Selected |
|--------|-------------|----------|
| Да, сохранять в PlayerPrefs | Игрок вводит имя один раз, при следующем Game Over поле уже заполнено. | ✓ |
| Нет, каждый раз пусто | Поле очищается после каждого Game Over. | |

**User's choice:** Да, сохранять в PlayerPrefs (ключ `"PlayerName"`)

---

## LeaderboardView Layout

| Option | Description | Selected |
|--------|-------------|----------|
| 10 статичных TextMeshProUGUI строк | 10 предзаданных строк в Inspector, заполняемых данными из UGS. Без ScrollRect. | ✓ |
| ScrollView + prefab строки | ScrollRect с Content, динамическое создание строк. Гибче (> 10 записей), но сложнее. | |

**User's choice:** 10 статичных TextMeshProUGUI строк (без ScrollRect)

---

## Содержимое строки Top-10

| Option | Description | Selected |
|--------|-------------|----------|
| Позиция + имя + счёт | Например: «3. ACE  42 000». Классический аркадный стиль. | ✓ |
| Только имя + счёт | Без номера позиции. | |

**User's choice:** Позиция + имя + счёт

---

## UGS Project ID

| Option | Description | Selected |
|--------|-------------|----------|
| Добавить поле в GameData | GameData.UgsProjectId рядом с LeaderboardId. Всё в одном месте. | ✓ |
| ProjectSettings.asset (автоматически) | Unity Dashboard привязывает Project ID через Link Project. Поле в ScriptableObject не нужно. | |

**User's choice:** Добавить поле UgsProjectId в GameData ScriptableObject

---

## Claude's Discretion

- UGS архитектура: отдельный C# класс `UgsService` без MonoBehaviour (паттерн AudioManager)
- Coroutine/async bridge: ApplicationEntry оборачивает Task в Coroutine
- Error UI: скрытый TextMeshProUGUI для ошибок в LeaderboardView и GameOverView

## Deferred Ideas

Нет — обсуждение не выходило за рамки фазы.
