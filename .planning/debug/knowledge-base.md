# GSD Debug Knowledge Base

Resolved debug sessions. Used by `gsd-debugger` to surface known-pattern hypotheses at the start of new investigations.

---

## get-game-state-isplaying-false — get_game_state всегда возвращает isPlaying:false в Play Mode
- **Date:** 2026-03-29
- **Error patterns:** isPlaying false, get_game_state, Play Mode, RuntimeBridgeProxy, GetType null, Assembly-CSharp, McpUnityBridge
- **Root cause:** McpUnityBridge.HandleGetGameState использовала System.Type.GetType("SelStrom.Asteroids.RuntimeBridgeProxy, Asteroids") — метод не находил тип потому что сборка McpUnityBridge не имеет reference на сборку Asteroids. GetType всегда возвращал null, условие `type == null` срабатывало и возвращало isPlaying:false.
- **Fix:** (1) Убраны директивы #if UNITY_EDITOR из RuntimeBridgeProxy.cs. (2) В McpUnityBridge.cs System.Type.GetType заменён на перебор AppDomain.CurrentDomain.GetAssemblies(). (3) Логика изменена: isPlaying = EditorApplication.isPlaying (всегда), isRunning = RuntimeBridgeProxy.IsRunning. (4) TypeScript пересобран.
- **Files changed:** Assets/Scripts/Application/RuntimeBridgeProxy.cs, Packages/com.shtl.mcp-unity/Editor/McpUnityBridge.cs
---

