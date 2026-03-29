---
status: resolved
trigger: "MCP инструмент get_game_state возвращает isPlaying:false когда Unity игра запущена в Play Mode"
created: 2026-03-29T00:00:00Z
updated: 2026-03-29T00:00:00Z
---

## Current Focus

hypothesis: System.Type.GetType("..., Asteroids") возвращает null потому что McpUnityBridge.asmdef не ссылается на "Asteroids" сборку, и .NET не ищет тип в незагруженных/нереференсированных сборках через простой GetType.
test: заменить System.Type.GetType на перебор AppDomain.CurrentDomain.GetAssemblies() — гарантированно найдёт тип в любой загруженной сборке
expecting: isPlaying:true при Play Mode, score/wave/lives корректные
next_action: awaiting_human_verify — Fix применён, нужно пересобрать и протестировать

## Symptoms

expected: get_game_state возвращает isPlaying:true когда Unity Editor находится в Play Mode
actual: get_game_state возвращает isPlaying:false даже когда игра запущена
errors: нет явных ошибок, просто неверное значение
reproduction: 1) Запустить Play Mode в Unity 2) Вызвать get_game_state через MCP 3) Получить isPlaying:false
started: обнаружено 2026-03-29, неизвестно работало ли раньше

## Eliminated

- hypothesis: Проблема в TypeScript index.ts (неверный endpoint или парсинг)
  evidence: callBridge("/get_game_state") вызывается корректно, данные возвращаются с isPlaying полем
  timestamp: 2026-03-29T00:00:00Z

- hypothesis: RuntimeBridgeProxy.UpdateState не вызывается
  evidence: ApplicationEntry.Update() вызывает RuntimeBridgeProxy.UpdateState каждый кадр с _application?.IsRunning
  timestamp: 2026-03-29T00:00:00Z

- hypothesis: Поля RuntimeBridgeProxy скрыты #if UNITY_EDITOR в Assembly-CSharp
  evidence: Исправление #if UNITY_EDITOR не помогло — проблема не в директивах, а в имени сборки
  timestamp: 2026-03-29T01:00:00Z

- hypothesis: Неверное имя сборки "Assembly-CSharp" в GetType — нужно "Asteroids"
  evidence: Исправление на "Asteroids" не помогло — всё равно isPlaying:false. Проблема не в имени сборки.
  timestamp: 2026-03-29T02:00:00Z

## Evidence

- timestamp: 2026-03-29T01:00:00Z
  checked: Assets/Asteroids.asmdef — имя сборки для рантайм скриптов
  found: |
    Assets/Asteroids.asmdef содержит "name": "Asteroids".
    RuntimeBridgeProxy.cs находится в Assets/Scripts/Application/ — под этим .asmdef.
    Сборка называется "Asteroids", а НЕ "Assembly-CSharp".
    McpUnityBridge.cs строка 650: GetType("SelStrom.Asteroids.RuntimeBridgeProxy, Assembly-CSharp")
    ВСЕГДА возвращает null — имя сборки не совпадает.
    type == null → немедленно возвращает {"isPlaying":false} — независимо от Play Mode!
  implication: |
    НАСТОЯЩИЙ ROOT CAUSE: неверное имя сборки в GetType.
    Всё предыдущее расследование (#if UNITY_EDITOR) было неверным следом.
    Исправление #if UNITY_EDITOR не могло помочь, потому что проблема в другом месте.
    Нужно заменить "Assembly-CSharp" на "Asteroids" в строке 650 McpUnityBridge.cs.

- timestamp: 2026-03-29T00:00:00Z
  checked: McpUnityBridge.cs HandleGetGameState (строки 648-676)
  found: |
    Строка 650: var type = System.Type.GetType("SelStrom.Asteroids.RuntimeBridgeProxy, Assembly-CSharp");
    Строка 652: if (type == null || !EditorApplication.isPlaying)
    Строка 654:     SendJsonRaw(context, "{\"success\":true,\"score\":0,\"wave\":0,\"lives\":0,\"isPlaying\":false}");

    КРИТИЧЕСКАЯ ПРОБЛЕМА: условие `type == null || !EditorApplication.isPlaying` означает:
    - если тип НЕ найден через GetType (а это происходит в Edit Mode) → немедленно вернуть isPlaying:false
    - НО в Play Mode тип RuntimeBridgeProxy ТОЖЕ может не найтись через GetType в первые моменты

    НАСТОЯЩАЯ ПРОБЛЕМА: RuntimeBridgeProxy определён с #if UNITY_EDITOR директивой.
    Поле IsRunning существует только в #if UNITY_EDITOR блоке.
    В Play Mode Assembly-CSharp компилируется КАК RUNTIME сборка, где UNITY_EDITOR не определён.
    Значит RuntimeBridgeProxy в Play Mode НЕ ИМЕЕТ полей Score/Wave/Lives/IsRunning.
    GetType может найти класс, но GetField("IsRunning") вернёт null → NullReferenceException.

    ОДНАКО: реальная проблема ещё проще. McpUnityBridge.cs — это Editor код (#if UNITY_EDITOR через namespace Editor).
    RuntimeBridgeProxy с #if UNITY_EDITOR — его поля видны в Editor контексте.
    Но GetType("SelStrom.Asteroids.RuntimeBridgeProxy, Assembly-CSharp") ищет тип в Assembly-CSharp.
    В Play Mode класс RuntimeBridgeProxy существует в Assembly-CSharp, НО его поля (Score, Wave, Lives, IsRunning)
    объявлены только под #if UNITY_EDITOR — в сборке Assembly-CSharp они ОТСУТСТВУЮТ в рантайме!

    GetField("Score").GetValue(null) → NullReferenceException (GetField вернёт null).
    Исключение поймано в catch HandleRequest → возвращается 500/error.

    НО: есть ещё один путь — проверить что именно происходит при type == null.
    В Play Mode Unity Editor Assembly-CSharp ВКЛЮЧАЕТ рантайм скрипты.
    UNITY_EDITOR не определён для Assembly-CSharp даже при запуске в Editor.
    Значит RuntimeBridgeProxy в Assembly-CSharp компилируется без полей.
    System.Type.GetType найдёт КЛАСС (он существует), но GetField("Score") вернёт null.

  implication: |
    Есть ДВЕ потенциальные проблемы:
    1. type может найтись (класс существует в Assembly-CSharp), но поля отсутствуют → NullReferenceException на GetField().GetValue()
    2. Или type вообще null в Play Mode (менее вероятно)

    В обоих случаях DESIGN ошибка: #if UNITY_EDITOR скрывает поля RuntimeBridgeProxy от Assembly-CSharp.
    McpUnityBridge (Editor-only код) пытается читать поля через рефлексию из Assembly-CSharp сборки,
    где эти поля не существуют.

- timestamp: 2026-03-29T00:00:00Z
  checked: RuntimeBridgeProxy.cs
  found: |
    Поля Score, Wave, Lives, IsRunning и методы UpdateState/Reset обёрнуты в #if UNITY_EDITOR.
    Значит в сборке Assembly-CSharp (рантайм) эти поля НЕ существуют.
    GetField("Score") вернёт null → GetValue(null) бросит NullReferenceException.

    ФИНАЛЬНЫЙ ВЫВОД: HandleGetGameState падает с NullReferenceException при попытке GetField("Score").GetValue(null).
    Исключение ловится в HandleRequest catch и возвращает error-ответ или пустой ответ.

    НО: на самом деле catch в HandleRequest возвращает JSON с success:false и message.
    index.ts парсит этот JSON и возвращает его как text. В нём нет isPlaying поля.

    ПЕРЕСМОТР: посмотрим на условие снова:
    if (type == null || !EditorApplication.isPlaying)

    Если type не null (класс найден), но EditorApplication.isPlaying == true → идём дальше.
    GetField("Score") вернёт null → GetField("Score").GetValue(null) бросит NullReferenceException.
    Исключение всплывает в HandleRequest catch → SendJsonRaw с error JSON без isPlaying.

    А вот в условии: type == null. Если тип НЕ найден (что возможно если сборка не загружена),
    то сразу возвращает {"isPlaying":false}.

    Основная проблема очевидна: поля RuntimeBridgeProxy скрыты #if UNITY_EDITOR от Assembly-CSharp.

  implication: |
    ROOT CAUSE: RuntimeBridgeProxy.cs обёртывает поля в #if UNITY_EDITOR.
    McpUnityBridge (Editor код) ищет эти поля через рефлексию в Assembly-CSharp сборке.
    В Assembly-CSharp поля НЕ скомпилированы (нет UNITY_EDITOR define).
    Результат: GetField("Score") == null → NullReferenceException → error ответ без isPlaying.
    ИЛИ: type == null → немедленный возврат {"isPlaying":false}.

    В обоих случаях isPlaying возвращается false.

## Resolution

root_cause: |
  McpUnityBridge.HandleGetGameState использовала System.Type.GetType("..., Asteroids") для поиска типа.
  McpUnityBridge.asmdef (сборка "McpUnityBridge") не имеет references на сборку "Asteroids".
  В .NET/Mono, System.Type.GetType ищет тип только в текущей сборке и mscorlib, если не передано
  полное имя сборки. С именем сборки — ищет только если сборка уже загружена, НО незареференсированные
  сборки могут не быть доступны через простой поиск по имени.
  Результат: GetType всегда возвращал null → `type == null` → isPlaying:false.

fix: |
  1. RuntimeBridgeProxy.cs: убраны директивы #if UNITY_EDITOR (поля теперь всегда в сборке).
  2. McpUnityBridge.cs: System.Type.GetType заменён на перебор AppDomain.CurrentDomain.GetAssemblies():
     foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
     {
         var t = asm.GetType("SelStrom.Asteroids.RuntimeBridgeProxy");
         if (t != null) { return t; }
     }
  3. Логика HandleGetGameState изменена: сначала проверяем EditorApplication.isPlaying,
     затем ищем тип — если тип не найден но isPlaying:true, возвращаем isPlaying:true с нулями
     и пишем Warning в лог.

verification: Подтверждено пользователем 2026-03-29. get_game_state возвращает isPlaying:true в Play Mode, isRunning:true при активном геймплее, isRunning:false на title screen.
files_changed:
  - Assets/Scripts/Application/RuntimeBridgeProxy.cs
  - Packages/com.shtl.mcp-unity/Editor/McpUnityBridge.cs
