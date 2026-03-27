# Asteroids — Unity WebGL

## What This Is

Полнофункциональная реализация классической аркадной игры Asteroids на Unity 2022 LTS с WebGL-сборкой.
Игра воссоздаётся по детализированным спецификациям, проверяя способность восстановить проект из описания.
Включает встроенный MCP-пакет `com.shtl.mcp-unity` для управления проектом через Claude Code.

## Core Value

Играбельный Asteroids в браузере, полностью воспроизводящий геймплей оригинала — движение, стрельба, астероиды, жизни, счёт.

## Requirements

### Validated

(None yet — ship to validate)

### Active

**Геймплей**
- [ ] Корабль игрока: тяга, вращение, инерция (Newtonian physics, wrap-around экрана)
- [ ] Стрельба: ограниченное число пуль на экране, cooldown
- [ ] Астероиды трёх размеров: Large → 2×Medium → 2×Small при уничтожении
- [ ] UFO: малый (точный) и большой (случайный) — периодическое появление
- [ ] Коллизии: корабль/астероид, пуля/астероид, корабль/UFO, пуля/UFO
- [ ] Жизни (начальный запас 3), respawn с кратковременным иммунитетом
- [ ] Счёт: Large 20, Medium 50, Small 100, Large UFO 200, Small UFO 1000
- [ ] Экстра-жизнь каждые 10 000 очков
- [ ] Волны: при очистке поля — новая волна с увеличенным числом астероидов
- [ ] Game Over экран с отображением финального счёта

**Графика**
- [ ] 2D-спрайты из предоставленного текстурного атласа
- [ ] Built-in Render Pipeline, ортографическая камера
- [ ] Wrap-around экрана для всех объектов (корабль, астероиды, пули, UFO)
- [ ] Particle effects: взрывы при уничтожении

**Аудио**
- [ ] Звуки: выстрел, тяга, взрыв корабля, взрыв астероида, UFO-ворнинг
- [ ] Фоновый пульс (нарастает с уменьшением числа астероидов)

**Лидерборды**
- [ ] Отправка счёта в Unity Gaming Services Leaderboards после Game Over
- [ ] Экран лидерборда: Top-10 глобально, позиция текущего игрока
- [ ] Guest-аутентификация через UGS Authentication

**MCP-пакет com.shtl.mcp-unity**
- [ ] TypeScript MCP-сервер в `Editor~/Server/` (stdio, Anthropic MCP SDK)
- [ ] Editor bridge (C# HTTP-сервер на localhost) — управление сценами, AssetDatabase, Play Mode
- [ ] Runtime bridge — чтение состояния игры (счёт, волна, жизни) во время Play Mode
- [ ] MCP tools: `compile`, `play`, `stop`, `get_game_state`, `list_scenes`, `open_scene`, `import_asset`
- [ ] `mcp.json` конфиг для регистрации сервера в Claude Code

### Out of Scope

- Многопользовательский режим — проект одиночный по условию задачи
- Серверная часть (кроме UGS) — нет собственного бэкенда
- Мобильные платформы — целевая платформа WebGL
- Сохранение прогресса локально — только onсессионный счёт

## Context

- **Тест восстановления**: проект задуман как проверка точности воспроизведения по спецификации, поэтому спецификации должны быть исчерпывающими
- **Текстурный атлас**: пользователь предоставит sprite sheet; структура атласа (имена спрайтов, расположение) уточняется при передаче
- **UGS Leaderboards**: требует `Project ID` и настроенного лидерборда в Unity Dashboard; конфигурация вносится вручную
- **MCP-сервер**: запускается как внешний stdio-процесс; Unity Editor Extension открывает HTTP-сервер на localhost для взаимодействия

## Constraints

- **Tech stack**: Unity 2022.3 LTS, C#, Built-in Render Pipeline — выбрано для простоты настройки WebGL
- **Платформа**: WebGL — нет доступа к файловой системе, threading ограничен, нет нативных плагинов
- **Рендер**: Built-in (Legacy) — не URP, учитывать при написании шейдеров и материалов
- **MCP Runtime**: Runtime bridge работает только в Editor Play Mode, не в релизной сборке
- **Зависимости**: `com.unity.services.leaderboards`, `com.unity.services.authentication` через Package Manager

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Built-in Render Pipeline вместо URP | Проще настройка для WebGL, меньше конфигурации | — Pending |
| TypeScript для MCP-сервера | Официальный Anthropic MCP SDK, наиболее зрелый | — Pending |
| UGS Leaderboards вместо кастомного API | Без собственного сервера, готовый SDK | — Pending |
| Guest auth через UGS Authentication | WebGL не требует регистрации от пользователя | — Pending |
| Embedded package com.shtl.mcp-unity | Самодостаточный инструмент разработки, не часть игры | — Pending |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd:transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd:complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-03-27 after initialization*
