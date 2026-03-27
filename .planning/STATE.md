---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: milestone
status: executing
last_updated: "2026-03-27T18:35:41.920Z"
progress:
  total_phases: 8
  completed_phases: 2
  total_plans: 10
  completed_plans: 7
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-03-27)

**Core value:** Играбельный Asteroids в браузере, точно воспроизводящий геймплей оригинала
**Current focus:** Phase 03 — core-mechanics

## Current Phase

**Phase:** 3
**Status:** Executing Phase 03
**Next action:** /gsd:plan-phase 03

## Decisions

- webGLCompressionFormat=0: отключена компрессия WebGL для упрощения деплоя на этапе разработки
- webGLMemoryGrowthMode=2: Memory Growth для WebGL включён
- com.shtl.mvvm подключён через git hash c7bda1c для воспроизводимости
- m_LayerCollisionMatrix: открытая матрица — финальная настройка через Unity Editor
- Embedded UPM пакет не требует записи в manifest.json — Unity распознаёт автоматически по Packages/{name}/package.json
- AsteroidsEditor.asmdef: autoReferenced=false, includePlatforms=[Editor] — Editor-сборки не попадают в build
- [Phase 01-project-foundation]: GameAtlas.spriteAtlas: enableRotation=0, padding=4, generateMipMaps=0, maxTextureSize=2048, WebGL RGBA32
- [Phase 01-project-foundation]: Sprite Atlas пустой (m_PackedSprites=[]) — PNG предоставит пользователь, атлас принимает спрайты после импорта
- [Phase 03-core-mechanics]: Sprite Atlas не используется — PNG нарезан на sub-sprites, prefabs ссылаются на них напрямую (D-01)
- [Phase 03-core-mechanics]: DATA_SCHEMA имеет приоритет над REQUIREMENTS для числовых значений (MaxShoots=5, BulletLifetime=2s)
- [Phase 03-core-mechanics]: Лазер реализуется полностью в Phase 3; VfxBlowPrefab (взрыв корабля) — backlog Phase 6
- [Phase 02-mcp-basic]: McpUnityBridge: ручная JSON-сериализация через StringBuilder — JsonUtility не поддерживает анонимные типы
- [Phase 02-mcp-basic]: McpUnityBridge: EditorApplication.delayCall dispatching всех Unity API вызовов из фонового Thread

## Milestone

**v1.0 — Complete Game**

- 8 phases total
- 68 v1 requirements

## Notes

- Sprite atlas будет предоставлен пользователем — необходим для Phase 1 (SETUP-05)
- UGS Project ID нужно создать в Unity Dashboard перед Phase 7
- MCP Basic (Phase 2) разблокирует работу с Unity через Claude Code

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260327-or9 | Fix McpUnityBridge socket-shutdown error on list_scenes | 2026-03-27 | c12d64e | [260327-or9-fix-mcpunitybridge-socket-shutdown-error](./quick/260327-or9-fix-mcpunitybridge-socket-shutdown-error/) |
| 260327-p00 | Increase MCP server timeouts and add error handling | 2026-03-27 | 1a096f6 | [260327-p00-increase-mcp-server-timeouts-and-add-err](./quick/260327-p00-increase-mcp-server-timeouts-and-add-err/) | [260327-or9-fix-mcpunitybridge-socket-shutdown-error](./quick/260327-or9-fix-mcpunitybridge-socket-shutdown-error/) |

---
*State initialized: 2026-03-27*
