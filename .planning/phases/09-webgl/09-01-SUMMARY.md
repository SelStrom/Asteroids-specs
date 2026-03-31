---
phase: 09-webgl
plan: 01
subsystem: infra
tags: [unity, webgl, build-settings, webassembly]

# Dependency graph
requires:
  - phase: 08-mcp-runtime
    provides: "Завершённый Unity-проект со всеми игровыми системами"
provides:
  - "EditorBuildSettings.asset с зарегистрированной сценой Main.unity"
  - "Build/WebGL/ — полный набор артефактов WebGL сборки (index.html, WebGL.wasm, WebGL.data, WebGL.loader.js)"
affects: [09-02-deploy]

# Tech tracking
tech-stack:
  added: [Unity WebGL build target]
  patterns: ["Прямое редактирование YAML-формата Unity .asset файлов для регистрации сцены в Build Settings"]

key-files:
  created:
    - "Build/WebGL/index.html"
    - "Build/WebGL/Build/WebGL.wasm"
    - "Build/WebGL/Build/WebGL.data"
    - "Build/WebGL/Build/WebGL.loader.js"
    - "Build/WebGL/Build/WebGL.framework.js"
  modified:
    - "ProjectSettings/EditorBuildSettings.asset"

key-decisions:
  - "GUID Main.unity (6d7de3ab382bf4928a9621158bb4fb3d) записан напрямую в EditorBuildSettings.asset без PhaseSetup-скрипта — по условию плана D-02"
  - "Файлы сборки называются WebGL.* (не Asteroids.*) — Unity использует имя папки вывода при сборке через диалог, не productName"
  - "Build/WebGL/ исключён из git через .gitignore [Bb]uild/ — артефакты не коммитятся в master"
  - "Компрессия WebGL отключена (webGLCompressionFormat=0) — GitHub Pages не поддерживает Brotli без Content-Encoding headers"

patterns-established:
  - "Редактирование ProjectSettings/*.asset: YAML-файл, поле m_Scenes с enabled/path/guid"

requirements-completed:
  - WEBGL-01
  - WEBGL-02
  - WEBGL-03

# Metrics
duration: 147min
completed: 2026-03-31
---

# Phase 09 Plan 01: WebGL Build Setup Summary

**Unity-проект собран под WebGL: сцена Main.unity зарегистрирована в Build Settings, платформа переключена, Build/WebGL/ содержит полный набор артефактов (index.html, WebGL.wasm, WebGL.data) без компрессии — готово к деплою на GitHub Pages**

## Performance

- **Duration:** ~147 min (включая ручной шаг переключения платформы и сборки в Unity Editor)
- **Started:** 2026-03-31T11:43:05Z
- **Completed:** 2026-03-31T12:10:35Z
- **Tasks:** 2/2
- **Files modified:** 1 (EditorBuildSettings.asset) + артефакты Build/WebGL/ (не в git)

## Accomplishments

- ProjectSettings/EditorBuildSettings.asset: m_Scenes [] заменён на запись Main.unity с enabled=1 и GUID 6d7de3ab382bf4928a9621158bb4fb3d
- Платформа Unity переключена на WebGL (пользователь выполнил Switch Platform + Build)
- Build/WebGL/ содержит все артефакты: index.html, WebGL.wasm, WebGL.data, WebGL.loader.js, WebGL.framework.js, TemplateData/
- Файлы без компрессии (.wasm без .br/.gz) — совместимо с GitHub Pages без настройки CORS/Content-Encoding

## Task Commits

1. **Задача 1: Добавить сцену Main.unity в EditorBuildSettings** - `26d1563` (feat)
2. **Задача 2: Переключить платформу на WebGL и выполнить сборку** — выполнено пользователем (checkpoint:human-action), артефакты верифицированы

**Plan metadata:** `b44bef2` (docs: checkpoint WebGL сборки)

## Files Created/Modified

- `ProjectSettings/EditorBuildSettings.asset` — m_Scenes: [] → запись Main.unity с GUID 6d7de3ab382bf4928a9621158bb4fb3d
- `Build/WebGL/index.html` — точка входа WebGL-приложения (не в git)
- `Build/WebGL/Build/WebGL.wasm` — скомпилированный WebAssembly модуль игры (не в git)
- `Build/WebGL/Build/WebGL.data` — игровые ресурсы (не в git)
- `Build/WebGL/Build/WebGL.loader.js` — загрузчик Unity WebGL (не в git)
- `Build/WebGL/Build/WebGL.framework.js` — Unity runtime framework (не в git)
- `Build/WebGL/TemplateData/` — CSS, иконки, логотипы страницы загрузки (не в git)

## Decisions Made

- Прямое редактирование YAML .asset файла вместо PhaseSetup-скрипта — согласно условию плана (D-02, нет PhaseSetup для 09-webgl)
- GUID берётся из .meta файла сцены, не генерируется
- Файлы сборки называются `WebGL.*` вместо `Asteroids.*`: Unity при сборке через диалог именует артефакты по имени папки вывода. Функционально эквивалентно.

## Deviations from Plan

Нет — план выполнен точно как написан. Задача 1 автоматическая, Задача 2 выполнена пользователем как checkpoint:human-action.

## Issues Encountered

Незначительное: имена файлов `WebGL.*` вместо ожидаемых `Asteroids.*`. Причина: Unity именует артефакты по имени папки вывода при использовании диалога выбора папки. Index.html корректно ссылается на WebGL.loader.js — всё работает.

## User Setup Required

Выполнено — пользователь переключил платформу на WebGL и собрал проект. Дальнейших ручных шагов не требуется.

## Next Phase Readiness

- Build/WebGL/ готов к деплою на GitHub Pages (план 09-02)
- Требования WEBGL-01, WEBGL-02, WEBGL-03 выполнены
- Нет блокеров для следующего плана

## Self-Check: PASSED

- SUMMARY.md: FOUND
- Commit 26d1563 (Task 1): FOUND
- Commit b44bef2 (plan metadata): FOUND
- Build/WebGL/index.html: FOUND
- Build/WebGL/Build/WebGL.wasm: FOUND
- Build/WebGL/Build/WebGL.data: FOUND

---
*Phase: 09-webgl*
*Completed: 2026-03-31*
