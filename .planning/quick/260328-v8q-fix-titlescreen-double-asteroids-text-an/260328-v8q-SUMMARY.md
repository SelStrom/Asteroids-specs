---
phase: quick-260328-v8q
plan: 01
subsystem: ui-scene-vfx
tags: [bugfix, titlescreen, particlesystem, phase6]
key-files:
  modified:
    - Assets/Scenes/Main.unity
    - Assets/Editor/Phase6Setup.cs
    - Assets/Media/prefabs/vfx_blow.prefab
decisions:
  - vfx_blow.prefab UVModule.enabled=0 — отключить TextureSheetAnimation в YAML, а не удалять блок (Unity требует наличия всех модулей)
  - Phase6Setup.cs UpdateTitleScreen: Find("Title") + DestroyImmediate добавлен до удаления title_text — защита от двойного запуска
metrics:
  duration: "~10m"
  completed: "2026-03-28T21:36:06Z"
  tasks: 3
  files: 3
---

# Quick Task 260328-v8q: Fix TitleScreen Double ASTEROIDS Text and ParticleSystem noninit Error

**One-liner:** Удалён дублирующий Title GameObject Phase3Setup из Main.unity и отключён UVModule в vfx_blow.prefab для устранения noninit property error.

## Tasks Completed

| # | Name | Commit | Files |
|---|------|--------|-------|
| 1 | Удалить дублирующий Title GameObject из Main.unity | 31016ae | Assets/Scenes/Main.unity |
| 2 | Phase6Setup.cs — удалять "Title" при UpdateTitleScreen | a73068b | Assets/Editor/Phase6Setup.cs |
| 3 | Убрать TextureSheetAnimation из Phase6Setup.cs и vfx_blow.prefab | 684dd33 | Assets/Editor/Phase6Setup.cs, Assets/Media/prefabs/vfx_blow.prefab |

## What Was Done

### Задача 1 — Main.unity

Из списка `m_Children` RectTransform TitleScreen (fileID: 1336126270) удалена ссылка `{fileID: 1580767957}`.

Удалены четыре YAML-блока дублирующего Title GameObject (Phase3Setup legacy):
- `!u!1 &1580767956` — GameObject "Title"
- `!u!224 &1580767957` — RectTransform
- `!u!114 &1580767958` — MonoBehaviour (TMP "ASTEROIDS", fontSize=56)
- `!u!222 &1580767959` — CanvasRenderer

### Задача 2 — Phase6Setup.cs UpdateTitleScreen()

Добавлен блок поиска и удаления дочернего объекта "Title" (Phase3Setup legacy) перед созданием нового title_text. Повторный запуск `Asteroids > Setup Phase 6 Assets` не оставит дублирующий Title.

### Задача 3 — TextureSheetAnimation

- `Phase6Setup.cs CreateVfxBlowPrefab()`: удалён блок `var tsa = ps.textureSheetAnimation; tsa.enabled = true; tsa.mode = ...; tsa.AddSprite(bulletSprite)`.
- `vfx_blow.prefab`: в секции `UVModule` изменено `enabled: 1` → `enabled: 0`. Блок не удалялся — Unity ожидает наличие всех модулей ParticleSystem в YAML.

## Verification Results

```
grep "1580767956|..." Main.unity → 0 совпадений — OK
grep '"Title"' Phase6Setup.cs   → найдена строка Find("Title") — OK
grep "textureSheetAnimation|AddSprite" Phase6Setup.cs → 0 совпадений — OK
UVModule.enabled в vfx_blow.prefab → 0 — OK
```

## Deviations from Plan

Нет. План выполнен в точности.

## Self-Check: PASSED

- [x] Assets/Scenes/Main.unity — изменён, содержит 0 ссылок на удалённые fileID
- [x] Assets/Editor/Phase6Setup.cs — содержит Find("Title") + DestroyImmediate, не содержит TSA вызовов
- [x] Assets/Media/prefabs/vfx_blow.prefab — UVModule.enabled=0
- [x] Коммиты 31016ae, a73068b, 684dd33 созданы
