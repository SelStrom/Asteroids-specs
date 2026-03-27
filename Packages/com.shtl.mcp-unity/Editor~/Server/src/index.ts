import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { z } from "zod";

const server = new McpServer({ name: "mcp-unity", version: "0.1.0" });

const BRIDGE_URL = "http://localhost:8765";

// Вспомогательная функция для вызова bridge с обработкой ошибок
async function callBridge(endpoint: string, body?: object, timeoutMs = 15000): Promise<string> {
  try {
    const response = await fetch(`${BRIDGE_URL}${endpoint}`, {
      method: "POST",
      headers: body ? { "Content-Type": "application/json" } : {},
      body: body ? JSON.stringify(body) : undefined,
      signal: AbortSignal.timeout(timeoutMs),
    });
    const data = await response.json();
    return JSON.stringify(data);
  } catch (err) {
    if (err instanceof Error && err.name === "TimeoutError") {
      return JSON.stringify({ success: false, message: "Таймаут при подключении к Unity. Убедитесь, что Unity открыт с проектом и MCP-мост активен." });
    }
    const msg = err instanceof Error ? err.message : String(err);
    return JSON.stringify({ success: false, message: `Ошибка соединения с Unity bridge: ${msg}` });
  }
}

// Инструмент compile — компиляция скриптов Unity через AssetDatabase.Refresh()
server.tool("compile", "Compile Unity scripts via AssetDatabase.Refresh() and return compilation status", {}, async () => {
  const text = await callBridge("/compile", undefined, 30000);
  return { content: [{ type: "text", text }] };
});

// Инструмент play — вход в Play Mode (EditorApplication.isPlaying = true)
server.tool("play", "Enter Unity Editor Play Mode (EditorApplication.isPlaying = true)", {}, async () => {
  const text = await callBridge("/play");
  return { content: [{ type: "text", text }] };
});

// Инструмент stop — выход из Play Mode (EditorApplication.isPlaying = false)
server.tool("stop", "Exit Unity Editor Play Mode (EditorApplication.isPlaying = false)", {}, async () => {
  const text = await callBridge("/stop");
  return { content: [{ type: "text", text }] };
});

// Инструмент list_scenes — список всех Unity-сцен через AssetDatabase
server.tool("list_scenes", "List all Unity scene files (.unity) in the project via AssetDatabase", {}, async () => {
  const text = await callBridge("/list_scenes");
  return { content: [{ type: "text", text }] };
});

// Инструмент open_scene — открытие сцены по пути
server.tool(
  "open_scene",
  "Open a Unity scene by asset path (e.g. Assets/Scenes/Main.unity)",
  { path: z.string().describe("Scene asset path, e.g. Assets/Scenes/Main.unity") },
  async ({ path }) => {
    const text = await callBridge("/open_scene", { path }, 30000);
    return { content: [{ type: "text", text }] };
  }
);

// Инструмент import_asset — принудительный реимпорт ассета через AssetDatabase.ImportAsset
server.tool(
  "import_asset",
  "Force reimport an asset via AssetDatabase.ImportAsset(path)",
  { path: z.string().describe("Asset path relative to project root, e.g. Assets/Textures/sprite.png") },
  async ({ path }) => {
    const text = await callBridge("/import_asset", { path }, 30000);
    return { content: [{ type: "text", text }] };
  }
);

// Подключение stdio-транспорта
const transport = new StdioServerTransport();
await server.connect(transport);
