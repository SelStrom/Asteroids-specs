---
status: complete
phase: 03-core-mechanics
source: 03-01-SUMMARY.md, 03-02-SUMMARY.md, 03-03-SUMMARY.md
started: 2026-03-27T19:00:00Z
updated: 2026-03-28T10:00:00Z
---

## Current Test

[testing complete]

## Tests

### 1. Компиляция проекта без ошибок
expected: Открой Unity Editor. В Console (Window → Console) не должно быть красных ошибок компиляции. Если ошибки есть — опиши текст.
result: pass

### 2. MenuItem Phase3Setup запускается
expected: В меню Unity Editor должен появиться пункт Asteroids → Setup Phase 3 Assets. Нажми его — он должен завершиться без ошибок в Console. После запуска в Project window должны появиться prefabs (ship.prefab, bullet.prefab, bullet_enemy.prefab) и конфиги (GameData.asset, UserGunData.asset, UfoGunData.asset).
result: issue
reported: "MissingComponentException: RectTransform not attached to Play — fixed by creating GameObject with typeof(RectTransform)"
severity: major

### 3. Сцена настроена скриптом
expected: После запуска MenuItem в Hierarchy сцены должны появиться: Canvas (с TitleScreen и HUD объектами), EventSystem. Camera должна быть orthographic с Size ≈ 22.5.
result: issue
reported: "Дублирующийся EventSystem — старый не удалялся при повторном запуске Setup; StandaloneInputModule конфликтовал с Input System"
severity: major

### 4. PlayerActions.inputactions содержит 5 действий
expected: Открой Assets/Input/PlayerActions.inputactions в Inspector. В Action Map "Player" должно быть 5 actions: Rotate, Thrust, Attack, Laser, Back.
result: issue
reported: "No action 'Rotate' — LoadAssetJson() падал в пустой fallback т.к. файл не в Resources папке"
severity: major

### 5. GameData ScriptableObject корректен
expected: Открой Assets/Media/configs/GameData.asset в Inspector. Должны быть вложенные секции: Ship (с полями Prefab, MainSprite, ThrustSprite, Gun), BulletData (Prefab, EnemyPrefab, LifeTimeSeconds=2, Speed), LaserData (MaxShoots=5, ReloadDurationSec). Поля Prefab и Sprite могут быть пустыми — это нормально до ручного назначения.
result: pass

### 6. TitleScreen показывается при старте
expected: Выполни ручное назначение из User Setup Required: GameData.asset → ApplicationEntry._configs в Inspector сцены. Нажми Play. Должен появиться экран заголовка с кнопкой PLAY (или текстовый элемент).
result: pass

### 7. Корабль движется и вращается
expected: После Setup и нажатия PLAY в игровом экране корабль должен появиться. Нажатие A/D (или стрелки) — корабль вращается. W — ускоряется. Wrap-around при достижении края.
result: skipped
reason: пользователь принял решение двигаться дальше

### 8. Стрельба работает с лимитом
expected: Space (Attack) — вылетает пуля, исчезает через ~2с. После 5 выстрелов — лимит. После исчезновения пуль — стрельба возобновляется.
result: skipped
reason: пользователь принял решение двигаться дальше

## Summary

total: 8
passed: 3
issues: 3
pending: 0
skipped: 2
blocked: 0

## Gaps

- truth: "Phase3Setup MenuItem запускается без ошибок и создаёт сцену с одним EventSystem"
  status: failed
  reason: "User reported: MissingComponentException (RectTransform на Play GO) + дублирующийся EventSystem + StandaloneInputModule конфликт"
  severity: major
  test: 2
  root_cause: "GameObject('Play') создавался без RectTransform; старый EventSystem не удалялся при повторном запуске; Unity автодобавляет StandaloneInputModule при AddComponent<EventSystem>()"
  artifacts:
    - path: "Assets/Editor/Phase3Setup.cs"
      issue: "Три бага — исправлены в рамках UAT"
  missing: []

- truth: "PlayerInput загружает все 5 actions из PlayerActions.inputactions"
  status: failed
  reason: "User reported: No action 'Rotate' in PlayerActions"
  severity: major
  test: 4
  root_cause: "LoadAssetJson() не использовал AssetDatabase в Editor — файл не в Resources, падал в пустой fallback"
  artifacts:
    - path: "Assets/Scripts/Input/Generated/PlayerActions.cs"
      issue: "LoadAssetJson() отсутствовал UNITY_EDITOR AssetDatabase fallback — исправлен"
  missing: []
