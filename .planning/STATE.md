---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: milestone
status: executing
last_updated: "2026-03-31T12:42:13.181Z"
progress:
  total_phases: 9
  completed_phases: 9
  total_plans: 33
  completed_plans: 33
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-03-27)

**Core value:** Играбельный Asteroids в браузере, точно воспроизводящий геймплей оригинала
**Current focus:** Phase 09 — webgl

## Current Phase

**Phase:** 8
**Status:** Executing Phase 09
**Next action:** Plan Phase 07 — UGS Leaderboards

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
- [Phase 04]: AsteroidSmall использует собственные small-спрайты (asteroid_small_1/2/3) — нарезаны вручную в asteroids.png, rect 32×32
- [Phase 05]: ShootToComponent использует using SelStrom.Asteroids для доступа к UfoModel — cross-namespace reference
- [Phase 05]: UfoVisual не содержит Update/rotation — UFO движется по прямой без вращения спрайта
- [Phase 05-ufo-progression-polish]: _ufoActive флаг + CheckUfoExit wrap-around detection для UFO-05
- [Phase 05-ufo-progression-polish]: Phase5Setup создаёт UFO assets через Editor script — не вручную
- [Phase 06]: AudioManager: отдельный AudioSource на каждый звук, null-guard в PlayOneShot, BeatLoop Lerp(0.25f, 1.0f, count/12f)
- [Phase 06-audio-visual-polish]: LeaderboardScreen наследует AbstractScreen, _leaderboardButton отключается в OnConnected View до Phase 7
- [Phase 06]: Audio callbacks передаются через Game.Connect() как опциональные Action параметры — Game не зависит от AudioManager напрямую
- [Phase 06]: AudioManager lifecycle: StartBeat() при старте, StopBeat()+StopAll() при GameOver, StopAll() при Restart — защита от зависших loop-звуков
- [Phase 06]: Phase6Setup следует конвенции Phase3/4/5: один MenuItem вызывает все методы настройки
- [Phase 06]: vfx_blow.prefab ParticleSystem.stopAction=Callback — критично для OnParticleSystemStopped возврата в пул
- [Phase 07-ugs-leaderboards]: UgsService: plain C# класс без MonoBehaviour по паттерну AudioManager (D-08, D-09)
- [Phase 07-ugs-leaderboards]: GameData.UgsProjectId — документационное поле, SDK читает ID из Project Settings (Pitfall 1, LEAD-06)
- [Phase 07-ugs-leaderboards]: IApplicationComponent расширен StartCoroutine — Application не MonoBehaviour, запускает Coroutine через entry
- [Phase 07-ugs-leaderboards]: InitUgsCoroutine использует прямой while без RunAsync — graceful обработка ошибок UGS без throw
- [Phase 07-ugs-leaderboards]: Phase7Setup следует конвенции Phase3/4/5/6: один MenuItem вызывает все методы настройки
- [Phase 08-mcp-runtime]: RuntimeBridgeProxy: #if UNITY_EDITOR guard без using UnityEditor — WebGL-safe статический буфер состояния
- [Phase 08-mcp-runtime]: McpUnityBridge: reflection через Assembly-CSharp для чтения RuntimeBridgeProxy — нет compile-time зависимости пакета от игрового кода
- [Phase 09-webgl]: GUID Main.unity (6d7de3ab382bf4928a9621158bb4fb3d) записан напрямую в EditorBuildSettings.asset — PhaseSetup не требуется для 09-webgl
- [Phase 09-webgl]: Файлы WebGL сборки названы WebGL.* (не Asteroids.*) — Unity использует имя папки вывода при сборке через диалог
- [Phase 09-webgl]: webGLCompressionFormat=2 (Disabled) — без сжатия для совместимости с GitHub Pages без CORS-заголовков
- [Phase 09-webgl]: EmbeddedJson в PlayerActions.cs — Input System на WebGL не читает файл из StreamingAssets, нужен embedded JSON

## Milestone

**v1.0 — Complete Game**

- 8 phases total
- 68 v1 requirements

## Accumulated Context

### Roadmap Evolution

- Phase 9 added: Добавить поддержку WebGL и настроить сборку под эту цель

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
| 260328-mxz | Assign small sprites (asteroid_small_1/2/3) to AsteroidSmallData.asset | 2026-03-28 | 71ad1e3 | [260328-mxz-small-asteroidsmalldata](./quick/260328-mxz-small-asteroidsmalldata/) |
| 260328-v8q | Fix TitleScreen double ASTEROIDS text and ParticleSystem noninit error | 2026-03-28 | 684dd33 | [260328-v8q-fix-titlescreen-double-asteroids-text-an](./quick/260328-v8q-fix-titlescreen-double-asteroids-text-an/) |
| 260328-xag | Fix vfx_blow effect not visible — Default-Particle material + startSize 0.8 | 2026-03-29 | 06382c7 | [260328-xag-fix-vfx-blow-effect-not-visible-at-all-p](./quick/260328-xag-fix-vfx-blow-effect-not-visible-at-all-p/) |

---

### Pending Todos

| File | Title | Area |
|------|-------|------|
| [2026-03-28-destroy-bullet-on-asteroid-ufo-collision.md](./todos/pending/2026-03-28-destroy-bullet-on-asteroid-ufo-collision.md) | Уничтожать пулю при коллайде с астероидом или НЛО | gameplay |

---
*State initialized: 2026-03-27*
