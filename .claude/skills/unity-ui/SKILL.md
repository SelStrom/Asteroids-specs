# Unity UI Skill

## Обязательные правила при создании/изменении UI элементов

### Anchor + Pivot
- Всегда явно устанавливай `pivot` перед `anchoredPosition` — AddComponent даёт pivot (0.5, 0.5) по умолчанию
- Правило: pivot должен совпадать с anchor corner:
  - Верх-лево: `anchorMin = anchorMax = (0, 1)`, `pivot = (0, 1)`
  - Верх-право: `anchorMin = anchorMax = (1, 1)`, `pivot = (1, 1)`
  - Низ-лево: `anchorMin = anchorMax = (0, 0)`, `pivot = (0, 0)`
  - Центр: `anchorMin = anchorMax = (0.5, 0.5)`, `pivot = (0.5, 0.5)`
  - Stretch: `anchorMin = (0, 0)`, `anchorMax = (1, 1)`, `offsetMin = offsetMax = (0, 0)`

### Растяжка панелей
- Панель-контейнер (Hud, GameOverScreen и т.п.) должна растягиваться на весь родительский Canvas:
  ```csharp
  rect.anchorMin = Vector2.zero;
  rect.anchorMax = Vector2.one;
  rect.offsetMin = Vector2.zero;
  rect.offsetMax = Vector2.zero;
  ```

### Поиск неактивных объектов
- `GameObject.Find()` не находит неактивные (SetActive=false) объекты
- Используй `Resources.FindObjectsOfTypeAll<T>()` и фильтруй по `gameObject.scene`

### Перекрытие элементов
- Перед позиционированием нового элемента проверь существующих соседей в той же панели
- Если есть debug-элементы с другим anchor — убедись, что новые элементы не занимают ту же экранную область
- Используй `VerticalLayoutGroup` на контейнере вместо ручных Y-смещений, если элементов > 2

### Проверка после создания
- После `ApplyModifiedProperties()` залоггируй каждое поле явно — NULL сигнализирует о несохранённом asset/объекте
- Если Setup-скрипт создаёт элементы — запускай его и проверяй Console перед Play Mode
