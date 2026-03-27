# Phase 2: MCP Basic — Context

**Gathered:** 2026-03-27
**Status:** Ready for planning

<domain>
## Phase Boundary

Реализовать TypeScript MCP-сервер (`Editor~/Server/`) и C# HttpListener-мост (`McpUnityBridge.cs`) внутри embedded-пакета `com.shtl.mcp-unity`. Scaffold из Phase 1 уже существует. Цель: Claude Code может вызывать `compile`, `play`, `stop`, `list_scenes`, `open_scene`, `import_asset` через MCP.

**Runtime bridge (`get_game_state`) и доступ к игровому состоянию — Phase 8, не сейчас.**

</domain>

<decisions>
## Implementation Decisions

### TypeScript сервер
- **D-01:** Транспорт — stdio. Сервер запускается как `node dist/index.js` — именно так Claude Code вызывает MCP-серверы.
- **D-02:** Сборка — `tsc` напрямую (не esbuild/tsup). `package.json` в `Editor~/Server/` содержит `"build": "tsc"`. Выход: `dist/index.js`.
- **D-03:** Зависимости: `@modelcontextprotocol/sdk` (последняя стабильная), `node-fetch` или встроенный `fetch` для HTTP-запросов к bridge.
- **D-04:** Точка входа: `src/index.ts`. Регистрирует все инструменты через `server.tool(...)` MCP SDK, затем подключает stdio-транспорт.

### C# HTTP Bridge (`McpUnityBridge.cs`)
- **D-05:** Порт: `localhost:8765`. Слушает все tool-запросы: `/compile`, `/play`, `/stop`, `/list_scenes`, `/open_scene`, `/import_asset`.
- **D-06:** Авто-старт через `[InitializeOnLoad]` — `McpUnityBridge` инициализируется при загрузке Unity Editor и перезапускается автоматически после domain reload.
- **D-07:** Потоковая модель: HttpListener работает в отдельном Thread (не async/await — Unity Editor не поддерживает `async void` в InitializeOnLoad). Поток демонизируется (`IsBackground = true`).
- **D-08:** Dispatching в главный поток: запросы к `AssetDatabase`, `EditorApplication`, `EditorSceneManager` выполняются через `EditorApplication.delayCall` (не напрямую из thread).
- **D-09:** При закрытии Unity Editor — bridge останавливается через `[InitializeOnLoad]` с подпиской на `AssemblyReloadEvents.beforeAssemblyReload` и `EditorApplication.quitting`.

### Формат ответов инструментов
- **D-10:** Все ответы — JSON. Успех: `{ "success": true, "message": "..." }`. Ошибка: `{ "success": false, "errors": [...] }`.
- **D-11:** `compile` при ошибках компиляции возвращает полный список: `{ "success": false, "errorCount": N, "errors": [{ "file": "...", "line": N, "column": N, "message": "...", "severity": "error|warning" }] }`.
- **D-12:** `compile` при успехе: `{ "success": true, "message": "Compilation successful. 0 errors." }`.
- **D-13:** `list_scenes` возвращает массив путей: `{ "success": true, "scenes": ["Assets/Scenes/Main.unity"] }`.
- **D-14:** `open_scene` и `import_asset` возвращают `{ "success": true, "message": "..." }` или `{ "success": false, "message": "..." }`.

### mcp.json / регистрация сервера
- **D-15:** Файл `.mcp.json` в корне проекта (auto-detect Claude Code). Формат:
  ```json
  {
    "mcpServers": {
      "mcp-unity": {
        "command": "node",
        "args": ["Packages/com.shtl.mcp-unity/Editor~/Server/dist/index.js"]
      }
    }
  }
  ```
- **D-16:** Путь к `dist/index.js` — относительный от корня проекта. Пользователь должен выполнить `npm install && npm run build` один раз в `Editor~/Server/` перед использованием.

### Claude's Discretion
- Конкретная версия `@modelcontextprotocol/sdk` (последняя стабильная на момент реализации)
- Структура `tsconfig.json` (target, module, outDir)
- Обработка таймаутов HTTP-запросов (разумный default ~5s)
- Формат логирования в Unity Console

</decisions>

<specifics>
## Specific Ideas

- ROADMAP success criteria: `npm install && npm run build` в `Editor~/Server/` без ошибок; `node dist/index.js` запускается — это цель сборки.
- Сервер отображается в `claude mcp list` после регистрации через `.mcp.json`.
- `[InitializeOnLoad]` — стандартный паттерн Unity для Editor Extensions, избавляет от необходимости ручного старта bridge.

</specifics>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Требования Phase 2
- `.planning/REQUIREMENTS.md` §MCP — MCP-01..MCP-06, MCP-08..MCP-10, MCP-12..MCP-13
- `.planning/ROADMAP.md` §Phase 2 — Success Criteria и список инструментов

### Структура проекта
- `.planning/codebase/STRUCTURE.md` — куда кладётся `McpUnityBridge.cs` (в `Packages/com.shtl.mcp-unity/Editor/`)
- `.planning/codebase/STACK.md` — пакеты, версии Unity, asmdef-файлы
- `.planning/codebase/ARCHITECTURE.md` — паттерны кода: `[InitializeOnLoad]`, Editor Extensions, `EditorApplication.delayCall`

### Контекст Phase 1
- `.planning/phases/01-project-foundation/1-CONTEXT.md` §D-13 — описание scaffold embedded-пакета

### Проект
- `.planning/PROJECT.md` §Key Decisions — TypeScript для MCP-сервера, embedded package

### Нет внешних спеков
- Anthropic MCP SDK документация доступна через npm/GitHub — researcher должен проверить актуальный API.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Packages/com.shtl.mcp-unity/package.json` — существует, версия 0.1.0
- `Packages/com.shtl.mcp-unity/Editor~/Server/` — пустая папка, сюда идёт TypeScript сервер
- `Packages/com.shtl.mcp-unity/Editor/` — пустая папка, сюда идёт `McpUnityBridge.cs`
- `Packages/com.shtl.mcp-unity/Runtime/` — пустая папка (используется Phase 8)

### Established Patterns
- `[InitializeOnLoad]` — стандарт Unity Editor Extensions для авто-старта при загрузке домена
- `EditorApplication.delayCall` — паттерн dispatching работы в главный Editor-поток
- `AssemblyReloadEvents.beforeAssemblyReload` — hook для graceful shutdown перед domain reload
- Проект использует C# 9.0, .NET Standard 2.1

### Integration Points
- `McpUnityBridge.cs` — standalone Editor Extension, не зависит от игрового кода
- `Assets/Editor/AsteroidsEditor.asmdef` — существует, но bridge лучше держать в своём пакете (`com.shtl.mcp-unity/Editor/`)
- `.mcp.json` → корень проекта (рядом с `Assets/`, `Packages/`, `ProjectSettings/`)

</code_context>

<deferred>
## Deferred Ideas

- `get_game_state` — чтение состояния игры (счёт, волна, жизни) — Phase 8 (MCP Runtime)
- `RuntimeBridgeProxy.cs` — прокси для Runtime-данных — Phase 8
- Горячая перезагрузка TypeScript-сервера (watch mode) — Claude's discretion в Phase 2, или backlog

</deferred>

---

*Phase: 02-mcp-basic*
*Context gathered: 2026-03-27*
