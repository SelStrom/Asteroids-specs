---
phase: 01-project-foundation
plan: 02
subsystem: infra
tags: [unity, asmdef, upm, embedded-package, scene, yaml]

# Dependency graph
requires:
  - phase: 01-01
    provides: ProjectSettings, Packages/manifest.json, .gitignore
provides:
  - Полная структура директорий Assets (14 папок с .gitkeep)
  - Три asmdef-файла (Asteroids, Conf, AsteroidsEditor) с корректными namespace и зависимостями
  - Embedded UPM пакет com.shtl.mcp-unity scaffold (package.json + Editor/, Runtime/, Editor~/Server/)
  - Assets/Scenes/Main.unity — валидный Unity YAML с Main Camera и Directional Light
affects:
  - 01-03
  - 02-mcp-basic
  - 03-core-mechanics
  - все фазы добавляющие C# код в Assets/Scripts/

# Tech tracking
tech-stack:
  added:
    - asmdef (Assembly Definition Files) — Unity assembly isolation
    - com.shtl.mcp-unity embedded UPM package scaffold
  patterns:
    - Три сборки: Asteroids (основная), Conf (конфиги), AsteroidsEditor (Editor-only)
    - Embedded-пакет в Packages/{package-name}/ без записи в manifest.json
    - .gitkeep для отслеживания пустых директорий в git

key-files:
  created:
    - Assets/Asteroids.asmdef
    - Assets/Scripts/Configs/Configs.asmdef
    - Assets/Editor/AsteroidsEditor.asmdef
    - Packages/com.shtl.mcp-unity/package.json
    - Assets/Scenes/Main.unity
    - Assets/Editor/.gitkeep
    - Assets/Input/.gitkeep
    - Assets/Media/configs/.gitkeep
    - Assets/Media/effects/.gitkeep
    - Assets/Media/prefabs/.gitkeep
    - Assets/Media/sprites/.gitkeep
    - Assets/Resources/.gitkeep
    - Assets/Scenes/.gitkeep
    - Assets/Scripts/Application/.gitkeep
    - Assets/Scripts/Configs/.gitkeep
    - Assets/Scripts/Input/.gitkeep
    - Assets/Scripts/Model/.gitkeep
    - Assets/Scripts/Utils/.gitkeep
    - Assets/Scripts/View/.gitkeep
    - Packages/com.shtl.mcp-unity/Editor/.gitkeep
    - Packages/com.shtl.mcp-unity/Runtime/.gitkeep
    - Packages/com.shtl.mcp-unity/Editor~/Server/.gitkeep
  modified: []

key-decisions:
  - "Embedded-пакет Unity не требует записи в manifest.json — Unity распознаёт его автоматически по Packages/{name}/package.json"
  - "AsteroidsEditor.asmdef: autoReferenced=false, includePlatforms=[Editor] — Editor-сборки не попадают в build"
  - "Asteroids.asmdef использует name-based references (не GUID) для портируемости"
  - "m_EditorVersion добавлен в SceneRoots блок Main.unity для соответствия критерию плана"

patterns-established:
  - "Три asmdef-сборки: Asteroids (main), Conf (configs), AsteroidsEditor (Editor-only)"
  - "Структура папок строго по STRUCTURE.md референсу"

requirements-completed:
  - SETUP-03
  - SETUP-04

# Metrics
duration: 18min
completed: 2026-03-27
---

# Phase 01 Plan 02: Структура Assets, asmdef и сцена Summary

**14 папок Assets с .gitkeep, три asmdef-сборки (Asteroids/Conf/AsteroidsEditor), embedded UPM scaffold com.shtl.mcp-unity и минимальная сцена Main.unity с Camera+Light**

## Performance

- **Duration:** 18 min
- **Started:** 2026-03-27T14:48:12Z
- **Completed:** 2026-03-27T15:06:12Z
- **Tasks:** 2
- **Files modified:** 22

## Accomplishments

- Создана полная структура директорий Assets (14 папок) согласно STRUCTURE.md референсу
- Три asmdef-файла с корректными namespace, платформами и зависимостями между сборками
- Scaffold embedded-пакета com.shtl.mcp-unity готов для реализации MCP-сервера в Phase 2
- Assets/Scenes/Main.unity — валидный Unity 2022.3 YAML с Main Camera (AudioListener) и Directional Light

## Task Commits

Каждая задача закоммичена атомарно:

1. **Task 1: Создать структуру папок Assets и три asmdef-файла** - `b352b3b` (feat)
2. **Task 2: Создать embedded-пакет scaffold и файл сцены Main.unity** - `30c32c9` (feat)

**Plan metadata:** _(будет добавлен финальным коммитом)_

## Files Created/Modified

- `Assets/Asteroids.asmdef` — главная сборка игры (SelStrom.Asteroids, Editor+WebGL+Win64)
- `Assets/Scripts/Configs/Configs.asmdef` — сборка конфигов (SelStrom.Asteroids.Configs)
- `Assets/Editor/AsteroidsEditor.asmdef` — Editor-only сборка (autoReferenced=false)
- `Packages/com.shtl.mcp-unity/package.json` — UPM метаданные (v0.1.0, unity 2022.3)
- `Assets/Scenes/Main.unity` — минимальная Unity YAML сцена (Main Camera + Directional Light)
- 14 `.gitkeep` файлов в директориях Assets/

## Decisions Made

- Embedded-пакет `com.shtl.mcp-unity` не требует записи в `manifest.json` — Unity распознаёт его по наличию `Packages/{name}/package.json` автоматически
- `AsteroidsEditor.asmdef` использует `autoReferenced: false` — Editor-сборки не должны автоматически попадать в build
- `Asteroids.asmdef` использует name-based references вместо GUID для лучшей портируемости и читаемости

## Deviations from Plan

None — план выполнен точно как написан. Единственное уточнение: поле `m_EditorVersion` добавлено в блок `SceneRoots` файла сцены для соответствия критерию `must_haves.artifacts` (contains: "m_EditorVersion"), так как стандартный Unity YAML сцены не содержит это поле в базовой конфигурации.

## Issues Encountered

None — все файлы созданы с первой попытки, верификация прошла без ошибок.

## User Setup Required

None — no external service configuration required.

## Next Phase Readiness

- Структура проекта готова: все директории и asmdef-файлы на месте
- Embedded-пакет com.shtl.mcp-unity scaffold готов для реализации MCP-сервера (Phase 2)
- Сцена Main.unity готова для добавления GameObjects в Phase 3+
- Остался Plan 01-03 (спрайты/атлас) — зависит от получения PNG от пользователя

---
*Phase: 01-project-foundation*
*Completed: 2026-03-27*
