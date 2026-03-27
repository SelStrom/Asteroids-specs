---
phase: 02-mcp-basic
plan: 01
subsystem: mcp
tags: [typescript, mcp-sdk, stdio, node, zod, npm]

# Dependency graph
requires:
  - phase: 01-project-foundation
    provides: scaffold embedded-пакета com.shtl.mcp-unity с пустой директорией Editor~/Server/
provides:
  - TypeScript MCP-сервер с package.json, tsconfig.json и src/index.ts
  - Регистрация 6 инструментов: compile, play, stop, list_scenes, open_scene, import_asset
  - npm-сборка через tsc (npm run build -> dist/index.js)
affects:
  - 02-02 (C# bridge — зависит от того, что сервер проксирует на localhost:8765)
  - 02-03 (mcp.json — регистрирует dist/index.js)

# Tech tracking
tech-stack:
  added:
    - "@modelcontextprotocol/sdk@1.28.0"
    - "zod@^3.24.0"
    - "typescript@^5.8.0"
    - "@types/node@^22.0.0"
  patterns:
    - McpServer + StdioServerTransport — стандартный паттерн stdio MCP-сервера
    - server.tool() с Zod-схемой для валидации параметров
    - fetch() с AbortSignal.timeout(5000) для HTTP-запросов к C# bridge
    - TypeScript module:Node16 + outDir:dist для ESM-совместимого вывода

key-files:
  created:
    - Packages/com.shtl.mcp-unity/Editor~/Server/package.json
    - Packages/com.shtl.mcp-unity/Editor~/Server/tsconfig.json
    - Packages/com.shtl.mcp-unity/Editor~/Server/package-lock.json
    - Packages/com.shtl.mcp-unity/Editor~/Server/src/index.ts
  modified: []

key-decisions:
  - "@modelcontextprotocol/sdk@1.28.0 — зафиксирована точная версия (последняя стабильная на 2026-03-25, per RESEARCH.md)"
  - "module:Node16 + moduleResolution:Node16 — обязательно для .js расширений в импортах SDK"
  - "dist/ игнорируется через .gitignore — пользователь запускает npm run build вручную перед использованием"
  - "AbortSignal.timeout(5000) — встроенный таймаут 5с для всех HTTP-запросов к bridge"

patterns-established:
  - "MCP tool registration: server.tool(name, description, zodSchema, asyncHandler)"
  - "Tool response format: { content: [{ type: 'text', text: JSON.stringify(data) }] }"
  - "Bridge proxy pattern: fetch('http://localhost:8765/{endpoint}') в каждом tool handler"

requirements-completed: [MCP-01, MCP-02]

# Metrics
duration: 2min
completed: 2026-03-27
---

# Phase 2 Plan 01: TypeScript MCP-сервер — Scaffold Summary

**TypeScript MCP-сервер с 6 инструментами (compile/play/stop/list_scenes/open_scene/import_asset) на @modelcontextprotocol/sdk@1.28.0, компилируется через tsc в Node16/ESM**

## Performance

- **Duration:** 2 мин
- **Started:** 2026-03-27T15:59:36Z
- **Completed:** 2026-03-27T16:01:12Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments

- Создан package.json с "build": "tsc", @modelcontextprotocol/sdk@1.28.0, zod@^3.24.0 и TypeScript dev-зависимостями
- Создан tsconfig.json с target:ES2022, module:Node16, moduleResolution:Node16, outDir:dist, strict:true
- Создан src/index.ts с регистрацией всех 6 MCP-инструментов через McpServer + StdioServerTransport
- npm run build завершается exit 0, node dist/index.js корректно завершается при EOF (exit 0)

## Task Commits

Каждая задача закоммичена атомарно:

1. **Задача 1: package.json и tsconfig.json** — `e0758a2` (chore)
2. **Задача 2: src/index.ts — точка входа MCP-сервера** — `f8ad726` (feat)

**Метаданные плана:** (docs-коммит ниже)

## Files Created/Modified

- `Packages/com.shtl.mcp-unity/Editor~/Server/package.json` — npm-конфигурация с build:tsc и всеми зависимостями
- `Packages/com.shtl.mcp-unity/Editor~/Server/tsconfig.json` — TypeScript конфигурация (ES2022, Node16, outDir:dist)
- `Packages/com.shtl.mcp-unity/Editor~/Server/package-lock.json` — lock-файл после npm install (94 пакета)
- `Packages/com.shtl.mcp-unity/Editor~/Server/src/index.ts` — точка входа с 6 MCP tools и stdio-транспортом

## Decisions Made

- `@modelcontextprotocol/sdk@1.28.0` — зафиксирована точная версия (latest stable per RESEARCH.md)
- `module: Node16` + `moduleResolution: Node16` — обязательно для корректного разрешения .js расширений в SDK-импортах
- `dist/` игнорируется в .gitignore — генерируемые файлы, пользователь запускает npm run build вручную (per D-16)
- `AbortSignal.timeout(5000)` — встроенный таймаут без внешних зависимостей (node-fetch не нужен)

## Deviations from Plan

None — план выполнен точно по спецификации.

## Issues Encountered

None — npm install завершился без ошибок и уязвимостей, tsc скомпилировал без предупреждений.

## User Setup Required

Перед первым использованием MCP-сервера выполнить:
```bash
cd Packages/com.shtl.mcp-unity/Editor~/Server
npm install && npm run build
```

После этого сервер готов к регистрации в .mcp.json (план 02-03).

## Next Phase Readiness

- TypeScript MCP-сервер готов: компилируется, запускается, регистрирует 6 инструментов
- Следующий шаг: план 02-02 — C# HttpListener bridge на localhost:8765
- dist/ не коммитится — пересборка нужна после npm install на новой машине

---

*Phase: 02-mcp-basic*
*Completed: 2026-03-27*
