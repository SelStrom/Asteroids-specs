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
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body ?? {}),
      signal: AbortSignal.timeout(timeoutMs),
    });
    if (!response.ok) {
      return JSON.stringify({ success: false, message: `HTTP ${response.status} ${response.statusText}` });
    }
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

// Инструмент run_menu_item — выполнение пункта меню Unity Editor
server.tool(
  "run_menu_item",
  "Execute a Unity Editor menu item by its full path (e.g. 'Asteroids/Setup Phase 3 Assets')",
  { menu_path: z.string().describe("Full menu path, e.g. 'Asteroids/Setup Phase 3 Assets' or 'GameObject/Create Empty'") },
  async ({ menu_path }) => {
    const text = await callBridge("/run_menu_item", { menu_path }, 30000);
    return { content: [{ type: "text", text }] };
  }
);

// Инструмент find_assets — поиск ассетов по типу и/или имени
server.tool(
  "find_assets",
  "Find Unity assets by type and optional name filter using AssetDatabase.FindAssets. Returns array of asset paths.",
  {
    type: z.string().optional().describe("Asset type filter, e.g. 'Prefab', 'ScriptableObject', 'Texture2D', 'Sprite'"),
    name: z.string().optional().describe("Optional name filter (partial match supported)"),
  },
  async ({ type, name }) => {
    const text = await callBridge("/find_assets", { type: type ?? "", name: name ?? "" });
    return { content: [{ type: "text", text }] };
  }
);

// Инструмент set_asset_field — назначение объектной ссылки на поле ScriptableObject/Prefab
server.tool(
  "set_asset_field",
  "Set a serialized object-reference field on a Unity asset (ScriptableObject or Prefab) using SerializedObject API. Saves the asset automatically.",
  {
    asset_path: z.string().describe("Path to target asset, e.g. 'Assets/Media/configs/GameData.asset'"),
    field_path: z.string().describe("Serialized property path using dot notation, e.g. 'Ship.Prefab' or 'Bullet.EnemyPrefab'"),
    value_asset_path: z.string().describe("Path to the asset to assign, e.g. 'Assets/Media/prefabs/ship.prefab'. Empty string sets null."),
    value_asset_name: z.string().optional().describe("Sub-asset name for sprite sheets or multi-object assets, e.g. 'ship_0' for a sprite within a PNG"),
  },
  async ({ asset_path, field_path, value_asset_path, value_asset_name }) => {
    const text = await callBridge("/set_asset_field", { asset_path, field_path, value_asset_path, value_asset_name: value_asset_name ?? "" }, 15000);
    return { content: [{ type: "text", text }] };
  }
);

// Инструмент set_scene_object_field — назначение поля на компоненте GameObject в открытой сцене
server.tool(
  "set_scene_object_field",
  "Set a serialized object-reference field on a component of a scene GameObject using SerializedObject API. Saves the scene automatically.",
  {
    object_name: z.string().describe("GameObject name in the active scene, e.g. 'ApplicationEntry'"),
    component_type: z.string().describe("Component class name, e.g. 'ApplicationEntry' or 'Camera'"),
    field_path: z.string().describe("Serialized property path, e.g. '_configs' or '_hudVisual'"),
    value_asset_path: z.string().describe("Asset path to assign, e.g. 'Assets/Media/configs/GameData.asset'. Empty string sets null."),
    value_asset_name: z.string().optional().describe("Sub-asset name for sprite sheets or multi-object assets"),
  },
  async ({ object_name, component_type, field_path, value_asset_path, value_asset_name }) => {
    const text = await callBridge("/set_scene_object_field", { object_name, component_type, field_path, value_asset_path, value_asset_name: value_asset_name ?? "" }, 15000);
    return { content: [{ type: "text", text }] };
  }
);

// Инструмент get_game_state — получить живое состояние игры из Runtime (MCP-07)
server.tool(
  "get_game_state",
  "Get current game state: score, wave, lives, isPlaying. Returns { score, wave, lives, isPlaying }. Works only in Play Mode; returns isPlaying:false otherwise.",
  {},
  async () => {
    const text = await callBridge("/get_game_state");
    return { content: [{ type: "text", text }] };
  }
);

// Подключение stdio-транспорта
const transport = new StdioServerTransport();
await server.connect(transport);
