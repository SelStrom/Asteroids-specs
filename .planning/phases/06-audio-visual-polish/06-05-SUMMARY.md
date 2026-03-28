---
phase: 06-audio-visual-polish
plan: "05"
subsystem: editor-setup
tags: [unity, csharp, editor-script, audio, vfx, ui, leaderboard, camera]

# Dependency graph
requires:
  - phase: 06-audio-visual-polish
    plan: "04"
    provides: ApplicationEntry SerializedField AudioManager/LeaderboardView/leaderboardGo, GameData.AudioData поле
  - phase: 06-audio-visual-polish
    plan: "01"
    provides: EffectVisual компонент с ParticleSystem + OnParticleSystemStopped
  - phase: 06-audio-visual-polish
    plan: "03"
    provides: LeaderboardView + LeaderboardScreen + TitleScreenView с _leaderboardButton/_titleText
  - phase: 06-audio-visual-polish
    plan: "02"
    provides: AudioManager MonoBehaviour с 9 SerializedField AudioSource

provides:
  - Assets/Editor/Phase6Setup.cs: MenuItem "Asteroids/Setup Phase 6 Assets" с 7 методами настройки
  - Assets/Media/configs/AudioData.asset: ScriptableObject с 9 AudioClip полями (null до назначения пользователем)
  - Assets/Media/prefabs/vfx_blow.prefab: ParticleSystem(stopAction=Callback, burst=15) + EffectVisual
  - Assets/Scenes/Main.unity: AudioManager с 9 AudioSource, LeaderboardScreen, TitleScreen обновлён
  - GameData.asset: Audio=AudioData, VfxBlowPrefab=vfx_blow назначены
  - ApplicationEntry: _audioManager, _leaderboardView, _leaderboardGo назначены в Inspector

affects:
  - Play Mode: AudioManager готов принять AudioClip файлы от пользователя
  - Play Mode: vfx_blow.prefab активируется через EntitiesCatalog.SpawnEffect при взрывах
  - 07-leaderboard-integration: LeaderboardScreen готов к подключению реального API

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Phase6Setup по конвенции проекта: MenuItem + статические методы + SerializedObject + Resources.FindObjectsOfTypeAll"
    - "SaveOrReplacePrefab: delete existing + SaveAsPrefabAsset — предотвращает дублирование asset"
    - "AudioSource дочерние объекты: loop=true только для thrust/ufoTone, playOnAwake=false для всех"

key-files:
  created:
    - Assets/Editor/Phase6Setup.cs
    - Assets/Media/configs/AudioData.asset
    - Assets/Media/prefabs/vfx_blow.prefab
  modified:
    - Assets/Scenes/Main.unity
    - Assets/Media/configs/GameData.asset
    - Assets/Scripts/Application/Screens/LeaderboardScreen.cs

key-decisions:
  - "Phase6Setup создаёт все runtime assets через единый MenuItem — конвенция проекта Phase3-5"
  - "AudioClip поля AudioData.asset оставлены null: пользователь назначает .wav/.ogg через Inspector"
  - "vfx_blow.prefab ParticleSystem.stopAction=Callback: обязательно для OnParticleSystemStopped → пул возврат"

patterns-established:
  - "Editor setup скрипты создают сцену через EditorSceneManager.OpenScene + MarkSceneDirty + SaveScene"

requirements-completed: [VIS-01, VIS-02, VIS-03, VIS-04, VIS-05, VIS-06, VIS-07, VIS-08, AUD-07]

# Metrics
duration: 15min
completed: 2026-03-28
---

# Phase 06 Plan 05: Phase6Setup Unity Assets Summary

**Editor script Phase6Setup.cs создан и выполнен: AudioData.asset, vfx_blow.prefab с ParticleSystem(stopAction=Callback), AudioManager с 9 AudioSource в сцене, LeaderboardScreen Canvas, TitleScreen "ASTEROIDS", Camera.backgroundColor=black**

## Performance

- **Duration:** ~15 min
- **Started:** 2026-03-28T21:12:41Z
- **Completed:** 2026-03-28T21:27:00Z
- **Tasks:** 2 (+ checkpoint)
- **Files created:** 3 (Phase6Setup.cs, AudioData.asset, vfx_blow.prefab)
- **Files modified:** 3 (Main.unity, GameData.asset, LeaderboardScreen.cs)

## Accomplishments

- Phase6Setup.cs создан с 7 методами: CreateAudioDataAsset, CreateVfxBlowPrefab, SetupAudioManagerInScene, SetupLeaderboardScreen, UpdateTitleScreen, UpdateCamera, UpdateGameData
- MenuItem "Asteroids/Setup Phase 6 Assets" выполнен через MCP — все assets материализованы
- AudioData.asset создан с 9 полями AudioClip (null — ожидаемо, пользователь добавит файлы)
- vfx_blow.prefab: ParticleSystem(loop=false, stopAction=Callback, burst=15 частиц) + EffectVisual
- AudioManager GameObject с 9 дочерними AudioSource в сцене; thrust/ufoTone с loop=true
- LeaderboardScreen: полноэкранный Image(black,alpha=0.9) + LEADERBOARD title + placeholder + Back button
- TitleScreen: title_text "ASTEROIDS" (fontSize=96) + leaderboard_button (disabled до Phase 7)
- Camera.backgroundColor = Color.black, clearFlags = SolidColor (VIS-02)
- GameData.asset: Audio=AudioData, VfxBlowPrefab=vfx_blow назначены
- ApplicationEntry: _audioManager, _leaderboardView, _leaderboardGo назначены

## Task Commits

1. **Task 1: Создать Phase6Setup.cs Editor script** — `0ad1679` (feat)
2. **Task 2: Запустить Phase6Setup через MCP + исправить баг** — `b406af0` (feat)

## Files Created/Modified

- `Assets/Editor/Phase6Setup.cs` — новый Editor script с MenuItem и 7 методами настройки
- `Assets/Media/configs/AudioData.asset` — ScriptableObject AudioData (clips null)
- `Assets/Media/prefabs/vfx_blow.prefab` — ParticleSystem + EffectVisual prefab
- `Assets/Scenes/Main.unity` — AudioManager, LeaderboardScreen, TitleScreen, Camera обновлены
- `Assets/Media/configs/GameData.asset` — Audio и VfxBlowPrefab назначены
- `Assets/Scripts/Application/Screens/LeaderboardScreen.cs` — исправлен баг Disconnect→Dispose

## Decisions Made

- Phase6Setup следует конвенции Phase3/4/5 Setup скриптов: один MenuItem вызывает все методы настройки
- AudioClip поля оставлены null — отдельный user_setup шаг; игра запускается без crash благодаря null-guard в AudioManager.PlayOneShot
- ParticleSystem.stopAction = Callback — критично: без него OnParticleSystemStopped не вызывается и EffectVisual не возвращается в пул

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Исправлен LeaderboardScreen.Hide() Disconnect() → Dispose()**
- **Найдено при:** Task 2 — первая компиляция
- **Проблема:** AbstractWidgetView не имеет метода Disconnect() — метод называется Dispose()
- **Исправление:** Заменено `_view.Disconnect()` на `_view.Dispose()` в LeaderboardScreen.cs:27
- **Файл:** Assets/Scripts/Application/Screens/LeaderboardScreen.cs
- **Коммит:** b406af0

## Known Stubs

- `Assets/Media/configs/AudioData.asset` — все 9 AudioClip полей null. Звуки не воспроизводятся до назначения .wav/.ogg файлов пользователем через Inspector. Это ожидаемое поведение (user_setup из плана), игра работает без crash.

## User Setup Required

После прохождения checkpoint:
- Назначить AudioClip файлы (.wav или .ogg) в Assets/Media/configs/AudioData.asset → Inspector
  - Поля: Shoot, Thrust, ExplodeShip, ExplodeAsteroidBig, ExplodeAsteroidMedium, ExplodeAsteroidSmall, UfoTone, BeatLow, BeatHigh

## Self-Check: PASSED

- Assets/Editor/Phase6Setup.cs: FOUND
- Assets/Media/configs/AudioData.asset: FOUND
- Assets/Media/prefabs/vfx_blow.prefab: FOUND
- Commits 0ad1679, b406af0: FOUND
- Compilation: 0 errors

---
*Phase: 06-audio-visual-polish*
*Completed: 2026-03-28*
