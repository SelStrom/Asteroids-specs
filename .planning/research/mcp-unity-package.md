# MCP Unity Package — Research Notes

**Дата:** 2026-03-27
**Область:** TypeScript MCP-сервер + Unity Editor HTTP-мост, встроенный в UPM-пакет
**Confidence:** MEDIUM — опирается на знание обучающей выборки (август 2025); WebSearch/WebFetch недоступны. Всё описанное совпадает с официальным API на дату среза знаний. Рекомендуется проверить текущую версию `@modelcontextprotocol/sdk` перед реализацией.

---

## 1. Anthropic MCP TypeScript SDK (`@modelcontextprotocol/sdk`)

### Актуальная версия (на август 2025)

```
@modelcontextprotocol/sdk  ^1.0.0
```

Пакет опубликован на npm. Основные экспорты:
- `Server` — класс MCP-сервера
- `StdioServerTransport` — транспорт через stdin/stdout
- Типы: `CallToolRequestSchema`, `ListToolsRequestSchema` и др. из `@modelcontextprotocol/sdk/types.js`

### Паттерн stdio-сервера (copy-paste-ready)

```typescript
import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import {
  CallToolRequestSchema,
  ListToolsRequestSchema,
} from "@modelcontextprotocol/sdk/types.js";

const server = new Server(
  {
    name: "mcp-unity",
    version: "1.0.0",
  },
  {
    capabilities: {
      tools: {},
    },
  }
);

// --- Объявление инструментов ---
server.setRequestHandler(ListToolsRequestSchema, async () => {
  return {
    tools: [
      {
        name: "play",
        description: "Запустить Play Mode в Unity Editor",
        inputSchema: {
          type: "object",
          properties: {},
          required: [],
        },
      },
      {
        name: "stop",
        description: "Остановить Play Mode",
        inputSchema: {
          type: "object",
          properties: {},
          required: [],
        },
      },
      {
        name: "compile",
        description: "Принудительная перекомпиляция скриптов (AssetDatabase.Refresh)",
        inputSchema: {
          type: "object",
          properties: {},
          required: [],
        },
      },
      {
        name: "get_game_state",
        description: "Получить игровое состояние: счёт, волна, жизни (только во время Play Mode)",
        inputSchema: {
          type: "object",
          properties: {},
          required: [],
        },
      },
      {
        name: "list_scenes",
        description: "Перечислить все сцены в проекте (через AssetDatabase)",
        inputSchema: {
          type: "object",
          properties: {},
          required: [],
        },
      },
      {
        name: "open_scene",
        description: "Открыть сцену по имени или пути",
        inputSchema: {
          type: "object",
          properties: {
            path: {
              type: "string",
              description: "Путь к сцене, например Assets/Scenes/Main.unity",
            },
          },
          required: ["path"],
        },
      },
      {
        name: "import_asset",
        description: "Импортировать ассет по пути (AssetDatabase.ImportAsset)",
        inputSchema: {
          type: "object",
          properties: {
            path: {
              type: "string",
              description: "Путь к ассету относительно Assets/",
            },
          },
          required: ["path"],
        },
      },
    ],
  };
});

// --- Диспетчер вызовов ---
server.setRequestHandler(CallToolRequestSchema, async (request) => {
  const { name, arguments: args } = request.params;

  try {
    switch (name) {
      case "play":
        return await callUnityBridge("/play", {});
      case "stop":
        return await callUnityBridge("/stop", {});
      case "compile":
        return await callUnityBridge("/compile", {});
      case "get_game_state":
        return await callUnityBridge("/game-state", {});
      case "list_scenes":
        return await callUnityBridge("/list-scenes", {});
      case "open_scene":
        return await callUnityBridge("/open-scene", { path: (args as any).path });
      case "import_asset":
        return await callUnityBridge("/import-asset", { path: (args as any).path });
      default:
        throw new Error(`Неизвестный инструмент: ${name}`);
    }
  } catch (err: any) {
    // Ошибка (Unity не запущен, таймаут и т.п.) возвращается как isError: true
    return {
      content: [
        {
          type: "text",
          text: `Ошибка: ${err.message}`,
        },
      ],
      isError: true,
    };
  }
});

// --- Запуск ---
async function main() {
  const transport = new StdioServerTransport();
  await server.connect(transport);
  // После connect() сервер читает stdin и пишет в stdout
  // Не используем console.log — он загрязняет stdout и ломает протокол MCP
}

main().catch((err) => {
  // Писать в stderr, не в stdout
  process.stderr.write(`Fatal: ${err.message}\n`);
  process.exit(1);
});
```

### Формат успешного ответа

```typescript
// Успех — plain text
return {
  content: [{ type: "text", text: "Play Mode запущен" }],
};

// Успех — JSON-данные
return {
  content: [{ type: "text", text: JSON.stringify(gameState, null, 2) }],
};

// Ошибка (мягкая — Claude видит текст, не исключение)
return {
  content: [{ type: "text", text: "Unity Editor не отвечает (порт 8765 недоступен)" }],
  isError: true,
};
```

**Правило:** никогда не бросать исключения из handler'а напрямую — SDK перехватит его и вернёт MCP error-ответ, который Claude может не отобразить понятно. Лучше возвращать `isError: true` с читаемым сообщением.

### package.json для TypeScript MCP-сервера

```json
{
  "name": "mcp-unity-server",
  "version": "1.0.0",
  "private": true,
  "type": "module",
  "main": "dist/index.js",
  "scripts": {
    "build": "tsc",
    "dev": "tsc --watch",
    "start": "node dist/index.js"
  },
  "dependencies": {
    "@modelcontextprotocol/sdk": "^1.0.0"
  },
  "devDependencies": {
    "typescript": "^5.4.0",
    "@types/node": "^20.0.0"
  }
}
```

### tsconfig.json

```json
{
  "compilerOptions": {
    "target": "ES2022",
    "module": "Node16",
    "moduleResolution": "Node16",
    "outDir": "dist",
    "rootDir": "src",
    "strict": true,
    "esModuleInterop": true,
    "skipLibCheck": true
  },
  "include": ["src/**/*.ts"],
  "exclude": ["node_modules", "dist"]
}
```

**Важно:** `"type": "module"` в package.json и `"module": "Node16"` в tsconfig — без этого ESM-импорты SDK (`import ... from ".../.js"`) не работают.

---

## 2. HTTP-вызов из TypeScript к Unity Editor

### Паттерн `callUnityBridge`

```typescript
const UNITY_BRIDGE_URL = "http://localhost:8765";
const TIMEOUT_MS = 5000;

async function callUnityBridge(
  endpoint: string,
  body: Record<string, unknown>
): Promise<{ content: Array<{ type: string; text: string }> }> {
  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), TIMEOUT_MS);

  try {
    const response = await fetch(`${UNITY_BRIDGE_URL}${endpoint}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
      signal: controller.signal,
    });

    clearTimeout(timeoutId);

    if (!response.ok) {
      const errText = await response.text();
      throw new Error(`Unity вернул ${response.status}: ${errText}`);
    }

    const data = await response.json() as UnityResponse;

    if (!data.ok) {
      throw new Error(data.error ?? "Неизвестная ошибка Unity");
    }

    return {
      content: [
        {
          type: "text",
          text: typeof data.result === "string"
            ? data.result
            : JSON.stringify(data.result, null, 2),
        },
      ],
    };
  } catch (err: any) {
    clearTimeout(timeoutId);
    if (err.name === "AbortError") {
      throw new Error(
        `Unity Editor не отвечает на ${endpoint} (таймаут ${TIMEOUT_MS}ms). ` +
        "Убедитесь, что Unity Editor открыт и MCP Bridge запущен."
      );
    }
    // ECONNREFUSED — Unity не запущен
    if (err.code === "ECONNREFUSED") {
      throw new Error(
        "Соединение отклонено. Unity Editor не запущен или HTTP-мост не активирован."
      );
    }
    throw err;
  }
}

interface UnityResponse {
  ok: boolean;
  result?: unknown;
  error?: string;
}
```

**Ключевые точки:**
- `fetch` доступен из Node.js 18+. Для Node 16 нужен `node-fetch`.
- `AbortController` с таймаутом — обязателен: без него MCP-сервер зависнет, если Unity не запущен.
- `ECONNREFUSED` — отличный сигнал: Unity не запущен, сообщаем об этом явно.
- Порт 8765 — рекомендуется (не конфликтует со стандартными сервисами, см. раздел 4).

---

## 3. Unity Editor HTTP Bridge (C#)

### `[InitializeOnLoad]` + `HttpListener` — базовый паттерн

```csharp
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Newtonsoft.Json;  // com.unity.nuget.newtonsoft-json уже в проекте

namespace McpUnity.Editor
{
    [InitializeOnLoad]
    public static class McpUnityBridge
    {
        private const int Port = 8765;
        private static HttpListener _listener;
        private static Thread _listenerThread;

        static McpUnityBridge()
        {
            StartBridge();
            // Останавливаем при выгрузке домена (перекомпиляция, закрытие Editor)
            AppDomain.CurrentDomain.DomainUnload += (_, __) => StopBridge();
        }

        private static void StartBridge()
        {
            if (_listener != null && _listener.IsListening)
            {
                return;
            }

            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{Port}/");

            try
            {
                _listener.Start();
            }
            catch (Exception e)
            {
                Debug.LogError($"[McpUnityBridge] Не удалось запустить HTTP-мост на порту {Port}: {e.Message}");
                return;
            }

            _listenerThread = new Thread(ListenLoop)
            {
                IsBackground = true,
                Name = "McpUnityBridge"
            };
            _listenerThread.Start();

            Debug.Log($"[McpUnityBridge] HTTP-мост запущен на порту {Port}");
        }

        private static void StopBridge()
        {
            try
            {
                _listener?.Stop();
                _listener?.Close();
            }
            catch
            {
                // игнорируем при завершении
            }
            _listener = null;
        }

        private static void ListenLoop()
        {
            while (_listener != null && _listener.IsListening)
            {
                HttpListenerContext context;
                try
                {
                    context = _listener.GetContext(); // блокирующий вызов
                }
                catch (HttpListenerException)
                {
                    break; // listener остановлен
                }
                catch (Exception e)
                {
                    Debug.LogError($"[McpUnityBridge] Ошибка получения запроса: {e.Message}");
                    continue;
                }

                // Обрабатываем каждый запрос в том же фоновом потоке.
                // Для команд, требующих Unity main thread, используем
                // EditorApplication.delayCall (см. ниже).
                ThreadPool.QueueUserWorkItem(_ => HandleRequest(context));
            }
        }

        private static void HandleRequest(HttpListenerContext context)
        {
            var req = context.Request;
            var resp = context.Response;
            resp.ContentType = "application/json; charset=utf-8";

            string body;
            using (var reader = new StreamReader(req.InputStream, Encoding.UTF8))
            {
                body = reader.ReadToEnd();
            }

            string path = req.Url.AbsolutePath.TrimEnd('/');

            try
            {
                switch (path)
                {
                    case "/play":
                        HandleOnMainThread(() => EditorApplication.isPlaying = true);
                        SendOk(resp, "Play Mode запущен");
                        break;

                    case "/stop":
                        HandleOnMainThread(() => EditorApplication.isPlaying = false);
                        SendOk(resp, "Play Mode остановлен");
                        break;

                    case "/compile":
                        HandleOnMainThread(() => AssetDatabase.Refresh());
                        SendOk(resp, "AssetDatabase.Refresh() вызван");
                        break;

                    case "/list-scenes":
                        // AssetDatabase.FindAssets можно вызывать из фонового потока
                        // в большинстве версий Unity 2022, но безопаснее — через main thread
                        string scenesJson = null;
                        HandleOnMainThreadSync(() =>
                        {
                            var guids = AssetDatabase.FindAssets("t:Scene");
                            var paths = new string[guids.Length];
                            for (int i = 0; i < guids.Length; i++)
                            {
                                paths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
                            }
                            scenesJson = JsonConvert.SerializeObject(paths);
                        });
                        SendOkRaw(resp, scenesJson);
                        break;

                    case "/open-scene":
                        var openArgs = JsonConvert.DeserializeObject<PathArgs>(body);
                        HandleOnMainThread(() =>
                        {
                            EditorSceneManager.OpenScene(openArgs.path,
                                OpenSceneMode.Single);
                        });
                        SendOk(resp, $"Сцена открыта: {openArgs.path}");
                        break;

                    case "/import-asset":
                        var importArgs = JsonConvert.DeserializeObject<PathArgs>(body);
                        HandleOnMainThread(() =>
                        {
                            AssetDatabase.ImportAsset(importArgs.path,
                                ImportAssetOptions.ForceUpdate);
                        });
                        SendOk(resp, $"Ассет импортирован: {importArgs.path}");
                        break;

                    case "/game-state":
                        // Делегируем Runtime-мосту (см. раздел «Runtime bridge»)
                        string stateJson = RuntimeBridgeProxy.GetLatestState();
                        if (stateJson == null)
                        {
                            SendError(resp, "Play Mode не активен или состояние ещё не получено");
                        }
                        else
                        {
                            SendOkRaw(resp, stateJson);
                        }
                        break;

                    default:
                        resp.StatusCode = 404;
                        SendError(resp, $"Неизвестный endpoint: {path}");
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[McpUnityBridge] Ошибка обработки {path}: {e}");
                resp.StatusCode = 500;
                SendError(resp, e.Message);
            }
        }

        // ---------------------------------------------------------------
        // Утилиты
        // ---------------------------------------------------------------

        /// <summary>
        /// Запланировать action на main thread (fire-and-forget, ответ не ждёт завершения).
        /// Подходит для /play, /stop, /compile, /open-scene, /import-asset.
        /// </summary>
        private static void HandleOnMainThread(Action action)
        {
            EditorApplication.delayCall += () => action();
        }

        /// <summary>
        /// Запустить action на main thread и синхронно дождаться результата.
        /// Использует ManualResetEventSlim. Подходит для /list-scenes.
        /// ВНИМАНИЕ: вызывать только из фонового потока, иначе — deadlock.
        /// </summary>
        private static void HandleOnMainThreadSync(Action action)
        {
            var done = new ManualResetEventSlim(false);
            Exception caughtEx = null;
            EditorApplication.delayCall += () =>
            {
                try { action(); }
                catch (Exception e) { caughtEx = e; }
                finally { done.Set(); }
            };
            // Ждём завершения (с таймаутом, чтобы не зависнуть при ошибке)
            if (!done.Wait(TimeSpan.FromSeconds(10)))
            {
                throw new TimeoutException("Main thread не ответил за 10 секунд");
            }
            if (caughtEx != null)
            {
                throw caughtEx;
            }
        }

        private static void SendOk(HttpListenerResponse resp, string message)
        {
            var json = JsonConvert.SerializeObject(new { ok = true, result = message });
            var bytes = Encoding.UTF8.GetBytes(json);
            resp.ContentLength64 = bytes.Length;
            resp.OutputStream.Write(bytes, 0, bytes.Length);
            resp.OutputStream.Close();
        }

        private static void SendOkRaw(HttpListenerResponse resp, string json)
        {
            var payload = JsonConvert.SerializeObject(new { ok = true, result = JsonConvert.DeserializeObject(json) });
            var bytes = Encoding.UTF8.GetBytes(payload);
            resp.ContentLength64 = bytes.Length;
            resp.OutputStream.Write(bytes, 0, bytes.Length);
            resp.OutputStream.Close();
        }

        private static void SendError(HttpListenerResponse resp, string message)
        {
            var json = JsonConvert.SerializeObject(new { ok = false, error = message });
            var bytes = Encoding.UTF8.GetBytes(json);
            resp.ContentLength64 = bytes.Length;
            resp.OutputStream.Write(bytes, 0, bytes.Length);
            resp.OutputStream.Close();
        }

        private class PathArgs
        {
            public string path;
        }
    }
}
```

### Важные нюансы Unity Editor API

| API | Thread-safety | Примечание |
|-----|--------------|------------|
| `EditorApplication.isPlaying = true/false` | Main thread only | Через `delayCall` |
| `AssetDatabase.Refresh()` | Main thread only | Через `delayCall` |
| `AssetDatabase.FindAssets()` | Main thread only (безопаснее) | Через `delayCall` + sync wait |
| `EditorSceneManager.OpenScene()` | Main thread only | Через `delayCall` |
| `AssetDatabase.ImportAsset()` | Main thread only | Через `delayCall` |
| `HttpListener.GetContext()` | Фоновый поток | Блокирующий вызов — нельзя на main thread |

### `EditorApplication.delayCall` vs `Update`

`EditorApplication.delayCall` — это multicast delegate, срабатывает в начале следующего кадра Editor'а. После срабатывания автоматически удаляет себя. Для fire-and-forget — оптимальный выбор. Для синхронного ожидания — использовать `ManualResetEventSlim` (см. выше).

**Альтернатива:** `EditorApplication.update` — срабатывает каждый кадр. Если нужна очередь команд, можно держать `ConcurrentQueue<Action>` и дренировать её в `EditorApplication.update`.

---

## 4. Runtime Bridge — передача состояния из Play Mode в Editor

### Проблема

Editor-скрипты (`[InitializeOnLoad]`) и Runtime-код (MonoBehaviour) работают в разных доменах C#, но **в одном процессе Unity**. Они не могут обмениваться объектами напрямую — только через статические классы, доступные в обеих частях.

### Решение: статический посредник с `UNITY_EDITOR` guard

```csharp
// Packages/com.shtl.mcp-unity/Runtime/RuntimeBridgeProxy.cs
// Компилируется во ВСЕ платформы (включая WebGL), но пишет только в Editor
using System;

namespace McpUnity.Runtime
{
    /// <summary>
    /// Промежуточный буфер для передачи игрового состояния из Runtime в Editor-мост.
    /// В релизных сборках — заглушки (методы ничего не делают).
    /// </summary>
    public static class RuntimeBridgeProxy
    {
        private static string _latestStateJson;
        private static readonly object _lock = new object();

        /// <summary>
        /// Вызывается из игрового кода (MonoBehaviour) в Play Mode.
        /// Пишет актуальное состояние в буфер.
        /// </summary>
        public static void PushState(GameState state)
        {
#if UNITY_EDITOR
            var json = UnityEngine.JsonUtility.ToJson(state);
            lock (_lock)
            {
                _latestStateJson = json;
            }
#endif
        }

        /// <summary>
        /// Вызывается из Editor-скрипта (McpUnityBridge) для чтения буфера.
        /// Возвращает null, если Play Mode не активен или данные ещё не поступали.
        /// </summary>
        public static string GetLatestState()
        {
#if UNITY_EDITOR
            lock (_lock)
            {
                return _latestStateJson;
            }
#else
            return null;
#endif
        }

        /// <summary>
        /// Сбрасывает буфер при выходе из Play Mode.
        /// </summary>
        public static void ClearState()
        {
#if UNITY_EDITOR
            lock (_lock)
            {
                _latestStateJson = null;
            }
#endif
        }
    }

    [Serializable]
    public class GameState
    {
        public int score;
        public int wave;
        public int lives;
        public bool isPlaying;
        public int asteroidCount;
    }
}
```

```csharp
// Packages/com.shtl.mcp-unity/Runtime/GameStatePusher.cs
// MonoBehaviour, добавляемый на один из объектов сцены (или через [RuntimeInitializeOnLoadMethod])
using UnityEngine;

namespace McpUnity.Runtime
{
    public class GameStatePusher : MonoBehaviour
    {
        [SerializeField] private float _pushIntervalSeconds = 0.5f;

        private float _timer;

        // Ссылки на данные игры — заполняются из ApplicationEntry или через инспектор
        public int Score;
        public int Wave;
        public int Lives;
        public int AsteroidCount;

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < _pushIntervalSeconds)
            {
                return;
            }
            _timer = 0f;

            RuntimeBridgeProxy.PushState(new GameState
            {
                score = Score,
                wave = Wave,
                lives = Lives,
                isPlaying = true,
                asteroidCount = AsteroidCount,
            });
        }

        private void OnDestroy()
        {
            RuntimeBridgeProxy.ClearState();
        }
    }
}
```

**Альтернативный паттерн — `[RuntimeInitializeOnLoadMethod]`:**

Если не хочется добавлять MonoBehaviour вручную, можно использовать атрибут, который вызывается Unity автоматически при запуске Play Mode:

```csharp
using UnityEngine;

namespace McpUnity.Runtime
{
    public static class RuntimeBridgeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
#if UNITY_EDITOR
            // Создаём pusher автоматически в любой сцене
            var go = new GameObject("[McpUnityBridge]");
            go.AddComponent<GameStatePusher>();
            Object.DontDestroyOnLoad(go);
#endif
        }
    }
}
```

### Очистка состояния при выходе из Play Mode

```csharp
// В McpUnityBridge добавить в static constructor:
EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

private static void OnPlayModeStateChanged(PlayModeStateChange state)
{
    if (state == PlayModeStateChange.ExitingPlayMode)
    {
        RuntimeBridgeProxy.ClearState();
    }
}
```

---

## 5. Структура встроенного UPM-пакета

### Директорийный layout

```
Packages/
└── com.shtl.mcp-unity/
    ├── package.json                       # UPM manifest
    ├── CHANGELOG.md                       # опционально
    │
    ├── Editor/                            # C#-скрипты только для Editor
    │   ├── McpUnityBridge.cs              # HttpListener + InitializeOnLoad
    │   └── McpUnity.Editor.asmdef        # Editor-only assembly definition
    │
    ├── Runtime/                           # C#-скрипты для Runtime (включая WebGL)
    │   ├── RuntimeBridgeProxy.cs          # Статический буфер состояния
    │   ├── RuntimeBridgeBootstrap.cs      # RuntimeInitializeOnLoadMethod
    │   ├── GameStatePusher.cs             # MonoBehaviour-компонент
    │   └── McpUnity.Runtime.asmdef        # Runtime assembly definition
    │
    └── Editor~/                           # Папка ИГНОРИРУЕТСЯ Unity (тильда в имени)
        └── Server/                        # TypeScript MCP-сервер
            ├── package.json               # npm-манифест сервера
            ├── tsconfig.json
            ├── src/
            │   ├── index.ts               # Точка входа MCP-сервера
            │   └── bridge.ts              # callUnityBridge + типы
            ├── dist/                      # Скомпилированный JS (gitignored или включён)
            │   └── index.js
            └── node_modules/              # npm-зависимости (gitignored)
```

### `package.json` Unity-пакета (UPM manifest)

```json
{
  "name": "com.shtl.mcp-unity",
  "version": "1.0.0",
  "displayName": "MCP Unity Bridge",
  "description": "TypeScript MCP-сервер и Editor HTTP-мост для управления Unity-проектом через Claude Code",
  "unity": "2022.3",
  "unityRelease": "0f1",
  "author": {
    "name": "SelStrom",
    "url": "https://github.com/SelStrom"
  },
  "keywords": [
    "mcp",
    "editor-tooling",
    "claude"
  ],
  "dependencies": {}
}
```

**Обязательные поля для Unity 2022:** `name`, `version`, `displayName`, `unity`. Поле `unityRelease` опционально, но помогает при публикации.

### Assembly Definition: Editor-only

```json
// Editor/McpUnity.Editor.asmdef
{
  "name": "McpUnity.Editor",
  "rootNamespace": "McpUnity.Editor",
  "references": [
    "GUID:..."
  ],
  "includePlatforms": [
    "Editor"
  ],
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

Ключевое: `"includePlatforms": ["Editor"]` — гарантирует, что код не компилируется в WebGL и другие Runtime-цели.

### Assembly Definition: Runtime (включая WebGL)

```json
// Runtime/McpUnity.Runtime.asmdef
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

`"includePlatforms": []` означает все платформы. `#if UNITY_EDITOR` в коде `RuntimeBridgeProxy` исключает Editor-специфичный код из WebGL-сборки.

### Регистрация пакета в проекте

В `Packages/manifest.json` проекта добавить:

```json
{
  "dependencies": {
    "com.shtl.mcp-unity": "file:../Packages/com.shtl.mcp-unity"
  }
}
```

Или, если пакет находится прямо внутри `Packages/` (embedded package — по умолчанию Unity подхватывает его автоматически без записи в manifest):

```
Packages/com.shtl.mcp-unity/package.json   ← Unity автоматически включает embedded packages
```

Embedded-пакеты (прямо в директории `Packages/`) Unity обнаруживает автоматически без записи в `manifest.json`. Это самый простой вариант.

---

## 6. `mcp.json` — регистрация сервера в Claude Code

```json
{
  "mcpServers": {
    "unity": {
      "command": "node",
      "args": [
        "Packages/com.shtl.mcp-unity/Editor~/Server/dist/index.js"
      ],
      "cwd": "/path/to/unity/project"
    }
  }
}
```

Или с использованием `npx` для автоматической сборки:

```json
{
  "mcpServers": {
    "unity": {
      "command": "node",
      "args": [
        "${workspaceFolder}/Packages/com.shtl.mcp-unity/Editor~/Server/dist/index.js"
      ]
    }
  }
}
```

**Важно:** `cwd` должен указывать на корень Unity-проекта. Путь в `args` — относительный или абсолютный к `dist/index.js`.

Альтернативно — скрипт-обёртка `mcp-unity.sh` в корне проекта:

```bash
#!/usr/bin/env bash
# Собирает TypeScript если dist устарел, затем запускает сервер
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SERVER_DIR="$SCRIPT_DIR/Packages/com.shtl.mcp-unity/Editor~/Server"

if [ ! -f "$SERVER_DIR/dist/index.js" ]; then
  cd "$SERVER_DIR" && npm ci && npm run build
fi

exec node "$SERVER_DIR/dist/index.js"
```

---

## 7. Выбор порта и избежание конфликтов

**Рекомендуемый порт: 8765**

| Порт | Статус | Примечание |
|------|--------|------------|
| 8080 | Занят часто | Webpack dev server, различные прокси |
| 8000 | Занят часто | Python HTTP-серверы, Django dev |
| 3000 | Занят часто | Node.js dev servers, Create React App |
| 4200 | Занят часто | Angular CLI |
| 8765 | Свободен | Нет стандартных сервисов, используется Unity Remote |
| 7777 | Используется Unity | Unity Network manager default |
| 9876 | Используется Unity | Unity Test Runner |

Порт 8765 — официальный порт Unity Remote (подключение мобильных устройств к Editor). Конфликт возможен только при одновременном использовании Unity Remote. Если это проблема — использовать 8766.

**Конфигурируемость:** Порт лучше вынести в `EditorPrefs` или константу в отдельном файле:

```csharp
// Editor/McpUnityBridgeConfig.cs
namespace McpUnity.Editor
{
    public static class McpUnityBridgeConfig
    {
        public const int Port = 8765;
        public const string BaseUrl = "http://localhost:" + Port + "/";
    }
}
```

---

## 8. Критические нюансы и ловушки

### WebGL build exclusion

`HttpListener` из `System.Net` **недоступен в WebGL**. Это ключевая причина, по которой весь bridge-код должен находиться либо:
1. В `Editor/` с `asmdef` `"includePlatforms": ["Editor"]` — тогда он вообще не компилируется в WebGL.
2. Или обёрнут в `#if UNITY_EDITOR ... #endif`.

`RuntimeBridgeProxy` в `Runtime/` должен использовать `#if UNITY_EDITOR` для всей логики, оставляя только пустые заглушки для WebGL-сборки.

### `AppDomain.DomainUnload` vs `AssemblyReloadEvents`

В Unity 2022 при перекомпиляции скриптов происходит **перезагрузка домена**. `AppDomain.DomainUnload` срабатывает и останавливает `HttpListener` — это правильное поведение. После перекомпиляции `[InitializeOnLoad]` вызывается снова и перезапускает мост.

Альтернатива (более явная):

```csharp
[InitializeOnLoad]
public static class McpUnityBridge
{
    static McpUnityBridge()
    {
        StartBridge();
        AssemblyReloadEvents.beforeAssemblyReload += StopBridge;
    }
}
```

`AssemblyReloadEvents` доступен в Unity 2019+.

### Newtonsoft JSON vs JsonUtility

Проект уже имеет `com.unity.nuget.newtonsoft-json` 3.2.2 как транзитивную зависимость `com.shtl.mvvm`. Можно использовать в Editor-коде. Для Runtime-кода (`RuntimeBridgeProxy`) предпочтительнее `JsonUtility` — он работает в WebGL без дополнительных зависимостей.

### Потокобезопасность `EditorApplication.delayCall`

`EditorApplication.delayCall` — delegate, не thread-safe. Добавлять из фонового потока безопасно (Unity гарантирует это), но срабатывает он на main thread в следующем кадре Editor'а. **Если кадр Editor'а не тикает** (Editor в фокусе не находится, `Application.runInBackground` = false), `delayCall` не сработает до следующего взаимодействия пользователя. Для MCP-сервера это редко критично, но стоит знать.

### Порядок инициализации: `[InitializeOnLoad]` + Play Mode

`[InitializeOnLoad]` срабатывает:
1. При загрузке Editor (запуск Unity)
2. После перекомпиляции скриптов
3. После выхода из Play Mode (перезагрузка домена, если `reloadDomain: true`)

Если `reloadDomain: false` в настройках Editor (Project Settings → Editor → Enter Play Mode Settings), домен не перезагружается при входе/выходе из Play Mode, и `[InitializeOnLoad]` при входе в Play Mode не вызывается повторно. В этом случае `HttpListener` продолжает работать без перерыва.

### `Content-Length` обязателен для `HttpListener`

Без явного `resp.ContentLength64 = bytes.Length` некоторые HTTP-клиенты (включая Node.js `fetch`) могут зависнуть, ожидая конец тела ответа. Всегда устанавливать `ContentLength64` перед записью.

---

## 9. Полный JSON schema для HTTP endpoint'ов

### Запросы (TypeScript → Unity)

```
POST /play         Body: {}
POST /stop         Body: {}
POST /compile      Body: {}
POST /list-scenes  Body: {}
POST /open-scene   Body: { "path": "Assets/Scenes/Main.unity" }
POST /import-asset Body: { "path": "Assets/Media/sprites/atlas.png" }
POST /game-state   Body: {}
```

### Ответы (Unity → TypeScript)

**Успех:**
```json
{ "ok": true, "result": "Play Mode запущен" }
{ "ok": true, "result": ["Assets/Scenes/Main.unity"] }
{ "ok": true, "result": { "score": 1500, "wave": 2, "lives": 3, "isPlaying": true, "asteroidCount": 4 } }
```

**Ошибка:**
```json
{ "ok": false, "error": "Сцена не найдена: Assets/Scenes/Test.unity" }
```

HTTP status code: 200 при `ok: true`, 4xx/5xx при `ok: false` (можно использовать 500 для всех ошибок — TypeScript-клиент проверяет поле `ok`).

---

## 10. Итоговый deployment checklist

```
1. Создать Packages/com.shtl.mcp-unity/ со структурой выше
2. Написать McpUnityBridge.cs в Editor/ с [InitializeOnLoad]
3. Написать RuntimeBridgeProxy.cs + GameStatePusher.cs в Runtime/
4. Создать asmdef для Editor/ и Runtime/
5. В Editor~/Server/ создать TypeScript-проект
6. npm install @modelcontextprotocol/sdk
7. Написать src/index.ts + src/bridge.ts
8. npm run build → dist/index.js
9. Добавить mcp.json в корень Unity-проекта
10. Зарегистрировать GameStatePusher в сцене (или через RuntimeInitializeOnLoadMethod)
11. Открыть Unity — убедиться в логе "[McpUnityBridge] HTTP-мост запущен на порту 8765"
12. Claude Code: добавить mcp.json в настройки → перезапустить
```

---

## Confidence Assessment

| Область | Уровень | Обоснование |
|---------|---------|------------|
| `@modelcontextprotocol/sdk` API | MEDIUM | Знание из обучающей выборки (август 2025); SDK активно развивался — проверить npm перед реализацией |
| Unity `HttpListener` в Editor | HIGH | Стабильное API, присутствует с Unity 5; не менялось в Unity 2022 |
| `[InitializeOnLoad]`, `EditorApplication` | HIGH | Core Editor API, стабильное и хорошо задокументированное |
| `AssemblyReloadEvents` | HIGH | Доступен с Unity 2019, стабилен в 2022 |
| UPM embedded package структура | HIGH | Официальная документация, стабильный формат `package.json` |
| `Editor~/` конвенция | HIGH | Официальная Unity документация по пакетам |
| `RuntimeInitializeOnLoadMethod` | HIGH | Стандартное Unity API |
| TypeScript ESM + Node16 | MEDIUM | Рекомендуемый паттерн на момент среза знаний; проверить с актуальной версией Node |
