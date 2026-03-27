# Phase 2: MCP Basic — Research

**Researched:** 2026-03-27
**Domain:** TypeScript MCP Server (stdio) + Unity C# HttpListener Editor Bridge
**Confidence:** HIGH

## Summary

Phase 2 строит два компонента: TypeScript MCP-сервер (stdio-транспорт, `@modelcontextprotocol/sdk`) и C# Editor Extension (`McpUnityBridge.cs`) с `HttpListener` на `localhost:8765`. Оба компонента помещаются в embedded-пакет `com.shtl.mcp-unity`, scaffold которого уже создан в Phase 1.

TypeScript-сторона использует стабильный SDK версии `1.28.0` (опубликован 2026-03-25). API `McpServer` + `StdioServerTransport` + `server.tool()` с Zod-валидацией — стандартный и хорошо документированный паттерн. C#-сторона реализует классический паттерн Unity Editor Extension: `[InitializeOnLoad]` статический конструктор запускает `Thread` с `HttpListener`, вызовы Unity API маршрутизируются в главный поток через `EditorApplication.delayCall`. Компиляционные ошибки извлекаются через `CompilationPipeline.assemblyCompilationFinished` с `CompilerMessage[]`.

Регистрация в Claude Code через `.mcp.json` в корне проекта — подтверждена официальной документацией Claude Code.

**Первичная рекомендация:** Реализовать в строгом соответствии с решениями CONTEXT.md. SDK и Unity API стабильны, нестандартных решений не требуется.

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

**TypeScript сервер:**
- D-01: Транспорт — stdio. Сервер запускается как `node dist/index.js`.
- D-02: Сборка — `tsc` напрямую (не esbuild/tsup). `package.json` содержит `"build": "tsc"`. Выход: `dist/index.js`.
- D-03: Зависимости: `@modelcontextprotocol/sdk` (последняя стабильная), `node-fetch` или встроенный `fetch`.
- D-04: Точка входа: `src/index.ts`. Регистрирует все инструменты через `server.tool(...)`, затем подключает stdio-транспорт.

**C# HTTP Bridge:**
- D-05: Порт `localhost:8765`. Эндпоинты: `/compile`, `/play`, `/stop`, `/list_scenes`, `/open_scene`, `/import_asset`.
- D-06: Авто-старт через `[InitializeOnLoad]`.
- D-07: HttpListener в отдельном `Thread` с `IsBackground = true`.
- D-08: Dispatching через `EditorApplication.delayCall`.
- D-09: Graceful shutdown через `AssemblyReloadEvents.beforeAssemblyReload` и `EditorApplication.quitting`.

**Формат ответов:**
- D-10: JSON. Успех: `{ "success": true, "message": "..." }`. Ошибка: `{ "success": false, "errors": [...] }`.
- D-11: `compile` при ошибках: `{ "success": false, "errorCount": N, "errors": [{ "file", "line", "column", "message", "severity" }] }`.
- D-12: `compile` при успехе: `{ "success": true, "message": "Compilation successful. 0 errors." }`.
- D-13: `list_scenes`: `{ "success": true, "scenes": ["Assets/Scenes/Main.unity"] }`.
- D-14: `open_scene`, `import_asset`: `{ "success": true/false, "message": "..." }`.

**mcp.json:**
- D-15: Файл `.mcp.json` в корне проекта. Формат:
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
- D-16: Путь к `dist/index.js` — относительный от корня проекта.

### Claude's Discretion
- Конкретная версия `@modelcontextprotocol/sdk` (последняя стабильная на момент реализации)
- Структура `tsconfig.json` (target, module, outDir)
- Обработка таймаутов HTTP-запросов (разумный default ~5s)
- Формат логирования в Unity Console

### Deferred Ideas (OUT OF SCOPE)
- `get_game_state` — Phase 8
- `RuntimeBridgeProxy.cs` — Phase 8
- Hot reload / watch mode TypeScript-сервера
</user_constraints>

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| MCP-01 | Пакет имеет корректную структуру: `package.json`, `Editor/`, `Runtime/`, `Editor~/Server/`, два `.asmdef` | Scaffold из Phase 1 уже создан; нужно добавить `.asmdef` файлы и наполнить директории |
| MCP-02 | TypeScript MCP-сервер в `Editor~/Server/` запускается как `node dist/index.js` (stdio) | McpServer + StdioServerTransport паттерн подтверждён; tsconfig с `module: Node16`, `outDir: dist` |
| MCP-03 | `McpUnityBridge.cs` запускает HttpListener на `localhost:8765` при загрузке Editor (`[InitializeOnLoad]`) | InitializeOnLoad + Thread паттерн стандартный; Thread.IsBackground=true для демонизации |
| MCP-04 | MCP tool `compile` — выполняет `AssetDatabase.Refresh()` и возвращает статус компиляции | `CompilationPipeline.assemblyCompilationFinished` даёт `CompilerMessage[]` с file/line/column/severity |
| MCP-05 | MCP tool `play` — запускает Play Mode (`EditorApplication.isPlaying = true`) | Прямое присваивание; обязателен dispatch через `delayCall` — нельзя вызывать из background thread |
| MCP-06 | MCP tool `stop` — останавливает Play Mode (`EditorApplication.isPlaying = false`) | Аналогично play; нужен `delayCall` dispatch |
| MCP-08 | MCP tool `list_scenes` — возвращает список `.unity` файлов из `AssetDatabase` | `AssetDatabase.FindAssets("t:Scene")` + `GUIDToAssetPath` — стандартный паттерн |
| MCP-09 | MCP tool `open_scene` — открывает сцену по пути (`EditorSceneManager.OpenScene`) | `EditorSceneManager.OpenScene(path, OpenSceneMode.Single)` — из `UnityEditor.SceneManagement` |
| MCP-10 | MCP tool `import_asset` — выполняет `AssetDatabase.ImportAsset(path)` | Прямой вызов; нужен `delayCall` dispatch из background thread |
| MCP-12 | `Editor~/Server/mcp.json` содержит конфиг для регистрации в Claude Code | Исходя из D-15/D-16; формат подтверждён официальной документацией Claude Code |
| MCP-13 | `Editor~/Server/README.md` с инструкцией: `npm install`, `npm run build`, регистрация в Claude Code | Статический контент; никаких технических зависимостей |
</phase_requirements>

---

## Standard Stack

### Core

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| `@modelcontextprotocol/sdk` | 1.28.0 | MCP server implementation, tool registration, stdio transport | Официальный Anthropic SDK; единственный поддерживаемый путь интеграции с Claude Code |
| `zod` | ^3.x | Input schema validation для `server.tool()` | Peer dependency MCP SDK; required для типобезопасной валидации tool-параметров |
| TypeScript | ^5.x | Компиляция `src/` → `dist/` | Статическая типизация; совместим с Node16 module resolution |
| Node.js | v22.18.0 (runtime) | Выполнение `node dist/index.js` | Доступен на машине разработчика (подтверждено); встроенный `fetch` доступен с Node 18+ |

### Supporting (C# сторона)

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| `System.Net.HttpListener` | .NET Standard 2.1 | HTTP-сервер на localhost:8765 | Встроен в .NET; не требует NuGet-зависимостей |
| `UnityEditor.Compilation.CompilationPipeline` | Unity 2022.3 | Получение ошибок компиляции | Используется в `compile` tool handler |
| `UnityEditor.SceneManagement.EditorSceneManager` | Unity 2022.3 | Открытие сцен | Используется в `open_scene` |
| `UnityEditor.AssetDatabase` | Unity 2022.3 | Refresh, FindAssets, ImportAsset | Используется в `compile`, `list_scenes`, `import_asset` |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| `tsc` | `esbuild` / `tsup` | esbuild быстрее, но D-02 зафиксировал `tsc`; не менять |
| stdlib `fetch` (Node 18+) | `node-fetch` | Node 22 включает встроенный `fetch`; `node-fetch` нужен только для Node < 18 |
| `HttpListener` | Kestrel / Nancy | HttpListener встроен в .NET Standard 2.1, нет внешних зависимостей |
| `EditorApplication.delayCall` | `EditorApplication.update` | `delayCall` — одноразовый, достаточен для обработки одного HTTP-запроса |

**Installation:**
```bash
# В Editor~/Server/
npm install @modelcontextprotocol/sdk zod
npm install --save-dev typescript @types/node
```

**Version verification (подтверждено):**
- `@modelcontextprotocol/sdk`: **1.28.0** (npm registry, 2026-03-25)
- Node.js runtime: **v22.18.0** (доступен, встроенный fetch — да)

---

## Architecture Patterns

### Recommended Project Structure

```
Packages/com.shtl.mcp-unity/
├── package.json                        # УЖЕ СУЩЕСТВУЕТ (Phase 1)
├── Editor/
│   ├── McpUnityBridge.cs               # C# HttpListener (создать в Phase 2)
│   └── McpUnityBridge.asmdef           # Editor-only assembly def (создать в Phase 2)
├── Editor~/
│   └── Server/
│       ├── src/
│       │   └── index.ts                # TypeScript MCP server entry point
│       ├── dist/
│       │   └── index.js                # Compiled output (gitignored)
│       ├── package.json                # npm: build script, dependencies
│       ├── tsconfig.json               # TypeScript config
│       ├── mcp.json                    # Claude Code registration config (MCP-12)
│       └── README.md                   # Setup instructions (MCP-13)
└── Runtime/                            # Пустая (Phase 8 — RuntimeBridgeProxy)
    └── McpUnityRuntime.asmdef          # Runtime assembly def (создать в Phase 2 для MCP-01)
```

### Pattern 1: TypeScript MCP Server (stdio)

**Что:** McpServer регистрирует инструменты через `server.tool()`, подключается к StdioServerTransport.
**Когда использовать:** Всегда для stdio-серверов, вызываемых как дочерний процесс.

```typescript
// Source: https://github.com/modelcontextprotocol/typescript-sdk/blob/main/docs/server.md
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { z } from "zod";

const server = new McpServer({
  name: "mcp-unity",
  version: "0.1.0",
});

// Регистрация инструмента с Zod-схемой и async handler
server.tool(
  "compile",
  "Compile Unity scripts and return errors",
  {}, // пустая схема — нет параметров
  async () => {
    const response = await fetch("http://localhost:8765/compile", {
      method: "POST",
      signal: AbortSignal.timeout(5000),
    });
    const data = await response.json();
    return {
      content: [{ type: "text", text: JSON.stringify(data) }],
    };
  }
);

// Инструмент с параметром
server.tool(
  "open_scene",
  "Open a Unity scene by path",
  { path: z.string().describe("Scene asset path, e.g. Assets/Scenes/Main.unity") },
  async ({ path }) => {
    const response = await fetch("http://localhost:8765/open_scene", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ path }),
      signal: AbortSignal.timeout(5000),
    });
    const data = await response.json();
    return {
      content: [{ type: "text", text: JSON.stringify(data) }],
    };
  }
);

const transport = new StdioServerTransport();
await server.connect(transport);
```

### Pattern 2: Unity [InitializeOnLoad] HttpListener с Thread

**Что:** Статический конструктор класса с `[InitializeOnLoad]` запускает HttpListener в фоновом потоке. Unity API вызывается через `EditorApplication.delayCall`.
**Когда использовать:** Всегда когда нужен авто-старт Editor Extension и вызов Unity API из нестандартного потока.

```csharp
// Source: Unity Scripting API + community patterns
using System;
using System.Net;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class McpUnityBridge
{
    private static HttpListener _listener;
    private static Thread _listenerThread;
    private static volatile bool _running;

    // Статический конструктор — вызывается при загрузке домена
    static McpUnityBridge()
    {
        // Подписаться на события жизненного цикла
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        EditorApplication.quitting += OnEditorQuitting;

        StartServer();
    }

    private static void StartServer()
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://localhost:8765/");
        _listener.Start();
        _running = true;

        _listenerThread = new Thread(ListenLoop) { IsBackground = true };
        _listenerThread.Start();

        Debug.Log("[McpUnityBridge] Запущен на localhost:8765");
    }

    private static void ListenLoop()
    {
        while (_running)
        {
            try
            {
                var context = _listener.GetContext(); // Блокирует поток
                EditorApplication.delayCall += () => HandleRequest(context);
            }
            catch (HttpListenerException)
            {
                // Нормально при Stop() — выходим из цикла
                break;
            }
        }
    }

    private static void HandleRequest(HttpListenerContext context)
    {
        // Unity API вызывается здесь — в главном потоке через delayCall
        var path = context.Request.Url.LocalPath;
        // ... dispatch по path
    }

    private static void OnBeforeAssemblyReload()
    {
        StopServer();
    }

    private static void OnEditorQuitting()
    {
        StopServer();
    }

    private static void StopServer()
    {
        _running = false;
        _listener?.Stop();
        _listenerThread?.Join(1000);
        Debug.Log("[McpUnityBridge] Остановлен");
    }
}
```

### Pattern 3: Получение ошибок компиляции через CompilationPipeline

**Что:** Подписываемся на `assemblyCompilationFinished`, собираем `CompilerMessage[]` по всем сборкам, отдаём после `compilationFinished`.

```csharp
// Source: https://docs.unity3d.com/ScriptReference/Compilation.CompilationPipeline.html
using System.Collections.Generic;
using UnityEditor.Compilation;

// Внутри McpUnityBridge или отдельного класса:
private static List<CompilerMessage> _compilationMessages = new List<CompilerMessage>();
private static bool _compiling = false;

private static void SubscribeCompilation()
{
    CompilationPipeline.compilationStarted += _ => {
        _compilationMessages.Clear();
        _compiling = true;
    };
    CompilationPipeline.assemblyCompilationFinished += (assemblyName, messages) => {
        _compilationMessages.AddRange(messages);
    };
    CompilationPipeline.compilationFinished += _ => {
        _compiling = false;
        // Теперь _compilationMessages содержит все сообщения
    };
}

// При вызове /compile endpoint:
private static object HandleCompile()
{
    AssetDatabase.Refresh();
    // Refresh инициирует компиляцию асинхронно.
    // Простейший подход: вернуть результат последней компиляции.
    var errors = _compilationMessages.FindAll(m => m.type == CompilerMessageType.Error);
    if (errors.Count > 0)
    {
        return new {
            success = false,
            errorCount = errors.Count,
            errors = errors.ConvertAll(e => new {
                file = e.file,
                line = e.line,
                column = e.column,
                message = e.message,
                severity = "error"
            })
        };
    }
    return new { success = true, message = "Compilation successful. 0 errors." };
}
```

### Pattern 4: AssetDatabase.FindAssets для list_scenes

```csharp
// Source: Unity Scripting API + community pattern (verified)
using UnityEditor;

private static string[] GetAllScenePaths()
{
    var guids = AssetDatabase.FindAssets("t:Scene");
    var paths = new string[guids.Length];
    for (int i = 0; i < guids.Length; i++)
    {
        paths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
    }
    return paths;
}
```

### Pattern 5: tsconfig.json для MCP сервера

**Стандартный tsconfig на основе официального шаблона `create-typescript-server`:**

```json
{
  "compilerOptions": {
    "target": "ES2022",
    "module": "Node16",
    "moduleResolution": "Node16",
    "outDir": "./dist",
    "rootDir": "./src",
    "strict": true,
    "esModuleInterop": true,
    "skipLibCheck": true,
    "forceConsistentCasingInFileNames": true,
    "resolveJsonModule": true
  },
  "include": ["src/**/*"],
  "exclude": ["node_modules", "dist"]
}
```

**Важно:** `module: "Node16"` требует `.js` расширений в import-путях в TypeScript коде:
```typescript
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js"; // .js — обязательно
```

### Pattern 6: package.json для TypeScript MCP сервера

```json
{
  "name": "mcp-unity-server",
  "version": "0.1.0",
  "private": true,
  "type": "module",
  "scripts": {
    "build": "tsc",
    "start": "node dist/index.js"
  },
  "dependencies": {
    "@modelcontextprotocol/sdk": "^1.28.0",
    "zod": "^3.24.0"
  },
  "devDependencies": {
    "typescript": "^5.8.0",
    "@types/node": "^22.0.0"
  }
}
```

**Важно:** `"type": "module"` необходим при использовании `module: "Node16"` с ES-модулями.

### Anti-Patterns to Avoid

- **Вызов Unity API напрямую из background thread:** `AssetDatabase`, `EditorSceneManager`, `EditorApplication.isPlaying` — все требуют главного потока. Всегда через `EditorApplication.delayCall`.
- **`async void` в `[InitializeOnLoad]`:** Unity не поддерживает async/await в статических конструкторах InitializeOnLoad. Использовать явный `Thread`.
- **Не вызывать `HttpListener.Stop()` перед domain reload:** Порт останется занят, следующий `StartServer()` упадёт с `AddressAlreadyInUseException`.
- **Отвечать на HTTP-запрос из background thread после `delayCall`:** Запись в `context.Response` должна происходить там же, где вызывается Unity API (в main thread), либо через синхронизацию.
- **`import` без `.js` при `module: "Node16"`:** TypeScript с Node16 module resolution требует явных `.js` расширений в import-путях даже для `.ts` файлов.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| MCP protocol (stdio JSON-RPC) | Custom stdin/stdout parser | `@modelcontextprotocol/sdk` | Протокол сложнее, чем кажется: framing, cancellation, initialization handshake |
| Tool input validation | Custom type checking | `zod` (через SDK) | Schema validation с TypeScript-типами из коробки |
| HTTP server в C# | Custom TCP listener | `System.Net.HttpListener` | Входит в .NET Standard 2.1, не требует NuGet |
| Получение ошибок компиляции | Парсинг Console.Log | `CompilationPipeline.assemblyCompilationFinished` | Structured `CompilerMessage[]` с file/line/column/type |
| Список сцен | Рекурсивный поиск файлов | `AssetDatabase.FindAssets("t:Scene")` | Unity-tracked, работает с meta-файлами, правильные пути |

**Key insight:** MCP SDK инкапсулирует весь транспортный слой (framing, ack, error codes). Не пытаться реализовать JSON-RPC самостоятельно.

---

## Common Pitfalls

### Pitfall 1: Unity API из background thread

**What goes wrong:** `NullReferenceException` или `InvalidOperationException` при вызове `AssetDatabase.Refresh()`, `EditorApplication.isPlaying = true` напрямую из `ListenLoop`.
**Why it happens:** Unity API thread-affine — большинство методов Editor API безопасны только в главном потоке.
**How to avoid:** Весь код с Unity API обернуть в `EditorApplication.delayCall += () => { /* Unity API */ }`.
**Warning signs:** Stack trace начинается с `Thread.` и содержит `UnityEditor.` вызовы.

### Pitfall 2: Порт 8765 уже занят после domain reload

**What goes wrong:** `HttpListenerException: Failed to listen on prefix 'http://localhost:8765/'` при перезагрузке домена (после компиляции).
**Why it happens:** Статический конструктор вызывается снова после domain reload, но предыдущий `HttpListener` не был закрыт.
**How to avoid:** Подписаться на `AssemblyReloadEvents.beforeAssemblyReload += StopServer` и в `StopServer` вызвать `_listener.Stop()` + `_listener.Close()`.
**Warning signs:** Ошибка появляется не при первом запуске, а при повторном (после сохранения скрипта).

### Pitfall 3: `module: Node16` требует `.js` в import путях

**What goes wrong:** `Error [ERR_MODULE_NOT_FOUND]` при `node dist/index.js` с сообщением об отсутствии модуля.
**Why it happens:** Node16 module resolution требует явных `.js` расширений даже когда исходный файл `.ts`.
**How to avoid:** Писать `import { X } from "./module.js"` в `.ts` файлах при `module: Node16`.
**Warning signs:** Ошибка при `node dist/index.js`, но не при `tsc`.

### Pitfall 4: HTTP ответ должен быть отправлен из того же контекста

**What goes wrong:** Запрос зависает или бросает исключение если ответ записывается из другого потока.
**Why it happens:** `HttpListenerContext.Response` — объект, привязанный к конкретному запросу; отправка из другого потока возможна, но требует аккуратности с `Close()`.
**How to avoid:** В `delayCall`-обработчике выполнить Unity API, сформировать ответ и записать в `context.Response.OutputStream`, затем `context.Response.Close()`.
**Warning signs:** Claude Code зависает при вызове tool без ошибки на C#-стороне.

### Pitfall 5: `AssetDatabase.Refresh()` асинхронен

**What goes wrong:** `compile` возвращает `0 errors` сразу, не дождавшись реальной компиляции.
**Why it happens:** `AssetDatabase.Refresh()` инициирует компиляцию, которая продолжается асинхронно после возврата метода.
**How to avoid:** Использовать `CompilationPipeline` события для сбора ошибок. Для синхронного ответа: возвращать результат предыдущей компиляции плюс статус `isCompiling`. Альтернатива: `AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport)` — блокирует, но может быть медленным.
**Warning signs:** `compile` всегда возвращает успех даже при наличии синтаксических ошибок.

### Pitfall 6: `.mcp.json` vs `mcp.json` — разные вещи

**What goes wrong:** Сервер не появляется в `claude mcp list`.
**Why it happens:** Claude Code ищет `.mcp.json` (с точкой) в корне проекта. `mcp.json` (без точки) в `Editor~/Server/` — отдельный файл для MCP-12, не для Claude Code auto-detect.
**How to avoid:** Создать `.mcp.json` (с точкой) в корне Unity проекта (рядом с `Assets/`, `Packages/`). Файл `mcp.json` в `Editor~/Server/` — описание сервера по MCP-12, это разные файлы.

---

## Code Examples

### Полный минимальный `src/index.ts`

```typescript
// Source: https://github.com/modelcontextprotocol/typescript-sdk (verified API)
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { z } from "zod";

const BRIDGE_URL = "http://localhost:8765";
const TIMEOUT_MS = 5000;

async function callBridge(path: string, body?: object): Promise<unknown> {
  const response = await fetch(`${BRIDGE_URL}${path}`, {
    method: "POST",
    headers: body ? { "Content-Type": "application/json" } : undefined,
    body: body ? JSON.stringify(body) : undefined,
    signal: AbortSignal.timeout(TIMEOUT_MS),
  });
  return response.json();
}

const server = new McpServer({ name: "mcp-unity", version: "0.1.0" });

server.tool("compile", "Compile Unity scripts", {}, async () => {
  const data = await callBridge("/compile");
  return { content: [{ type: "text" as const, text: JSON.stringify(data) }] };
});

server.tool("play", "Enter Unity Play Mode", {}, async () => {
  const data = await callBridge("/play");
  return { content: [{ type: "text" as const, text: JSON.stringify(data) }] };
});

server.tool("stop", "Exit Unity Play Mode", {}, async () => {
  const data = await callBridge("/stop");
  return { content: [{ type: "text" as const, text: JSON.stringify(data) }] };
});

server.tool("list_scenes", "List all Unity scenes", {}, async () => {
  const data = await callBridge("/list_scenes");
  return { content: [{ type: "text" as const, text: JSON.stringify(data) }] };
});

server.tool(
  "open_scene",
  "Open a Unity scene by asset path",
  { path: z.string().describe("Asset path, e.g. Assets/Scenes/Main.unity") },
  async ({ path }) => {
    const data = await callBridge("/open_scene", { path });
    return { content: [{ type: "text" as const, text: JSON.stringify(data) }] };
  }
);

server.tool(
  "import_asset",
  "Import/reimport a Unity asset",
  { path: z.string().describe("Asset path, e.g. Assets/Textures/ship.png") },
  async ({ path }) => {
    const data = await callBridge("/import_asset", { path });
    return { content: [{ type: "text" as const, text: JSON.stringify(data) }] };
  }
);

const transport = new StdioServerTransport();
await server.connect(transport);
```

### Skeleton `McpUnityBridge.cs` с dispatcher

```csharp
// Паттерн: InitializeOnLoad + Thread + delayCall dispatch
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class McpUnityBridge
{
    private static HttpListener _listener;
    private static Thread _thread;
    private static volatile bool _running;
    private static readonly List<CompilerMessage> _messages = new List<CompilerMessage>();

    static McpUnityBridge()
    {
        AssemblyReloadEvents.beforeAssemblyReload += Stop;
        EditorApplication.quitting += Stop;

        // Подписка на компиляцию
        CompilationPipeline.compilationStarted += _ => _messages.Clear();
        CompilationPipeline.assemblyCompilationFinished += (_, msgs) => _messages.AddRange(msgs);

        Start();
    }

    private static void Start()
    {
        if (_running) return;
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://localhost:8765/");
        _listener.Start();
        _running = true;
        _thread = new Thread(Loop) { IsBackground = true };
        _thread.Start();
        Debug.Log("[McpUnityBridge] Слушает localhost:8765");
    }

    private static void Loop()
    {
        while (_running)
        {
            try
            {
                var ctx = _listener.GetContext();
                // Dispatch в главный поток
                EditorApplication.delayCall += () => Handle(ctx);
            }
            catch (HttpListenerException) { break; }
            catch (ObjectDisposedException) { break; }
        }
    }

    private static void Handle(HttpListenerContext ctx)
    {
        var path = ctx.Request.Url.LocalPath;
        object result;
        try
        {
            result = path switch
            {
                "/compile" => HandleCompile(),
                "/play" => HandlePlay(),
                "/stop" => HandleStop(),
                "/list_scenes" => HandleListScenes(),
                "/open_scene" => HandleOpenScene(ctx),
                "/import_asset" => HandleImportAsset(ctx),
                _ => new { success = false, message = $"Unknown endpoint: {path}" }
            };
        }
        catch (Exception e)
        {
            result = new { success = false, message = e.Message };
        }

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(result);
        var bytes = Encoding.UTF8.GetBytes(json);
        ctx.Response.ContentType = "application/json";
        ctx.Response.ContentLength64 = bytes.Length;
        ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
        ctx.Response.Close();
    }

    private static object HandleCompile()
    {
        AssetDatabase.Refresh();
        var errors = _messages.FindAll(m => m.type == CompilerMessageType.Error);
        if (errors.Count > 0)
            return new { success = false, errorCount = errors.Count,
                errors = errors.ConvertAll(e => new { e.file, e.line, e.column, e.message, severity = "error" }) };
        return new { success = true, message = "Compilation successful. 0 errors." };
    }

    private static object HandlePlay()
    {
        EditorApplication.isPlaying = true;
        return new { success = true, message = "Play Mode started." };
    }

    private static object HandleStop()
    {
        EditorApplication.isPlaying = false;
        return new { success = true, message = "Play Mode stopped." };
    }

    private static object HandleListScenes()
    {
        var guids = AssetDatabase.FindAssets("t:Scene");
        var paths = System.Array.ConvertAll(guids, AssetDatabase.GUIDToAssetPath);
        return new { success = true, scenes = paths };
    }

    private static object HandleOpenScene(HttpListenerContext ctx)
    {
        var body = new StreamReader(ctx.Request.InputStream, Encoding.UTF8).ReadToEnd();
        var req = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(body);
        string scenePath = req.path;
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        return new { success = true, message = $"Scene opened: {scenePath}" };
    }

    private static object HandleImportAsset(HttpListenerContext ctx)
    {
        var body = new StreamReader(ctx.Request.InputStream, Encoding.UTF8).ReadToEnd();
        var req = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(body);
        string assetPath = req.path;
        AssetDatabase.ImportAsset(assetPath);
        return new { success = true, message = $"Asset imported: {assetPath}" };
    }

    private static void Stop()
    {
        _running = false;
        try { _listener?.Stop(); _listener?.Close(); } catch { }
        _thread?.Join(500);
        Debug.Log("[McpUnityBridge] Остановлен");
    }
}
```

**Важно:** `Newtonsoft.Json` уже доступен как `com.unity.nuget.newtonsoft-json` (транзитивная зависимость через `com.shtl.mvvm`).

### `.mcp.json` в корне проекта

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

### Файл `mcp.json` (в `Editor~/Server/`, для MCP-12)

```json
{
  "command": "node",
  "args": ["dist/index.js"]
}
```

### Assembly Definition для Editor/ (MCP-01)

```json
{
  "name": "McpUnity.Editor",
  "rootNamespace": "McpUnity.Editor",
  "references": [],
  "includePlatforms": ["Editor"],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": false,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
```

### Assembly Definition для Runtime/ (MCP-01)

```json
{
  "name": "McpUnity.Runtime",
  "rootNamespace": "McpUnity.Runtime",
  "references": [],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
```

---

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `Server` class (low-level) | `McpServer` class (high-level) | SDK ~1.5+ | Упрощает регистрацию tools; не нужно вручную обрабатывать `CallTool` |
| `server.setRequestHandler()` | `server.tool()` | SDK ~1.x | Декларативный API с Zod-схемами |
| `.claude.json` / ручная регистрация MCP | `.mcp.json` в корне проекта | Claude Code 2024 | Auto-detect, team-sharable |

**Deprecated/outdated:**
- Низкоуровневый `Server` класс (из `@modelcontextprotocol/sdk/server/index.js`): всё ещё доступен, но `McpServer` — рекомендуемый высокоуровневый API.

---

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| Node.js | `node dist/index.js` | ✓ | v22.18.0 | — |
| npm | `npm install`, `npm run build` | ✓ | 10.9.3 | — |
| TypeScript | `tsc` build | ✗ (devDep) | — | `npm install` установит из devDependencies |
| Unity 2022.3.60f1 | C# bridge | ✓ (по STACK.md) | 2022.3.60f1 | — |
| Newtonsoft.Json | JSON-сериализация в C# | ✓ | 3.2.2 (транзитивно через `com.shtl.mvvm`) | — |

**Missing dependencies with no fallback:** Нет.

**Missing dependencies with fallback:**
- TypeScript: устанавливается через `npm install --save-dev typescript` (входит в devDependencies пакета).

---

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | Manual / smoke-test (нет автоматизированных тестов для Editor Extensions) |
| Config file | — |
| Quick run command | `node dist/index.js --help` (проверка запуска) |
| Full suite command | Ручной тест: запустить Unity, вызвать `claude mcp list`, вызвать каждый tool |

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| MCP-01 | Корректная структура пакета | smoke | `ls Packages/com.shtl.mcp-unity/Editor/ && ls Packages/com.shtl.mcp-unity/Runtime/` | ❌ Wave 0 |
| MCP-02 | `node dist/index.js` стартует без ошибок | smoke | `cd Packages/com.shtl.mcp-unity/Editor~/Server && node dist/index.js < /dev/null` | ❌ Wave 0 |
| MCP-03 | HttpListener на 8765 | smoke | `curl -s -X POST http://localhost:8765/list_scenes` | ❌ требует Unity |
| MCP-04–10 | Tool calls | manual | `claude mcp list` + вызов tool из чата | ❌ требует Unity |
| MCP-12 | mcp.json содержит правильный формат | smoke | `cat Packages/com.shtl.mcp-unity/Editor~/Server/mcp.json \| python3 -m json.tool` | ❌ Wave 0 |
| MCP-13 | README.md существует | smoke | `test -f Packages/com.shtl.mcp-unity/Editor~/Server/README.md` | ❌ Wave 0 |

### Sampling Rate
- **Per task commit:** `cd Packages/com.shtl.mcp-unity/Editor~/Server && npm run build` (TypeScript компилируется без ошибок)
- **Per wave merge:** `npm run build` + `node dist/index.js` запускается
- **Phase gate:** Полное ручное тестирование всех 6 tools через Claude Code

### Wave 0 Gaps
- [ ] `Packages/com.shtl.mcp-unity/Editor~/Server/package.json` — npm конфигурация
- [ ] `Packages/com.shtl.mcp-unity/Editor~/Server/tsconfig.json` — TypeScript конфигурация
- [ ] `Packages/com.shtl.mcp-unity/Editor~/Server/src/index.ts` — entry point
- [ ] `Packages/com.shtl.mcp-unity/Editor/McpUnityBridge.asmdef` — Editor assembly def
- [ ] `Packages/com.shtl.mcp-unity/Runtime/McpUnityRuntime.asmdef` — Runtime assembly def

---

## Open Questions

1. **Синхронность `compile` ответа**
   - Что знаем: `AssetDatabase.Refresh()` асинхронен; `CompilationPipeline` события дают результаты через callback.
   - Что неясно: Нужно ли ждать завершения компиляции или возвращать результат последней компиляции.
   - Рекомендация: Возвращать результат последней завершённой компиляции + инициировать Refresh. Если нужен синхронный результат — использовать `AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport)` (блокирует главный поток, может быть медленным).

2. **`HandleRequest` из `delayCall` отвечает в HTTP ответ**
   - Что знаем: `delayCall` выполняется в главном потоке, но `HttpListenerContext` живёт в background thread.
   - Что неясно: Возможны ли race conditions при записи в `context.Response` из main thread.
   - Рекомендация: Писать в `Response` и закрывать его в том же `delayCall`-лямбде; это безопасно, так как `HttpListenerContext` thread-safe для операций ответа.

---

## Sources

### Primary (HIGH confidence)
- [modelcontextprotocol/typescript-sdk README + docs/server.md](https://github.com/modelcontextprotocol/typescript-sdk/blob/main/docs/server.md) — McpServer API, StdioServerTransport, server.tool()
- [npm: @modelcontextprotocol/sdk](https://www.npmjs.com/package/@modelcontextprotocol/sdk) — версия 1.28.0, дата публикации 2026-03-25
- [create-typescript-server tsconfig.json](https://github.com/modelcontextprotocol/create-typescript-server/blob/main/tsconfig.json) — официальный шаблон tsconfig
- [Unity Scripting API: EditorApplication.delayCall](https://docs.unity3d.com/ScriptReference/EditorApplication-delayCall.html) — threading dispatch pattern
- [Unity Scripting API: AssemblyReloadEvents.beforeAssemblyReload](https://docs.unity3d.com/ScriptReference/AssemblyReloadEvents-beforeAssemblyReload.html) — graceful shutdown hook
- [Unity Scripting API: CompilationPipeline](https://docs.unity3d.com/ScriptReference/Compilation.CompilationPipeline.html) — compile events, CompilerMessage
- [MCP Tools Protocol spec](https://modelcontextprotocol.io/docs/concepts/tools) — tool response format, error handling
- [Claude Code MCP docs](https://code.claude.com/docs/en/mcp) — .mcp.json format, auto-detect

### Secondary (MEDIUM confidence)
- [Unity Forum: HttpListener in its own thread](https://discussions.unity.com/t/httplistener-in-its-own-thread/819682) — паттерн подтверждён multiple sources
- [AssetDatabase.FindAssets t:Scene pattern](https://bronsonzgeb.com/index.php/2021/08/21/scene-selector-tool/) — стандартный Editor паттерн

### Tertiary (LOW confidence)
- Нет.

---

## Metadata

**Confidence breakdown:**
- Standard Stack: HIGH — версия SDK подтверждена npm registry; Node.js версия подтверждена machine probe
- Architecture: HIGH — официальный SDK docs + Unity Scripting API + официальный tsconfig шаблон
- Pitfalls: HIGH — domain reload и threading issues — задокументированные Unity проблемы с несколькими источниками

**Research date:** 2026-03-27
**Valid until:** 2026-06-27 (SDK стабильный; Unity API стабильный)
