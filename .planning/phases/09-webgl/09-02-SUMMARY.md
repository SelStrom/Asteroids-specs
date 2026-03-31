---
phase: 09-webgl
plan: 02
subsystem: infra
tags: [github-pages, webgl, deploy, gh-pages, unity]

# Dependency graph
requires:
  - phase: 09-01
    provides: WebGL-билд в Build/WebGL/ (index.html, WebGL.data, WebGL.wasm, WebGL.framework.js, WebGL.loader.js)
provides:
  - Orphan-ветка gh-pages с WebGL-сборкой и .nojekyll
  - Публичный URL https://selstrom.github.io/asteroids-specs/ — игра доступна в браузере
  - Настроенный GitHub Pages из ветки gh-pages / (root)
affects: []

# Tech tracking
tech-stack:
  added: [GitHub Pages, gh-pages orphan branch]
  patterns: [orphan-ветка для деплоя статики, .nojekyll для отключения Jekyll]

key-files:
  created:
    - .nojekyll (в ветке gh-pages)
    - index.html (скопирован из Build/WebGL/ в корень gh-pages)
  modified:
    - ProjectSettings/ProjectSettings.asset (webGLCompressionFormat 0→2=Disabled)
    - Assets/Scripts/PlayerActions.cs (EmbeddedJson для WebGL-совместимости)

key-decisions:
  - "gh-pages orphan-ветка создана через git worktree — master не затронут"
  - "webGLCompressionFormat=2 (Disabled) — без сжатия для совместимости с GitHub Pages без CORS-заголовков"
  - "EmbeddedJson в PlayerActions.cs — Input System на WebGL не читает файл из StreamingAssets, нужен embedded JSON"
  - "force push при создании orphan-ветки — намеренно, первый деплой"

patterns-established:
  - "gh-pages деплой: orphan-ветка + .nojekyll + git worktree для изоляции от рабочей директории"

requirements-completed: [WEBGL-04, WEBGL-05, WEBGL-06, WEBGL-07]

# Metrics
duration: ~45min
completed: 2026-03-31
---

# Phase 09 Plan 02: GitHub Pages Deploy Summary

**Asteroids WebGL-игра задеплоена на GitHub Pages по адресу https://selstrom.github.io/asteroids-specs/ через orphan-ветку gh-pages с .nojekyll и отключённой компрессией WebGL**

## Performance

- **Duration:** ~45 min
- **Started:** 2026-03-31T12:00:00Z
- **Completed:** 2026-03-31T12:45:00Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments

- Создана orphan-ветка gh-pages с WebGL-сборкой и .nojekyll через git worktree
- GitHub Pages включён и настроен на ветку gh-pages / (root)
- Игра успешно открывается и запускается по https://selstrom.github.io/asteroids-specs/
- Исправлены два WebGL-специфичных бага: компрессия и Input System embedded JSON

## Task Commits

1. **Task 1: Создать orphan-ветку gh-pages и задеплоить файлы** — `928a4f8` → `100f4570` (chore: deploy WebGL build to gh-pages)
2. **Task 2: Включить GitHub Pages и верифицировать** — выполнено пользователем вручную через GitHub UI

**Дополнительные фиксы (deviations):**
- `621b721` — fix: webGLCompressionFormat 0→2 (Disabled) в ProjectSettings.asset
- `23d88bc` — fix: EmbeddedJson в PlayerActions.cs для WebGL

## Files Created/Modified

- `.nojekyll` (ветка gh-pages) — отключает Jekyll pipeline, предотвращает 404 на _-файлах Unity
- `index.html` (ветка gh-pages) — точка входа WebGL, скопирован из Build/WebGL/
- `Build/WebGL.*` (ветка gh-pages) — файлы Unity WebGL сборки (data, wasm, framework.js, loader.js)
- `ProjectSettings/ProjectSettings.asset` — webGLCompressionFormat изменён с 0 на 2 (Disabled)
- `Assets/Scripts/PlayerActions.cs` — добавлен EmbeddedJson для WebGL Input System

## Decisions Made

- **webGLCompressionFormat=2 (Disabled):** GitHub Pages не устанавливает заголовки Content-Encoding, поэтому сжатые файлы вызывают ошибку декодирования в браузере. Без сжатия — файлы крупнее, но стабильно работают.
- **EmbeddedJson в PlayerActions.cs:** Unity Input System на WebGL не может читать .inputactions файл из StreamingAssets (нет доступа к файловой системе). Embedded JSON компилируется прямо в сборку.
- **Force push при деплое orphan-ветки:** намеренно — orphan ветка не имеет общей истории с master, обычный push невозможен.
- **git worktree для деплоя:** позволяет работать с gh-pages без переключения ветки в рабочей директории.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] webGLCompressionFormat вызывал ошибку декодирования в браузере**
- **Found during:** Task 2 (верификация игры в браузере)
- **Issue:** ProjectSettings.asset имел webGLCompressionFormat=0 (Brotli), GitHub Pages не поддерживает декодирование сжатых файлов без CORS-заголовков
- **Fix:** Изменён на webGLCompressionFormat=2 (Disabled), пересборка и повторный деплой
- **Files modified:** ProjectSettings/ProjectSettings.asset
- **Verification:** Игра открылась без ошибок декодирования
- **Committed in:** 621b721

**2. [Rule 1 - Bug] Input System на WebGL не читает .inputactions файл**
- **Found during:** Task 2 (верификация управления в браузере)
- **Issue:** Unity Input System пытается читать файл из StreamingAssets на WebGL, файловая система недоступна → управление не работало
- **Fix:** PlayerActions.cs изменён для использования EmbeddedJson — JSON actions компилируется в сборку
- **Files modified:** Assets/Scripts/PlayerActions.cs
- **Verification:** Управление кораблём работает в браузере (WASD/стрелки)
- **Committed in:** 23d88bc

---

**Total deviations:** 2 auto-fixed (2 Rule 1 - Bug)
**Impact on plan:** Оба фикса необходимы для корректной работы WebGL. Без них игра не открывалась или не управлялась.

## Issues Encountered

- **Первоначальный деплой 928a4f8 заменён 100f4570:** После исправления webGLCompressionFormat потребовался повторный force push с пересобранными файлами. Первый деплой был промежуточным.
- **GitHub Pages задержка:** После нажатия Save в настройках требуется 1-3 минуты до активации URL — ожидаемое поведение.

## User Setup Required

Пользователь выполнил одно ручное действие:
- Открыл https://github.com/SelStrom/Asteroids-specs/settings/pages
- Настроил: Source → Deploy from a branch → gh-pages / (root) → Save

Это стандартный checkpoint:human-action — GitHub Pages нельзя включить через API без токена с нужными правами.

## Next Phase Readiness

- Фаза 09-webgl полностью завершена: игра доступна по публичной ссылке
- Все требования WEBGL-04, WEBGL-05, WEBGL-06, WEBGL-07 выполнены
- Milestone v1.0 достигнут — играбельный Asteroids в браузере

---
*Phase: 09-webgl*
*Completed: 2026-03-31*
