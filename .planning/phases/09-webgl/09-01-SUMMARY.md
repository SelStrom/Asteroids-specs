---
phase: 09-webgl
plan: 01
subsystem: infra
tags: [unity, webgl, build-settings]

# Dependency graph
requires:
  - phase: 08-mcp-runtime
    provides: "Завершённый Unity-проект со всеми игровыми системами"
provides:
  - "EditorBuildSettings.asset с зарегистрированной сценой Main.unity"
  - "Первая WebGL-сборка Unity-проекта (после ручного шага)"
affects: [09-02-deploy]

# Tech tracking
tech-stack:
  added: []
  patterns: ["Прямое редактирование YAML-формата Unity .asset файлов для регистрации сцены в Build Settings"]

key-files:
  created: []
  modified:
    - "ProjectSettings/EditorBuildSettings.asset"

key-decisions:
  - "GUID Main.unity (6d7de3ab382bf4928a9621158bb4fb3d) записан напрямую в EditorBuildSettings.asset без PhaseSetup-скрипта — по условию плана D-02"
  - "Задача 2 (переключение платформы и сборка) — ручная операция в Unity Editor, не автоматизируемая"

patterns-established:
  - "Редактирование ProjectSettings/*.asset: YAML-файл, поле m_Scenes с enabled/path/guid"

requirements-completed:
  - WEBGL-01

# Metrics
duration: 10min
completed: 2026-03-31
---

# Phase 09 Plan 01: WebGL Build Setup Summary

**Сцена Main.unity зарегистрирована в EditorBuildSettings.asset (GUID 6d7de3ab382bf4928a9621158bb4fb3d) — готова к WebGL-сборке в Unity Editor**

## Performance

- **Duration:** ~10 min
- **Started:** 2026-03-31T00:00:00Z
- **Completed:** 2026-03-31 (частично — Задача 2 ожидает ручного действия)
- **Tasks:** 1/2 (Задача 2 — checkpoint:human-action)
- **Files modified:** 1

## Accomplishments
- ProjectSettings/EditorBuildSettings.asset: m_Scenes [] заменён на запись Main.unity с enabled=1
- GUID корректно получен из Assets/Scenes/Main.unity.meta и вставлен в файл
- Проверка: grep подтверждает путь, enabled, удаление пустого списка, правильный GUID

## Task Commits

1. **Задача 1: Добавить сцену Main.unity в EditorBuildSettings** - `26d1563` (feat)

**Задача 2** ожидает ручного действия в Unity Editor (переключение платформы + сборка).

## Files Created/Modified
- `ProjectSettings/EditorBuildSettings.asset` — m_Scenes: [] → запись Main.unity с guid 6d7de3ab382bf4928a9621158bb4fb3d

## Decisions Made
- Прямое редактирование YAML .asset файла вместо PhaseSetup-скрипта — согласно условию плана (D-02, нет PhaseSetup для 09-webgl)
- GUID берётся из .meta файла сцены, не генерируется

## Deviations from Plan
Нет — план выполнен точно как написан.

## Issues Encountered
Нет.

## User Setup Required

**Задача 2 требует ручных действий в Unity Editor:**

1. Открыть `File → Build Settings` (Cmd+Shift+B)
2. В списке платформ выбрать `WebGL` → нажать `Switch Platform`
3. Дождаться перекомпиляции (1-5 минут)
4. Убедиться что в `Scenes In Build` присутствует `Scenes/Main` с галочкой
5. Нажать `Build` → создать папку `Build/WebGL/` в корне проекта
6. Дождаться завершения сборки (5-30 минут)

**После сборки проверить:**
```bash
ls /Users/selstrom/work/projects/asteroids-specs/Build/WebGL/index.html
ls /Users/selstrom/work/projects/asteroids-specs/Build/WebGL/Build/*.wasm
ls /Users/selstrom/work/projects/asteroids-specs/Build/WebGL/Build/*.data
```

## Next Phase Readiness
- EditorBuildSettings готов к WebGL-сборке
- После ручного шага (Задача 2): Build/WebGL/ будет содержать все артефакты
- Phase 09-02 (деплой) разблокируется после успешной сборки

---
*Phase: 09-webgl*
*Completed: 2026-03-31*
