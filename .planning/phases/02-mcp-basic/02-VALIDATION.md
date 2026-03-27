---
phase: 2
slug: mcp-basic
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-27
---

# Phase 2 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | Manual / smoke-test (нет автоматизированных тестов для Unity Editor Extensions) |
| **Config file** | none |
| **Quick run command** | `cd Packages/com.shtl.mcp-unity/Editor~/Server && npm run build` |
| **Full suite command** | `npm run build && node dist/index.js < /dev/null` + ручной тест всех tools в Unity |
| **Estimated runtime** | ~10 seconds (build) + ручное тестирование |

---

## Sampling Rate

- **After every task commit:** Run `cd Packages/com.shtl.mcp-unity/Editor~/Server && npm run build`
- **After every plan wave:** Run `npm run build && node dist/index.js < /dev/null`
- **Before `/gsd:verify-work`:** Full manual test всех 6 MCP tools через Claude Code
- **Max feedback latency:** ~10 seconds (TypeScript build)

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 2-01-01 | 01 | 0 | MCP-01 | smoke | `ls Packages/com.shtl.mcp-unity/Editor/ && ls Packages/com.shtl.mcp-unity/Runtime/` | ❌ W0 | ⬜ pending |
| 2-01-02 | 01 | 1 | MCP-01 | smoke | `npm run build` exits 0 | ❌ W0 | ⬜ pending |
| 2-01-03 | 01 | 1 | MCP-02 | smoke | `node dist/index.js < /dev/null` starts without error | ❌ W0 | ⬜ pending |
| 2-02-01 | 02 | 2 | MCP-03 | manual | Unity Editor: HttpListener на localhost:8765 | ❌ requires Unity | ⬜ pending |
| 2-02-02 | 02 | 2 | MCP-04 | manual | `play` / `stop` переключают Play Mode | ❌ requires Unity | ⬜ pending |
| 2-03-01 | 03 | 3 | MCP-05 | manual | `list_scenes` возвращает список сцен | ❌ requires Unity | ⬜ pending |
| 2-03-02 | 03 | 3 | MCP-06 | manual | `open_scene` открывает сцену | ❌ requires Unity | ⬜ pending |
| 2-03-03 | 03 | 3 | MCP-08 | manual | `import_asset` вызывает AssetDatabase.ImportAsset | ❌ requires Unity | ⬜ pending |
| 2-04-01 | 04 | 4 | MCP-09 | smoke | `cat .mcp.json \| python3 -m json.tool` | ❌ W0 | ⬜ pending |
| 2-04-02 | 04 | 4 | MCP-10 | manual | `claude mcp list` показывает mcp-unity | ❌ requires Claude Code | ⬜ pending |
| 2-04-03 | 04 | 4 | MCP-12 | manual | `compile` возвращает errors[] с file/line/column/message/severity | ❌ requires Unity | ⬜ pending |
| 2-04-04 | 04 | 4 | MCP-13 | manual | Сервер подключается < 5 секунд после запуска | ❌ requires Claude Code | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `Packages/com.shtl.mcp-unity/Editor~/Server/package.json` — npm конфигурация с `"build": "tsc"`
- [ ] `Packages/com.shtl.mcp-unity/Editor~/Server/tsconfig.json` — TypeScript конфигурация
- [ ] `Packages/com.shtl.mcp-unity/Editor~/Server/src/index.ts` — entry point (stub)
- [ ] `Packages/com.shtl.mcp-unity/Editor/McpUnityBridge.cs` — C# bridge (stub)
- [ ] `.mcp.json` — MCP config at project root

*Wave 0 создаёт минимальные стабы для smoke-проверки `npm run build` с первых задач.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| HttpListener стартует на 8765 | MCP-02 | Требует Unity Editor | Открыть Unity, проверить Console на сообщение "McpUnityBridge started" |
| `compile` вызывает AssetDatabase.Refresh | MCP-03 | Требует Unity Editor | Вызвать tool `compile`, убедиться в Console что произошёл reimport |
| `play` / `stop` переключают Play Mode | MCP-04 | Требует Unity Editor | Вызвать `play`, Unity должен войти в Play Mode; вызвать `stop` — выйти |
| `list_scenes` возвращает сцены | MCP-05 | Требует Unity Editor | Вызвать tool, убедиться что `Assets/Scenes/Main.unity` в ответе |
| `open_scene` открывает сцену | MCP-06 | Требует Unity Editor | Вызвать tool с путём, Unity должен переключить активную сцену |
| `import_asset` импортирует ассет | MCP-08 | Требует Unity Editor | Вызвать tool с путём к существующему ассету |
| Сервер в `claude mcp list` | MCP-09/10 | Требует Claude Code | `claude mcp list` должен показывать "mcp-unity" |
| Compile errors с деталями | MCP-12 | Требует Unity с ошибками | Намеренно создать C# ошибку, проверить что `compile` возвращает file/line/column |
| Старт < 5 секунд | MCP-13 | Требует Claude Code | Замерить время от `claude mcp list` до первого успешного tool call |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 10s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
