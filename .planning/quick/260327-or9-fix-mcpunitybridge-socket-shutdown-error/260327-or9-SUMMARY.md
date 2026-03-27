---
quick_id: 260327-or9
status: complete
date: 2026-03-27
---

# Summary: Fix McpUnityBridge socket-shutdown error

## What was done

Fixed "The socket has been shut down" error that appeared when calling MCP tools (list_scenes etc.).

## Changes

**McpUnityBridge.cs:**
1. `ListenLoop` → `delayCall` lambda now checks `_running` before calling `HandleRequest`. If server was stopped (domain reload), calls `context.Response.Abort()` instead.
2. `SendJsonRaw` now catches `SocketException`, `ObjectDisposedException`, `IOException` silently — these are expected when the client disconnects before the response is written.

## Result

MCP tool calls no longer produce error noise in Unity Console when the connection closes naturally. Server remains stable across domain reloads.
