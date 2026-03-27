# Phase 1: Project Foundation — Context

**Gathered:** 2026-03-27
**Status:** Ready for planning

<domain>
## Phase Boundary

Создать новый Unity 2022.3 LTS проект с нуля: настроить Player Settings для WebGL, подключить все UPM-пакеты, создать структуру папок, настроить физические слои, импортировать предоставленный PNG-спрайтшит через Sprite Editor и создать Sprite Atlas. Embedded-пакет `com.shtl.mcp-unity` добавляется как пустая заглушка (scaffold) для последующей реализации в Phase 2.

**Игровой код, механика и сцены НЕ реализуются в этой фазе.**

</domain>

<decisions>
## Implementation Decisions

### Архитектура сцен
- **D-01:** Одна сцена — `Assets/Scenes/Main.unity`. Переключение между экранами (Title, Game, Leaderboard) через `SetActive()` на Canvas-объектах, не через `SceneManager.LoadScene`. Точно соответствует референсной архитектуре.
- **D-02:** REQUIREMENTS.md (SETUP-04) был написан с предположением о multi-scene. Решение: обновить REQUIREMENTS — оставить требование создания сцены, скорректировать описание под single-scene подход.

### Текстурный атлас
- **D-03:** Пользователь предоставит один PNG-файл (spritesheet). Нарезка спрайтов через Unity Sprite Editor (режим Automatic или Grid by Cell Size). Не используется TexturePacker или Aseprite-импортер.
- **D-04:** После нарезки создаётся Unity Sprite Atlas (`Assets/Media/sprites/GameAtlas.spriteAtlas`) — Allow Rotation = false, Padding = 4, Generate Mip Maps = false. Все нарезанные спрайты добавляются в атлас.
- **D-05:** Исходный PNG кладётся в `Assets/Media/sprites/` с Import Settings: Texture Type = Sprite (2D and UI), Sprite Mode = Multiple, Filter Mode = Point (No Filter) для пиксельного стиля, Compression = None или High Quality.

### Структура проекта
- **D-06:** Структура папок точно повторяет референс: `Assets/Editor/`, `Assets/Input/`, `Assets/Media/configs/`, `Assets/Media/effects/`, `Assets/Media/prefabs/`, `Assets/Media/sprites/`, `Assets/Resources/`, `Assets/Scenes/`, `Assets/Scripts/Application/`, `Assets/Scripts/Configs/`, `Assets/Scripts/Input/`, `Assets/Scripts/Model/`, `Assets/Scripts/Utils/`, `Assets/Scripts/View/`.
- **D-07:** Три Assembly Definition файла: `Assets/Asteroids.asmdef` (namespace `SelStrom.Asteroids`), `Assets/Scripts/Configs/Configs.asmdef` (namespace `SelStrom.Asteroids.Configs`), `Assets/Editor/AsteroidsEditor.asmdef` (Editor-only).

### Unity Player Settings (WebGL)
- **D-08:** Color Space = Gamma, Rendering = Forward (Built-in RP), Fullscreen Mode = Windowed для WebGL, Default Resolution = 1920×1080.
- **D-09:** WebGL: Memory Growth = enabled, Compression Format = Disabled (упрощает деплой, избегает проблем с сервер-заголовками на этапе разработки).
- **D-10:** Splash Screen отключён (`m_ShowUnitySplashScreen: 0`).

### Физические слои
- **D-11:** Настроить Layer 7 = Player, Layer 8 = Asteroid, Layer 9 = PlayerBullet, Layer 10 = EnemyBullet, Layer 11 = Enemy. Collision Matrix: PlayerBullet коллайдит с Asteroid и Enemy. EnemyBullet коллайдит с Player. Asteroid коллайдит с Player.

### UPM-пакеты
- **D-12:** Пакеты через `Packages/manifest.json`: `com.unity.inputsystem@1.19.0`, `com.unity.textmeshpro@3.0.9`, `com.unity.ugui@1.0.0`, `com.unity.feature.2d@2.0.1`, `com.unity.services.core@1.16.0`, `com.unity.services.authentication@3.6.0`, `com.unity.services.leaderboards@2.3.3`, `com.shtl.mvvm` (git: `https://github.com/SelStrom/shtl-mvvm.git#c7bda1c`).
- **D-13:** Embedded-пакет `com.shtl.mcp-unity` создаётся как scaffold: `Packages/com.shtl.mcp-unity/package.json` + пустые папки `Editor/`, `Runtime/`, `Editor~/Server/`. Реализация — Phase 2.

### UGS Configuration
- **D-14:** Cloud Project ID `b80d4dd7-4bb0-4c81-b29b-4e84466d4630` прописывается в `ProjectSettings/ProjectSettings.asset` (поле `cloudProjectId`). UGS будет работать с этим проектом.

### Claude's Discretion
- Точные настройки TextMesh Pro (шрифты, материалы) — стандартный импорт TMP Essentials
- Конкретный размер ячейки или режим Auto в Sprite Editor — определяется по реальному PNG
- Содержимое `.gitignore` — стандартный Unity `.gitignore`

</decisions>

<specifics>
## Specific Ideas

- Структура папок точно воспроизводит референс (по `.planning/codebase/STRUCTURE.md`) — это намеренно, т.к. цель — полная реконструкция
- Пользователь предоставит PNG позже — Phase 1 создаёт инфраструктуру, атлас настраивается после получения файла
- `com.shtl.mvvm` подключается через конкретный git-хэш `c7bda1c` для воспроизводимости

</specifics>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Project structure & architecture
- `.planning/codebase/STRUCTURE.md` — Полная структура папок, имена файлов, куда добавлять новый код
- `.planning/codebase/STACK.md` — Все пакеты с версиями, сборочная система, asmdef-файлы
- `.planning/codebase/ARCHITECTURE.md` — Архитектурные слои, точки входа, паттерны

### Unity configuration
- `.planning/codebase/UNITY_CONFIG.md` — Все настройки ProjectSettings: теги, слои, физика 2D, Player Settings

### Requirements
- `.planning/REQUIREMENTS.md` — SETUP-01..05 — требования этой фазы
- `.planning/PROJECT.md` — Контекст проекта, технические constraints

### Research
- `.planning/research/unity-webgl-asteroids.md` §1 — WebGL Player Settings, compрессия, Built-in RP рекомендации
- `.planning/research/mcp-unity-package.md` §4 — Структура embedded UPM-пакета

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- Нет существующего кода — проект создаётся с нуля.

### Established Patterns (из codebase map — для воспроизведения)
- `GameObjectPool` — пул GameObject по prefab-id, использовать в Phase 3+
- `CoroutineResult<T>` — типизированный результат корутины, использовать везде вместо async/await
- Три asmdef-файла: Asteroids (main), Conf (configs), AsteroidsEditor (Editor-only)

### Integration Points
- `ApplicationEntry.cs` — единственный MonoBehaviour, точка входа. Все остальные объекты через `new`.
- `GameData.asset` — главный конфиг ScriptableObject, содержит ссылки на все суб-конфиги и prefab'ы.

</code_context>

<deferred>
## Deferred Ideas

- Настройка Input Actions (`player_actions.inputactions`) — Phase 3 (Core Mechanics)
- Создание prefab'ов игровых объектов — Phase 3+
- Конфигурация ScriptableObject ассетов с данными — Phase 3+
- Реализация MCP-сервера — Phase 2

</deferred>

---

*Phase: 01-project-foundation*
*Context gathered: 2026-03-27*
