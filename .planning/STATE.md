---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: milestone
status: executing
last_updated: "2026-03-28T00:40:55.457Z"
progress:
  total_phases: 8
  completed_phases: 4
  total_plans: 14
  completed_plans: 14
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-03-27)

**Core value:** Играбельный Asteroids в браузере, точно воспроизводящий геймплей оригинала
**Current focus:** Phase 04 — asteroids-progression

## Current Phase

**Phase:** 3
**Status:** Executing Phase 04
**Next action:** Continue Phase 04 — Plan 03

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
- [Phase 03-core-mechanics]: MoveToSystem/ShootToSystem регистрируются как заглушки в Model.cs — компилируются, но пусты до Phase 4
- [Phase 03-core-mechanics]: ThrustSystem TNode — кортеж (ThrustComponent, MoveComponent, RotateComponent) без wrapper класса
- [Phase 04]: AsteroidData ScriptableObject — ссылочное сравнение data == _configs.AsteroidBig корректно
- [Phase 04-02]: _model.GameArea используется вместо GetSystem<MoveSystem>().GameArea — MoveSystem не имеет публичного геттера
- [Phase 04-02]: onScoreChanged параметр Connect() опциональный — обратная совместимость с Application.cs
- [Phase 04-02]: EntitiesCatalog.Reset() очищает только словари, не трогает pool/prefabRegistry
- [Phase 04]: Game.Connect перенесён в OnGameStart() — чтобы onScoreChanged callback передавался при каждом запуске включая Restart
- [Phase 04]: AsteroidSmall использует medium-спрайты — PNG не содержит small-вариантов

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
| 260327-p00 | Increase MCP server timeouts and add error handling | 2026-03-27 | 1a096f6 | [260327-p00-increase-mcp-server-timeouts-and-add-err](./quick/260327-p00-increase-mcp-server-timeouts-and-add-err/) |
| 260328-3p1 | Fix GameOverScreen duplication and NullReference in GunSystem | 2026-03-28 | e84c711 | [260328-3p1-fix-gameoverscreen-duplication-and-nullr](./quick/260328-3p1-fix-gameoverscreen-duplication-and-nullr/) |
| 260328-jmk | Fix sprite PPU 100→16 and asteroid splitting bug (bullet-only) | 2026-03-28 | b9a45ff | [260328-jmk-fix-sprite-pixels-per-unit-to-16-and-ast](./quick/260328-jmk-fix-sprite-pixels-per-unit-to-16-and-ast/) |
| 260328-jv0 | Rescale world for PPU=16: colliders ×6.25, Canvas Overlay, HUD pivot fix | 2026-03-28 | 6b3d110 | [260328-jv0-rescale-world-for-ppu-16-fix-prefab-scal](./quick/260328-jv0-rescale-world-for-ppu-16-fix-prefab-scal/) |
| 260328-kk4 | Fix ship sprite rotation: MoveComponent.Direction → ObservableValue, MVVM binding | 2026-03-28 | 714ea30 | [260328-kk4-fix-ship-sprite-rotation](./quick/260328-kk4-fix-ship-sprite-rotation/) |
| 260328-m7s | Fix gun shooting: cooldown timer after MaxShoots, CurrentShoots reset after ReloadDurationSec | 2026-03-28 | a574c0a | [260328-m7s-space-max-cooldown](./quick/260328-m7s-space-max-cooldown/) |
| 260328-me1 | Fix bullet sprite missing + small asteroids invisible | 2026-03-28 | a06b585 | [260328-me1-fix-small](./quick/260328-me1-fix-small/) |

---
*State initialized: 2026-03-27*
