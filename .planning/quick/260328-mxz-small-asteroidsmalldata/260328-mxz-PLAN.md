---
quick_task: 260328-mxz
type: execute
wave: 1
depends_on: []
files_modified:
  - Assets/Media/configs/AsteroidSmallData.asset
  - Assets/Editor/Phase4Setup.cs
  - .planning/STATE.md
autonomous: true

must_haves:
  truths:
    - "AsteroidSmallData.asset содержит 3 small-спрайта (не medium)"
    - "Phase4Setup.cs назначает asteroid_small_1/2/3 для AsteroidSmall"
    - "STATE.md не содержит устаревшую запись о medium-спрайтах для small"
  artifacts:
    - path: "Assets/Media/configs/AsteroidSmallData.asset"
      provides: "SpriteVariants с fileID -166148694, 32597434, 1187889473"
    - path: "Assets/Editor/Phase4Setup.cs"
      provides: "sprites: asteroid_small_1, asteroid_small_2, asteroid_small_3 для AsteroidSmall"
  key_links:
    - from: "AsteroidSmallData.asset"
      to: "asteroids.png (GUID cf467e92e508b5878eb24c5a126421e0)"
      via: "fileID internalID спрайтов"
      pattern: "fileID: -166148694|32597434|1187889473"
---

<objective>
Назначить нарезанные small-спрайты (asteroid_small_1/2/3) в AsteroidSmallData конфиге
и актуализировать Phase4Setup.cs и STATE.md.

Purpose: Ранее small-спрайты отсутствовали в PNG, поэтому AsteroidSmall использовал medium-спрайты
как заглушку. Теперь PNG содержит 3 small-варианта — нужно переключить конфиг и editor-скрипт.

Output:
- AsteroidSmallData.asset с правильными fileID small-спрайтов
- Phase4Setup.cs исправлен: CreateAsteroidConfigs() назначает asteroid_small_*
- STATE.md обновлён: убрана устаревшая запись о medium-заглушках для small
</objective>

<execution_context>
@$HOME/.claude/get-shit-done/workflows/execute-plan.md
</execution_context>

<context>
@.planning/STATE.md
@Assets/Media/configs/AsteroidSmallData.asset
@Assets/Editor/Phase4Setup.cs

<!-- Спрайты small-астероидов в asteroids.png (GUID: cf467e92e508b5878eb24c5a126421e0) -->
<!-- asteroid_small_1: internalID -166148694, rect (0,0,32,32)   -->
<!-- asteroid_small_2: internalID 32597434,   rect (32,0,32,32)  -->
<!-- asteroid_small_3: internalID 1187889473, rect (64,0,32,32)  -->

<!-- Unity sub-sprite fileID в YAML = internalID спрайта -->
<!-- Текущий AsteroidSmallData.asset содержит fileID: 5, 6, 7 (medium-спрайты — заглушки) -->
<!-- Phase4Setup.cs строка 62-63: захардкожены "asteroid_medium_1/2/3" для AsteroidSmall -->
<!-- STATE.md строка 51: "AsteroidSmall использует medium-спрайты — PNG не содержит small-вариантов" — устарело -->
</context>

<tasks>

<task type="auto">
  <name>Задача 1: Обновить AsteroidSmallData.asset — назначить small-спрайты</name>
  <files>Assets/Media/configs/AsteroidSmallData.asset</files>
  <action>
    Заменить SpriteVariants в YAML-файле Assets/Media/configs/AsteroidSmallData.asset.

    Текущее содержимое SpriteVariants (заглушки medium):
    ```yaml
    SpriteVariants:
    - {fileID: 5, guid: cf467e92e508b5878eb24c5a126421e0, type: 3}
    - {fileID: 6, guid: cf467e92e508b5878eb24c5a126421e0, type: 3}
    - {fileID: 7, guid: cf467e92e508b5878eb24c5a126421e0, type: 3}
    ```

    Заменить на (internalID small-спрайтов из asteroids.png):
    ```yaml
    SpriteVariants:
    - {fileID: -166148694, guid: cf467e92e508b5878eb24c5a126421e0, type: 3}
    - {fileID: 32597434, guid: cf467e92e508b5878eb24c5a126421e0, type: 3}
    - {fileID: 1187889473, guid: cf467e92e508b5878eb24c5a126421e0, type: 3}
    ```

    GUID текстуры остаётся неизменным: cf467e92e508b5878eb24c5a126421e0.
    Остальные поля файла (Score, Prefab, m_Script и т.д.) НЕ трогать.
  </action>
  <verify>
    Проверить содержимое файла:
    grep "fileID: -166148694" Assets/Media/configs/AsteroidSmallData.asset
    grep "fileID: 32597434" Assets/Media/configs/AsteroidSmallData.asset
    grep "fileID: 1187889473" Assets/Media/configs/AsteroidSmallData.asset
    Все три команды должны вернуть результат.
  </verify>
  <done>AsteroidSmallData.asset содержит 3 ссылки на small-спрайты с корректными fileID.</done>
</task>

<task type="auto">
  <name>Задача 2: Исправить Phase4Setup.cs — использовать asteroid_small_* для AsteroidSmall</name>
  <files>Assets/Editor/Phase4Setup.cs</files>
  <action>
    В методе CreateAsteroidConfigs() найти блок обновления AsteroidSmallData.asset (строки ~59-64).

    Текущий код (неверный — использует medium-спрайты):
    ```csharp
    // Обновить или создать AsteroidSmallData.asset — Score=3, используем medium-спрайты (PNG не содержит small)
    UpdateOrCreateAsteroidConfig(
        "Assets/Media/configs/AsteroidSmallData.asset",
        score: 3,
        sprites: new[] { "asteroid_medium_1", "asteroid_medium_2", "asteroid_medium_3" }
    );
    ```

    Заменить на:
    ```csharp
    // Обновить или создать AsteroidSmallData.asset — Score=3, 3 small-спрайта
    UpdateOrCreateAsteroidConfig(
        "Assets/Media/configs/AsteroidSmallData.asset",
        score: 3,
        sprites: new[] { "asteroid_small_1", "asteroid_small_2", "asteroid_small_3" }
    );
    ```

    Только этот блок. Остальной файл не трогать.
  </action>
  <verify>
    grep "asteroid_small_1" Assets/Editor/Phase4Setup.cs
    Должна вернуть строку с именами small-спрайтов.
    grep "medium.*Small\|Small.*medium" Assets/Editor/Phase4Setup.cs
    Должна вернуть пустой результат (medium-заглушек для Small больше нет).
  </verify>
  <done>Phase4Setup.cs назначает asteroid_small_1/2/3 для AsteroidSmallData. Комментарий про "medium-спрайты" убран.</done>
</task>

<task type="auto">
  <name>Задача 3: Обновить STATE.md — зафиксировать готовность small-спрайтов</name>
  <files>.planning/STATE.md</files>
  <action>
    В разделе ## Decisions найти строку:
    ```
    - [Phase 04]: AsteroidSmall использует medium-спрайты — PNG не содержит small-вариантов
    ```

    Заменить на:
    ```
    - [Phase 04]: AsteroidSmall использует собственные small-спрайты (asteroid_small_1/2/3) — нарезаны вручную в asteroids.png, rect 32×32
    ```

    Остальное содержимое STATE.md не трогать.
  </action>
  <verify>
    grep "asteroid_small_1/2/3" .planning/STATE.md
    Должна вернуть обновлённую строку.
    grep "medium-спрайты — PNG не содержит" .planning/STATE.md
    Должна вернуть пустой результат.
  </verify>
  <done>STATE.md содержит актуальную запись о small-спрайтах; устаревшая запись про medium-заглушки удалена.</done>
</task>

</tasks>

<verification>
После выполнения всех задач:
1. Assets/Media/configs/AsteroidSmallData.asset — SpriteVariants содержит fileID: -166148694, 32597434, 1187889473
2. Assets/Editor/Phase4Setup.cs — CreateAsteroidConfigs() использует "asteroid_small_1/2/3" для AsteroidSmallData
3. .planning/STATE.md — содержит запись о small-спрайтах, не medium-заглушках
</verification>

<success_criteria>
- AsteroidSmallData.asset ссылается на 3 уникальных small-спрайта из asteroids.png (не medium)
- Phase4Setup.cs при запуске "Asteroids/Setup Phase 4 Assets" назначит корректные small-спрайты
- STATE.md отражает актуальное состояние
</success_criteria>

<output>
После выполнения создать `.planning/quick/260328-mxz-small-asteroidsmalldata/260328-mxz-SUMMARY.md` с кратким описанием что было сделано.
</output>
