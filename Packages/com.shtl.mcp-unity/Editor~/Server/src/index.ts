import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { z } from "zod";

const server = new McpServer({ name: "mcp-unity", version: "0.1.0" });

// Инструмент compile — компиляция скриптов Unity через AssetDatabase.Refresh()
server.tool("compile", "Compile Unity scripts via AssetDatabase.Refresh() and return compilation status", {}, async () => {
  const response = await fetch("http://localhost:8765/compile", {
    method: "POST",
    signal: AbortSignal.timeout(5000),
  });
  const data = await response.json();
  return { content: [{ type: "text", text: JSON.stringify(data) }] };
});

// Инструмент play — вход в Play Mode (EditorApplication.isPlaying = true)
server.tool("play", "Enter Unity Editor Play Mode (EditorApplication.isPlaying = true)", {}, async () => {
  const response = await fetch("http://localhost:8765/play", {
    method: "POST",
    signal: AbortSignal.timeout(5000),
  });
  const data = await response.json();
  return { content: [{ type: "text", text: JSON.stringify(data) }] };
});

// Инструмент stop — выход из Play Mode (EditorApplication.isPlaying = false)
server.tool("stop", "Exit Unity Editor Play Mode (EditorApplication.isPlaying = false)", {}, async () => {
  const response = await fetch("http://localhost:8765/stop", {
    method: "POST",
    signal: AbortSignal.timeout(5000),
  });
  const data = await response.json();
  return { content: [{ type: "text", text: JSON.stringify(data) }] };
});

// Инструмент list_scenes — список всех Unity-сцен через AssetDatabase
server.tool("list_scenes", "List all Unity scene files (.unity) in the project via AssetDatabase", {}, async () => {
  const response = await fetch("http://localhost:8765/list_scenes", {
    method: "POST",
    signal: AbortSignal.timeout(5000),
  });
  const data = await response.json();
  return { content: [{ type: "text", text: JSON.stringify(data) }] };
});

// Инструмент open_scene — открытие сцены по пути
server.tool(
  "open_scene",
  "Open a Unity scene by asset path (e.g. Assets/Scenes/Main.unity)",
  { path: z.string().describe("Scene asset path, e.g. Assets/Scenes/Main.unity") },
  async ({ path }) => {
    const response = await fetch("http://localhost:8765/open_scene", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ path }),
      signal: AbortSignal.timeout(5000),
    });
    const data = await response.json();
    return { content: [{ type: "text", text: JSON.stringify(data) }] };
  }
);

// Инструмент import_asset — принудительный реимпорт ассета через AssetDatabase.ImportAsset
server.tool(
  "import_asset",
  "Force reimport an asset via AssetDatabase.ImportAsset(path)",
  { path: z.string().describe("Asset path relative to project root, e.g. Assets/Textures/sprite.png") },
  async ({ path }) => {
    const response = await fetch("http://localhost:8765/import_asset", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ path }),
      signal: AbortSignal.timeout(5000),
    });
    const data = await response.json();
    return { content: [{ type: "text", text: JSON.stringify(data) }] };
  }
);

// Подключение stdio-транспорта
const transport = new StdioServerTransport();
await server.connect(transport);
