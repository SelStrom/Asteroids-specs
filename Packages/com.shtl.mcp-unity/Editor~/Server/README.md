# MCP Unity Server

TypeScript MCP-сервер для интеграции Unity Editor с Claude Code.

## Установка

1. Перейти в директорию сервера:
   ```bash
   cd Packages/com.shtl.mcp-unity/Editor~/Server
   ```

2. Установить зависимости:
   ```bash
   npm install
   ```

3. Скомпилировать TypeScript:
   ```bash
   npm run build
   ```

## Регистрация в Claude Code

Файл `.mcp.json` уже создан в корне проекта. Проверить:
```bash
claude mcp list
```

В списке должен появиться `mcp-unity`.

## Инструменты (MCP Tools)

| Tool | Описание |
|------|---------|
| `compile` | AssetDatabase.Refresh() + статус компиляции |
| `play` | Войти в Play Mode |
| `stop` | Выйти из Play Mode |
| `list_scenes` | Список .unity файлов в проекте |
| `open_scene` | Открыть сцену по пути (path: string) |
| `import_asset` | AssetDatabase.ImportAsset(path) |

## Требования

- Node.js 18+ (встроенный fetch)
- Unity 2022.3 LTS с открытым Editor
- C# bridge `McpUnityBridge.cs` компилируется при открытии Unity

## Bridge

C# часть (`Editor/McpUnityBridge.cs`) стартует автоматически при загрузке Unity Editor на `localhost:8765`. TypeScript-сервер проксирует MCP-вызовы к bridge по HTTP.
