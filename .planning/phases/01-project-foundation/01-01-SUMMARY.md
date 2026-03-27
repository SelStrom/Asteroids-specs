---
phase: 01-project-foundation
plan: 01
subsystem: infra
tags: [unity, webgl, upm, physics2d, ugs, projectsettings]

# Dependency graph
requires: []
provides:
  - ProjectSettings/ProjectSettings.asset с WebGL-настройками и UGS cloudProjectId
  - ProjectSettings/TagManager.asset со слоями 7-11 (Player, Asteroid, PlayerBullet, EnemyBullet, Enemy)
  - ProjectSettings/Physics2DSettings.asset с базовой физикой 2D
  - Packages/manifest.json с 13 UPM-пакетами включая UGS Authentication и Leaderboards
  - .gitignore для Unity-проекта
affects:
  - 01-02 (structure, asmdef — читает ProjectSettings)
  - 01-03 (mcp-unity scaffold — зависит от Packages/)
  - все последующие фазы (используют физические слои и пакеты)

# Tech tracking
tech-stack:
  added:
    - com.unity.inputsystem@1.19.0
    - com.unity.textmeshpro@3.0.9
    - com.unity.feature.2d@2.0.1
    - com.unity.services.authentication@3.6.0
    - com.unity.services.leaderboards@2.3.3
    - com.shtl.mvvm (git hash c7bda1c)
  patterns:
    - Unity YAML asset format для ProjectSettings-файлов
    - UPM manifest.json с git URL + hash для воспроизводимости

key-files:
  created:
    - ProjectSettings/ProjectSettings.asset
    - ProjectSettings/ProjectVersion.txt
    - ProjectSettings/TagManager.asset
    - ProjectSettings/Physics2DSettings.asset
    - Packages/manifest.json
    - .gitignore
  modified: []

key-decisions:
  - "webGLCompressionFormat: 0 — отключена компрессия для упрощения деплоя на этапе разработки"
  - "webGLMemoryGrowthMode: 2 — Memory Growth включён для WebGL"
  - "m_LayerCollisionMatrix оставлена как открытая матрица (ffffffff×32) — финальная настройка через Unity Editor"
  - "com.shtl.mvvm подключён через git hash c7bda1c для воспроизводимости сборки"

patterns-established:
  - "Unity ProjectSettings в формате YAML 1.1 с тегом !u! tag:unity3d.com,2011:"

requirements-completed:
  - SETUP-01
  - SETUP-02

# Metrics
duration: 10min
completed: 2026-03-27
---

# Phase 1 Plan 01: Project Foundation — Settings Summary

**Unity 2022.3.60f1 ProjectSettings сконфигурированы для WebGL: Gamma color space, отключённый splash, 1920x1080, UGS cloudProjectId, физические слои 7-11 и manifest.json с 13 UPM-пакетами**

## Performance

- **Duration:** ~10 min
- **Started:** 2026-03-27T14:35:00Z
- **Completed:** 2026-03-27T14:45:34Z
- **Tasks:** 3
- **Files modified:** 6 создано, 0 изменено

## Accomplishments

- ProjectSettings.asset с полной конфигурацией WebGL Player Settings (Gamma, No splash, 1920x1080, UGS)
- TagManager.asset с 5 игровыми слоями (7-11: Player, Asteroid, PlayerBullet, EnemyBullet, Enemy)
- Physics2DSettings.asset с базовой физикой 2D
- Packages/manifest.json с 13 UPM-зависимостями включая UGS Authentication, Leaderboards и com.shtl.mvvm

## Task Commits

Каждая задача закоммичена атомарно:

1. **Task 1: Создать ProjectSettings/ProjectSettings.asset** — `03a5468` (feat)
2. **Task 2: Настроить физические слои и матрицу коллизий** — `8848c65` (feat)
3. **Task 3: Создать Packages/manifest.json и .gitignore** — `c4bfec5` (feat)

## Files Created/Modified

- `ProjectSettings/ProjectSettings.asset` — Unity Player Settings: WebGL-конфиг, Gamma color space, splash=off, cloudProjectId UGS
- `ProjectSettings/ProjectVersion.txt` — Версия Unity 2022.3.60f1 (fd900f65ea)
- `ProjectSettings/TagManager.asset` — Слои 0-31, пользовательские слои 7-11, SortingLayer Default
- `ProjectSettings/Physics2DSettings.asset` — Physics2D: gravity=(0,-9.81), VelocityIterations=8, открытая матрица коллизий
- `Packages/manifest.json` — 13 UPM-пакетов, com.shtl.mvvm через git hash c7bda1c
- `.gitignore` — Стандартный Unity .gitignore + Node.js для MCP-сервера

## Decisions Made

- `webGLCompressionFormat: 0` — компрессия отключена для упрощения деплоя на этапе разработки (избегает проблем с Content-Encoding заголовками)
- `webGLMemoryGrowthMode: 2` — Memory Growth включён для WebGL вместо фиксированного размера памяти
- `m_LayerCollisionMatrix` установлена как полностью открытая (32×ffffffff) — корректная матрица настраивается через Unity Editor после открытия проекта
- `com.shtl.mvvm` подключён через конкретный git hash `c7bda1c` для воспроизводимости — не плавающий тег

## Deviations from Plan

None — план выполнен точно как написан.

## Issues Encountered

None.

## User Setup Required

После открытия проекта в Unity Editor необходимо настроить матрицу коллизий:
1. Edit > Project Settings > Physics 2D > Layer Collision Matrix
2. Установить взаимодействия согласно UNITY_CONFIG.md:
   - Player (7) ↔ Asteroid (8), EnemyBullet (10), Enemy (11)
   - Asteroid (8) ↔ PlayerBullet (9), Enemy (11)
   - PlayerBullet (9) ↔ Enemy (11)

## Next Phase Readiness

- ProjectSettings и Packages готовы — 01-02 может создавать структуру папок и asmdef
- 01-03 может создавать embedded пакет com.shtl.mcp-unity (scaffold)
- Слои определены и готовы для использования в Phase 3+ (игровые объекты)

---
*Phase: 01-project-foundation*
*Completed: 2026-03-27*
