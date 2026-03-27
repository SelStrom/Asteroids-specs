---
phase: 3
slug: core-mechanics
status: draft
shadcn_initialized: false
preset: none
created: 2026-03-27
platform: Unity 2022.3 uGUI + TextMeshPro
---

# Phase 3 — UI Design Contract: Core Mechanics

> Визуальный и интеракционный контракт для фазы. Это НЕ веб-приложение.
> Все измерения — в Canvas-пикселях (Reference Resolution 1920x1080).
> Источники: SCENE.md, DATA_SCHEMA.md, 3-CONTEXT.md, REQUIREMENTS.md, UNITY_CONFIG.md.

---

## Design System

| Свойство | Значение |
|----------|---------|
| Tool | none (Unity uGUI) |
| Preset | не применимо — Unity Canvas |
| Render Mode | Screen Space — Camera (привязан к Main Camera, PlaneDistance = 1) |
| CanvasScaler | Scale With Screen Size, Reference 1920×1080, Match Width (0) |
| Компонентная библиотека | TextMeshPro (TMP) для всего текста |
| Иконки | Sub-sprites из `Assets/Media/sprites/` (PNG нарезан в Sprite Editor) |
| Шрифт | TextMeshPro default (Arial SDF или встроенный) |
| Sorting Layers | Только Default (uniqueID: 0) |

Источник: SCENE.md §UI (Canvas), UNITY_CONFIG.md §Sorting Layers

---

## Spacing Scale

Все значения в Canvas-пикселях (1:1 с Reference Resolution 1920×1080).

| Токен | Значение | Применение |
|-------|---------|------------|
| xs | 4 px | Зазор между иконками жизней (фаза 4+) |
| sm | 8 px | Внутренний отступ текстовых меток от края RectTransform |
| md | 16 px | Отступ HUD-панели от края экрана (якорный offset) |
| lg | 24 px | Вертикальный шаг между строками HUD |
| xl | 32 px | Зазор между блоками TitleScreen (заголовок → кнопка) |
| 2xl | 48 px | Не используется в Phase 3 |
| 3xl | 64 px | Не используется в Phase 3 |

Исключения: HUD-панель (`Hud` GameObject) — якорь AnchorMin(0,1)/AnchorMax(0,1), offset позиции (16, −16) — это значение md=16.

Источник: SCENE.md §Hud

---

## Typography

Все размеры — в единицах TextMeshPro (font size в TMP = пиксели Canvas при scale=1).

| Роль | Размер | Вес | Line Height | Применение |
|------|--------|-----|-------------|-----------|
| HUD Label | 28 | Regular (400) | 1.2 | `gui_text.prefab` — координаты, скорость, угол, лазер-заряды |
| Screen Title | 56 | Bold (700) | 1.1 | Название игры на TitleScreen ("ASTEROIDS") |
| Button Label | 28 | Regular (400) | 1.0 | Кнопки TitleScreen («PLAY») |
| Debug / No display | — | — | — | Координаты/угол/скорость — debug HUD, Phase 3 только |

Размер 28 взят из `gui_text.prefab` (SCENE.md §gui_text: `шрифт размер 28`).
Размер 56 — двойной масштаб от базового для заголовка, соответствует аркадной эстетике.

Wrapping: `enableWordWrapping = false` для всех HUD-меток (однострочные).
Alignment: левое выравнивание для HUD; центральное для TitleScreen.

Источник: SCENE.md §gui_text.prefab

---

## Color

Стиль: ретро-аркада — чёрный фон, белые векторные спрайты.

| Роль | Hex | Alpha | Применение |
|------|-----|-------|-----------|
| Dominant (60%) — фон | `#000000` | 255 | Camera Clear Color (Solid Color), фон сцены |
| Secondary (30%) — поверхности | `#000000` | 0 | Canvas и панели прозрачны — игровое поле видно сквозь HUD |
| Accent (10%) — интерактив | `#FFFFFF` | 255 | Весь текст TMP, спрайты корабля/пуль/лазера |
| Semantic — мигание неуязвимости | `#FFFFFF` / `#000000` | toggle | SpriteRenderer.enabled toggle каждые 150 мс (3 сек после respawn) |
| Laser charge indicator | `#00FF41` | 255 | Зарезервировано под счётчик зарядов лазера (отличается от основного белого) |

Accent зарезервирован исключительно для:
- Текст всех TMP-меток (HUD, TitleScreen)
- SpriteRenderer спрайты: ship, bullet, bullet_enemy, laser beam
- Кнопка Play (UnityEngine.UI.Button, нормальное состояние)

Зелёный `#00FF41` зарезервирован исключительно для счётчика зарядов лазера (`_laserShootCount`, `_laserReloadTime`) — визуально отличает игровой ресурс от информационного текста.

Destructive / Error: не применяется в Phase 3 (Game Over — Phase 4).

Источник: SCENE.md §Main Camera (Clear Flags: Solid Color, фон чёрный), CONTEXT.md §D-01 (спрайты белые векторные)

---

## Canvas Hierarchy Contract

Единственная сцена: `Assets/Scenes/Main.unity`.
Переключение экранов — через `SetActive()` на дочерних панелях Canvas.

```
UI (Canvas — Screen Space Camera, 1920×1080)
├── TitleScreen    [active=true  при старте]
│   ├── Title label      TMP "ASTEROIDS", size=56, anchor=center-top, pos=(0, -120)
│   └── Play button      TMP "PLAY", size=28, anchor=center, pos=(0, -40)
└── Hud            [active=false, включается Game.Start()]
    ├── coordinates      gui_text.prefab, anchor=top-left, pos=(16,-16)
    ├── rotation_angle   gui_text.prefab, anchor=top-left, pos=(16,-40)
    ├── speed_text       gui_text.prefab, anchor=top-left, pos=(16,-64)
    └── laser_shoot_count gui_text.prefab, anchor=top-left, pos=(16,-88)
        └── (laser_reload_time — дочерний или отдельный, pos=(16,-112))
```

Вертикальный шаг между HUD-строками: lg=24 px.
Начальный offset HUD-панели: (16, −16) = md от верхнего левого угла.

Источник: SCENE.md §Hud, REQUIREMENTS.md §PROG-02, CONTEXT.md §D-20

---

## Component Inventory

Компоненты Unity uGUI, создаваемые в Phase 3:

| Компонент | Prefab / GameObject | Назначение |
|-----------|--------------------|-----------|
| `Canvas` | UI (сцена) | Корневой Canvas, уже существует |
| `CanvasScaler` | UI (сцена) | Уже настроен 1920×1080 |
| `TextMeshPro` | `gui_text.prefab` | HUD-метки: координаты, угол, скорость, лазер |
| `UnityEngine.UI.Button` | TitleScreen > Play | Запуск Game.Start() |
| `TextMeshPro` | TitleScreen > Title | Заголовок игры |
| `HudVisual` (MonoBehaviour) | Hud | Биндит TMP-метки к ViewModel корабля |
| `TitleScreen` (MonoBehaviour) | TitleScreen | Связывает кнопку Play с Application |

Компоненты Phase 4+ (не создавать в Phase 3):
- Score / Game Over экран
- Иконки жизней (PROG-03 — Phase 4)
- High Score label (PROG-07 — Phase 4)

Источник: CONTEXT.md §D-14, D-15, D-20

---

## HUD Data Bindings

Все биндинги реализованы через `EventBindingContext` + `BindingToExtensions` (паттерн MVVM).

| HUD-поле | Метка формат | Источник данных | Тип ObservableValue |
|----------|-------------|-----------------|---------------------|
| `_coordinates` | `"X: {0:F1}  Y: {1:F1}"` | `ShipViewModel.Position` | `ReactiveValue<Vector2>` |
| `_rotationAngle` | `"Angle: {0:F0}"` | `ShipViewModel.Angle` | `ReactiveValue<float>` |
| `_speed` | `"Speed: {0:F1}"` | `ShipViewModel.Speed` | `ReactiveValue<float>` |
| `_laserShootCount` | `"Laser: {0}/3"` | `LaserComponent.CurrentShoots` | `ObservableValue<int>` |
| `_laserReloadTime` | `"Reload: {0:F1}s"` | `LaserComponent.ReloadTimeLeft` | `ObservableValue<float>` |

Лазер-максимум = 3 (LaserMaxShoots=3 из DATA_SCHEMA.md).
Метки `_coordinates`, `_rotationAngle`, `_speed` — debug-режим для Phase 3; будут скрыты или заменены в Phase 5 (HUD-баннер волны).

Источник: DATA_SCHEMA.md §LaserData, SCENE.md §Hud

---

## Interaction Contract

### TitleScreen

| Элемент | Действие | Результат |
|---------|----------|-----------|
| Кнопка "PLAY" | Click (Pointer Up) | `TitleScreen.SetActive(false)`, `Hud.SetActive(true)`, `Game.Start()` |

Нет hover-эффектов (аркадный стиль, минималистичный).
Button Normal Color: `#FFFFFF` (белый текст). Highlighted: `#CCCCCC`. Pressed: `#888888`.
Button Transition: Color Tint.

### Ship respawn (визуальный контракт)

| Событие | Визуальное поведение | Тайминг |
|---------|---------------------|---------|
| `ShipModel.Kill()` | `SpriteRenderer.enabled = false` | мгновенно |
| Respawn | Корабль появляется в центре (0,0) | через 2.0 сек (ActionScheduler) |
| Иммунитет | `SpriteRenderer.enabled` toggle каждые **150 мс** | 3.0 сек |
| Иммунитет завершён | `SpriteRenderer.enabled = true` постоянно | через 3.0 сек |

Источник: CONTEXT.md §D-05, REQUIREMENTS.md §SHIP-06, SHIP-07

### Laser beam (визуальный контракт)

| Событие | Визуальное поведение | Тайминг |
|---------|---------------------|---------|
| Выстрел лазером | `LaserPrefab` активируется, `SpriteRenderer` виден | мгновенно |
| Beam исчезает | `LaserPrefab.SetActive(false)` или `LifeTimeComponent` | через **0.5 сек** (BeamEffectLifetimeSec) |
| Заряд восстанавливается | `_laserShootCount` + 1 каждые 10 сек / 3 заряда | `LaserUpdateDurationSec=10` сек |
| Нет зарядов | Кнопка Q не создаёт эффект, `GunSystem` игнорирует | немедленно |

Источник: DATA_SCHEMA.md §LaserData

### Thrust indicator (визуальный контракт)

| Состояние | Спрайт корабля |
|-----------|---------------|
| Тяга НЕ нажата | `ShipData.MainSprite` (корабль без сопла) |
| Тяга НАЖАТА (W удерживается) | `ShipData.ThrustSprite` (корабль с соплом огня) |

Смена спрайта — через `ShipVisual.OnThrustChanged()`, реакция на `ThrustComponent.IsActive`.

Источник: REQUIREMENTS.md §SHIP-08, DATA_SCHEMA.md §ShipData

---

## Camera Contract

| Параметр | Значение | Обоснование |
|----------|---------|-------------|
| Projection | Orthographic | 2D аркада |
| Orthographic Size | **22.5** | Охват 45 мировых единиц по высоте при 1080p |
| Position | (0, 0, −10) | Центр поля, вдоль Z |
| Clear Flags | Solid Color | Чёрный фон `#000000` |
| Target Display | Display 1 | Единственный дисплей |
| HDR | Выкл. | Built-in RP, WebGL |
| MSAA | Выкл. на камере | Quality Settings управляет (Ultra = 2x) |

Игровое поле в мировых единицах при Reference Resolution 1920×1080 и Size=22.5:
- Высота: 45 единиц (от −22.5 до +22.5)
- Ширина: 80 единиц (от −40 до +40, ratio 16:9)

Wrap-around граница: `|x| > 40`, `|y| > 22.5` — телепорт на противоположный край.

Источник: SCENE.md §Main Camera

---

## Copywriting Contract

| Элемент | Текст | Экран |
|---------|-------|-------|
| Заголовок игры | `ASTEROIDS` | TitleScreen |
| Кнопка старта | `PLAY` | TitleScreen |
| HUD координаты | `X: {value}  Y: {value}` | Hud |
| HUD угол | `Angle: {value}` | Hud |
| HUD скорость | `Speed: {value}` | Hud |
| HUD заряды лазера | `Laser: {current}/3` | Hud |
| HUD таймер лазера | `Reload: {seconds}s` | Hud |

Empty state: не применяется в Phase 3 — игровое поле всегда содержит корабль.
Error state: не применяется в Phase 3 — ошибки только в Phase 7 (UGS).
Destructive confirmation: нет — Game Over в Phase 4.

Все строки — латиница (аркадный стиль оригинала). Без локализации.

Источник: CONTEXT.md (scope Phase 3), REQUIREMENTS.md §VIS-05, §PROG-02

---

## Prefab Inventory (Phase 3)

| Prefab | Путь | Layer | Collider |
|--------|------|-------|---------|
| `ship.prefab` | `Assets/Media/prefabs/` | 7 (Player) | PolygonCollider2D — треугольник (1,0)(−0.5,0.5)(−0.5,−0.5) |
| `bullet.prefab` | `Assets/Media/prefabs/` | 9 (PlayerBullet) | CircleCollider2D, radius=0.2 |
| `bullet_enemy.prefab` | `Assets/Media/prefabs/` | 10 (EnemyBullet) | CircleCollider2D, radius=0.2 (variant) |
| `gui_text.prefab` | `Assets/Media/prefabs/gui/` | UI (5) | нет |

Prefabs Phase 4+ (НЕ создавать в Phase 3): asteroid_*, ufo_*, vfx_blow.

Источник: SCENE.md §Префабы, CONTEXT.md §D-19

---

## Sprite Contract

Все спрайты — sub-sprites из нарезанного PNG `Assets/Media/sprites/asteroids.png`.
Sprite Atlas (`GameAtlas.spriteAtlas`) **не используется** — prefabs ссылаются на sub-sprites напрямую.

| Сущность | Поле в GameData | Спрайт |
|----------|----------------|--------|
| Корабль (без тяги) | `ShipData.MainSprite` | sub-sprite "ship" |
| Корабль (тяга) | `ShipData.ThrustSprite` | sub-sprite "ship_thrust" |
| Пуля игрока | `BulletData.Prefab` → SpriteRenderer | sub-sprite "bullet" |
| Вражеская пуля | `BulletData.EnemyPrefab` → SpriteRenderer | sub-sprite "bullet" (или вариант) |
| Лазер (beam) | `LaserData.Prefab` → SpriteRenderer | sub-sprite "laser" |

SpriteRenderer цвет: `#FFFFFF` (255,255,255,255) — белый, без оттенков.
SortingLayer: Default, Order=0 для всех игровых объектов.

Источник: CONTEXT.md §D-01, SCENE.md §ship.prefab, §bullet.prefab

---

## Registry Safety

| Registry | Используемые блоки | Safety Gate |
|----------|--------------------|-------------|
| Unity Package Manager | com.unity.textmeshpro (встроен в Unity 2022.3) | не требуется — официальный |
| Unity Package Manager | com.unity.inputsystem | не требуется — официальный |
| Git UPM | com.shtl.mvvm (hash c7bda1c) | не требуется — внутренний пакет |

Сторонние npm/shadcn registry: не применимо (Unity проект, не web).

---

## Checker Sign-Off

- [ ] Dimension 1 Copywriting: PASS
- [ ] Dimension 2 Visuals: PASS
- [ ] Dimension 3 Color: PASS
- [ ] Dimension 4 Typography: PASS
- [ ] Dimension 5 Spacing: PASS
- [ ] Dimension 6 Registry Safety: PASS

**Approval:** pending

---

*UI-SPEC создан: 2026-03-27*
*Фаза: 03-core-mechanics*
*Источники: SCENE.md, DATA_SCHEMA.md, 3-CONTEXT.md, REQUIREMENTS.md §SHIP/SHOT, UNITY_CONFIG.md*
