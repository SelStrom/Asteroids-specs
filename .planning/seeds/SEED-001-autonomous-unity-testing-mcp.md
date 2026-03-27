---
id: SEED-001
status: dormant
planted: 2026-03-28
planted_during: v1.0 / Phase 04-asteroids-progression
trigger_when: Любая фаза требует тестирования поведения в Unity или разработки MCP инструментария
scope: Medium
---

# SEED-001: Автономное тестирование Unity и MCP-first разработка

## Why This Matters

Три принципа рабочего процесса, которые должны применяться во всех фазах:

1. **Автономное тестирование поведения** — Claude может самостоятельно проверять поведение в Unity без участия пользователя: вызвать обновление домена (AssetDatabase.Refresh / compile), запустить Play mode через MCP, распарсить вывод EditorLog для получения результатов.

2. **Запуск Unity тестов** — EditMode и PlayMode тесты запускаются через CLI:
   ```bash
   # EditMode tests
   /Applications/Unity/Hub/Editor/6000.0.69f1/Unity.app/Contents/MacOS/Unity \
     -runTests -batchmode -projectPath "$(pwd)" \
     -testPlatform EditMode -testResults "./test-results-editmode.xml" \
     -logFile - 2>&1

   # PlayMode tests (только если EditMode прошёл)
   /Applications/Unity/Hub/Editor/6000.0.69f1/Unity.app/Contents/MacOS/Unity \
     -runTests -batchmode -projectPath "$(pwd)" \
     -testPlatform PlayMode -testResults "./test-results-playmode.xml" \
     -logFile - 2>&1
   ```
   Либо — если редактор уже запущен — доработать MCP для запуска тестов через bridge.

3. **MCP-first** — все манипуляции с Unity (создание объектов, назначение компонентов, запуск Play mode, импорт ассетов) производятся через развитие инструментария MCP, а не через ручные инструкции пользователю.

## When to Surface

**Trigger:** Любая фаза, где нужно верифицировать поведение в Unity или где Claude даёт ручные инструкции для Unity операций.

Этот seed должен быть представлен при `/gsd:new-milestone` если milestone включает:
- Тестирование игровой логики в Unity
- Разработку или расширение MCP инструментария
- Любые Unity Editor операции (создание prefabs, настройка сцены, импорт ассетов)

## Scope Estimate

**Medium** — требует планирования:
- Расширение McpUnityBridge для новых операций (запуск тестов, чтение EditorLog, управление ассетами)
- Возможно отдельная Phase 8 (MCP Runtime) уже частично покрывает это
- Baseline (запуск тестов через CLI) — применяется немедленно без изменений

## Breadcrumbs

- `Packages/com.shtl.mcp-unity/Editor/McpUnityBridge.cs` — HTTP bridge, точка расширения для новых MCP tools
- `.planning/ROADMAP.md §Phase 8` — MCP Runtime phase (RuntimeBridgeProxy, get_game_state)
- `.planning/phases/03-core-mechanics/03-UAT.md` — пример где ручные Unity операции потребовали участия пользователя
- `Assets/Editor/Phase3Setup.cs` — Editor script паттерн для автоматизации Unity операций

## Notes

Посеяно во время UAT Phase 3 — пользователь явно указал что хочет развивать MCP как основной инструмент взаимодействия с Unity. Три пункта выше — конкретные механизмы реализации этого принципа.

Применяется немедленно для CLI-тестов (без изменений кода). MCP-расширения — планировать в рамках Phase 8 или отдельной фазы.
