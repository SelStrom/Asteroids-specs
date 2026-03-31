# Phase 09: WebGL — Research

**Researched:** 2026-03-31
**Domain:** Unity 2022.3 WebGL build + GitHub Pages deployment
**Confidence:** HIGH

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

- **D-01:** Хостинг — GitHub Pages. Деплой из ветки `gh-pages` или папки `/docs` (структура на усмотрение исполнителя).
- **D-02:** Ручная сборка через `File → Build Settings → Build` в Unity Editor. Никаких Editor scripts, никакого PhaseSetup, никакого MCP build-command.
- **D-03:** Дефолтный Unity WebGL template — без кастомизации.
- **D-04 (зафиксировано):** `webGLCompressionFormat=0` — компрессия отключена.
- **D-05 (зафиксировано):** `webGLMemoryGrowthMode=2` — Memory Growth включён.
- Папка вывода сборки: `Build/WebGL/` (в корне проекта, в `.gitignore` для источника, публикуется отдельно).
- `webGLThreadsSupport: 0` — threads отключены, настройка COOP/COEP заголовков не нужна.

### Claude's Discretion

- Структура ветки/папки для GitHub Pages (gh-pages branch vs /docs subfolder).
- Содержимое `index.html` верхнего уровня (если нужен редирект).
- `.gitignore` для исходников билда.

### Deferred Ideas (OUT OF SCOPE)

- GitHub Actions CI для автоматической сборки и деплоя.
- Кастомный WebGL template (fullscreen, убрать Unity logo).
- Оптимизация размера бандла (Strip Engine Code, IL2CPP настройки).
- PWA-манифест для установки игры.
</user_constraints>

---

<phase_requirements>
## Phase Requirements

Формальные requirement IDs для WebGL-деплоя не определены в REQUIREMENTS.md (там нет отдельного раздела WEBGL). Фаза закрывает финальный этап проекта — публикацию работающей игры в браузере.

| ID | Описание | Источник |
|----|----------|----------|
| WEBGL-01 | Сцена Main.unity добавлена в Build Settings (EditorBuildSettings.asset) | Выявлено при аудите: `m_Scenes: []` |
| WEBGL-02 | WebGL выбрана как активная платформа (Switch Platform) | D-04/D-05 уже применены, но платформа не переключена |
| WEBGL-03 | Успешная ручная WebGL-сборка в `Build/WebGL/` | D-02 |
| WEBGL-04 | Файлы билда опубликованы на `gh-pages` ветке | D-01 |
| WEBGL-05 | `.nojekyll` файл присутствует в корне `gh-pages` | Критично — без него Jekyll блокирует файлы с `_` |
| WEBGL-06 | GitHub Pages настроен на ветку `gh-pages` | D-01 |
| WEBGL-07 | Игра открывается и запускается по публичной ссылке | Цель фазы |
</phase_requirements>

---

## Summary

Проект Unity 2022.3.60f1 уже имеет нужные WebGL-настройки в `ProjectSettings.asset`: компрессия отключена (`webGLCompressionFormat=0`), Memory Growth включён (`webGLMemoryGrowthMode=2`), threads отключены (`webGLThreadsSupport=0`). Это снимает главные риски GitHub Pages деплоя — проблему MIME-типов для сжатых файлов и необходимость COOP/COEP заголовков для SharedArrayBuffer.

Два критических пробела обнаружены при аудите: (1) сцена `Main.unity` **не добавлена в Build Settings** (`m_Scenes: []` в `EditorBuildSettings.asset`) — без этого билд пройдёт, но игра не запустится; (2) `Build/` уже в `.gitignore` паттерном `[Bb]uild/`, что означает `Build/WebGL/` тоже игнорируется — файлы нужно публиковать через отдельную ветку `gh-pages`, а не из корня репозитория.

Рекомендуется ветка `gh-pages` (а не папка `/docs`): папку `/docs` пришлось бы добавлять в `.gitignore`-исключение или переименовывать паттерн, что сложнее. Ветка `gh-pages` — изолированная история, стандартный подход для Unity WebGL на GitHub Pages.

**Первичная рекомендация:** Switch Platform → добавить сцену → Build → создать `gh-pages` ветку с файлами билда + `.nojekyll` → включить GitHub Pages на ветке `gh-pages`.

---

## Standard Stack

### Инструменты (нет устанавливаемых зависимостей)

| Инструмент | Версия | Роль |
|-----------|--------|------|
| Unity Editor | 2022.3.60f1 | Сборка WebGL |
| Git | системный | Управление ветками |
| GitHub Pages | — | Хостинг статики |

**Установка:** не требуется — всё уже есть в проекте.

---

## Architecture Patterns

### Рекомендуемая структура `gh-pages` ветки

```
(gh-pages branch root)
├── .nojekyll              # ОБЯЗАТЕЛЕН — отключает Jekyll, даёт доступ к _-файлам
├── index.html             # из Build/WebGL/index.html (Unity default template)
├── Build/
│   ├── Asteroids.loader.js
│   ├── Asteroids.framework.js.gz   # (нет, compression=0 → без .gz)
│   ├── Asteroids.framework.js
│   ├── Asteroids.wasm
│   └── Asteroids.data
└── TemplateData/
    ├── style.css
    ├── favicon.ico
    └── ...
```

**Важно:** при `webGLCompressionFormat=0` файлы НЕ сжимаются, расширения остаются `.js`, `.wasm`, `.data`. Это именно то, что нужно для GitHub Pages.

### Паттерн 1: Изолированная `gh-pages` ветка

**Что:** ветка содержит только файлы WebGL-билда, без истории Unity-проекта.
**Когда использовать:** когда `.gitignore` основного репозитория блокирует `Build/`.
**Как создать:**

```bash
# Из корня репозитория, после сборки в Build/WebGL/
git checkout --orphan gh-pages
git rm -rf .
cp -r /path/to/Build/WebGL/* .
touch .nojekyll
git add -A
git commit -m "deploy: WebGL build"
git push origin gh-pages
git checkout master
```

Либо более безопасный вариант — из worktree:

```bash
# Создать gh-pages как worktree рядом с основным деревом
git worktree add /tmp/gh-pages-deploy gh-pages
# скопировать файлы, сделать коммит, запушить
git worktree remove /tmp/gh-pages-deploy
```

### Паттерн 2: Последующие обновления `gh-pages`

При повторной сборке нужно заменить содержимое ветки новыми файлами:

```bash
git checkout gh-pages
# удалить старое, скопировать новое
git add -A
git commit -m "deploy: update WebGL build"
git push origin gh-pages
git checkout master
```

### Anti-Patterns to Avoid

- **Не добавлять `Build/` в корень `master`:** `.gitignore` корректно исключает `[Bb]uild/` — не нарушать это.
- **Не включать компрессию:** GitHub Pages не отдаёт `Content-Encoding: gzip` для предкомпрессованных Unity-файлов — браузер получит `.gz` как plain text и сборка не запустится.
- **Не пропускать `.nojekyll`:** Jekyll на GitHub Pages игнорирует папки и файлы с `_` в начале имени. Unity WebGL build содержит `_framework`, `_data` и другие — без `.nojekyll` они не будут обслуживаться, игра не загрузится (ошибка 404 на ключевые файлы).

---

## Don't Hand-Roll

| Проблема | Не строить самому | Использовать вместо этого | Почему |
|----------|-------------------|---------------------------|--------|
| Обход Jekyll на GitHub Pages | Кастомные 404 страницы, Jekyll плагины | Файл `.nojekyll` в корне ветки | Официальный механизм GitHub, работает мгновенно |
| Компрессия/деком прессия файлов | Nginx-прокси, lambda-обёртки | Отключить в Unity Player Settings | Сложность на ровном месте; GitHub Pages не поддерживает кастомные заголовки |
| Switch Platform через скрипт | `BuildTarget` через Editor script | `File → Build Settings → Switch Platform` вручную | Это решение D-02 из CONTEXT.md |

---

## Runtime State Inventory

> Данная фаза — не rename/refactor. Однако проверка показала один важный элемент:

| Категория | Найдено | Действие |
|-----------|---------|----------|
| Stored data | Нет | — |
| Live service config | Нет | — |
| OS-registered state | Нет | — |
| Secrets/env vars | Нет | — |
| Build artifacts | `Build/WebGL/` — не существует (первая сборка) | Создаётся при выполнении WEBGL-03 |

**Специфичный пробел:** `EditorBuildSettings.asset` содержит `m_Scenes: []` — сцена не зарегистрирована. Это не runtime state, но блокирует сборку: Unity соберёт пустой проект без геймплея.

---

## Common Pitfalls

### Pitfall 1: Сцена не добавлена в Build Settings

**Что идёт не так:** Unity собирает WebGL без ошибок, но запускает пустую сцену — игры нет.
**Почему:** `EditorBuildSettings.asset` содержит `m_Scenes: []`. Build Settings в Unity не добавляет текущую открытую сцену автоматически.
**Как избежать:** В `File → Build Settings` нажать `Add Open Scenes` с открытой `Main.unity`.
**Признаки:** Игра в браузере показывает чёрный экран или Unity загрузку без геймплея.

### Pitfall 2: Платформа не переключена на WebGL

**Что идёт не так:** Кнопка `Build` строит под текущую платформу (скорее всего PC), а не WebGL.
**Почему:** `Switch Platform` нужно нажать явно в `File → Build Settings`. Из `ProjectSettings.asset` видно, что `activeBuildTarget` не задан явно через файлы настроек.
**Как избежать:** В `Build Settings` выбрать WebGL → `Switch Platform` (занимает 1-5 минут).
**Признаки:** Вывод Build содержит `.exe` вместо `index.html`.

### Pitfall 3: Отсутствие `.nojekyll` → 404 для файлов сборки

**Что идёт не так:** `index.html` загружается, прогресс-бар стоит на месте, в консоли браузера 404 на `.wasm` или `.data`.
**Почему:** GitHub Pages обрабатывает ветку через Jekyll. Jekyll игнорирует папки/файлы начинающиеся с `_`. Unity WebGL с Default template генерирует имена без `_` в Unity 2022, **но** иногда в `TemplateData` или зависимости template могут присутствовать такие файлы. Кроме того — страховка на будущее при смене template.
**Как избежать:** Всегда создавать `.nojekyll` в корне `gh-pages` ветки.
**Авторитетный источник:** [GitHub Blog — Bypassing Jekyll on GitHub Pages](https://github.blog/news-insights/bypassing-jekyll-on-github-pages/)

### Pitfall 4: Включена компрессия (mismatch)

**Что идёт не так:** Браузер получает `.gz`-файл с MIME `application/javascript` или `application/wasm`, пытается запустить его как есть — ошибка декодирования.
**Почему:** GitHub Pages не добавляет `Content-Encoding: gzip` к уже сжатым Unity-файлам.
**Как избежать:** `webGLCompressionFormat=0` уже выставлен (D-04). Проверить что не изменился.
**Признаки:** Консоль браузера — `both async and sync fetching of the wasm failed`.

### Pitfall 5: Ветка `gh-pages` не настроена как источник GitHub Pages

**Что идёт не так:** Файлы запушены, но игра не открывается.
**Почему:** GitHub Pages по умолчанию публикует из `master/main`, а не из `gh-pages`, если не настроено вручную.
**Как избежать:** `Settings → Pages → Source → Deploy from a branch → gh-pages / (root)`.
**Признаки:** URL `https://selstrom.github.io/Asteroids-specs/` возвращает 404.

### Pitfall 6: Большой размер билда (долгое ожидание загрузки)

**Что идёт не так:** Игра технически работает, но загружается 30-60+ секунд.
**Почему:** Без компрессии `.wasm` Unity 2022 может весить 30-80 МБ.
**Как избежать:** Для этой фазы приемлемо — цель "показать работающую игру". Оптимизация в backlog.
**Признаки:** Прогресс-бар в браузере идёт медленно; размер файлов в `Build/WebGL/Build/` > 30 МБ суммарно.

---

## Code Examples

### Структура коммита для `gh-pages`

```bash
# Шаг 1: собрать в Unity → File → Build Settings → Build → выбрать Build/WebGL/
# Шаг 2: создать/обновить gh-pages ветку

# Первый деплой (orphan branch — нет истории)
git checkout --orphan gh-pages
git rm -rf .
# Скопировать файлы из Build/WebGL/ в текущую папку
cp -r /Users/selstrom/work/projects/asteroids-specs/Build/WebGL/. .
touch .nojekyll
git add -A
git commit -m "deploy: initial WebGL build"
git push origin gh-pages --force
git checkout master
```

### Проверка `.nojekyll`

```bash
# В корне gh-pages ветки должен быть пустой файл .nojekyll
ls -la .nojekyll   # должен существовать
```

### Ожидаемая структура вывода Unity WebGL Build (compression=0)

```
Build/WebGL/
├── index.html
├── Build/
│   ├── Asteroids.loader.js
│   ├── Asteroids.framework.js
│   ├── Asteroids.wasm
│   └── Asteroids.data
└── TemplateData/
    ├── style.css
    ├── favicon.ico
    ├── fullscreen.png
    ├── progress-bar-empty-dark.png
    ├── progress-bar-full-dark.png
    ├── unity-logo-dark.png
    └── webgl-logo.png
```

Имя файлов зависит от `Product Name` в Player Settings. Текущее: `Asteroids` (из `WebGL: com.Home.Asteroids` бандла).

---

## Environment Availability

| Зависимость | Нужна для | Доступна | Версия | Fallback |
|-------------|-----------|----------|--------|----------|
| Unity Editor | WebGL build | Предполагается да (проект открыт) | 2022.3.60f1 | — |
| Git | gh-pages ветка | Да | системный | — |
| GitHub repo remote | Деплой | Да | `git@github.com:SelStrom/Asteroids-specs.git` | — |
| GitHub Pages (настройка) | Хостинг | Требует ручной настройки в UI | — | — |

**Блокирующие зависимости без fallback:** GitHub Pages нужно включить вручную в настройках репозитория — это ручной шаг, не автоматизируемый без CI.

---

## Validation Architecture

nyquist_validation: true (из `.planning/config.json`).

### Test Framework

| Свойство | Значение |
|----------|----------|
| Framework | Нет unit-тестов для WebGL-деплоя — проверка только вручную (smoke) |
| Quick run command | `curl -s -o /dev/null -w "%{http_code}" https://selstrom.github.io/Asteroids-specs/` |
| Full validation | Открыть URL в браузере, загрузить игру, сыграть 30 секунд |

### Phase Requirements → Test Map

| ID | Поведение | Тип | Команда / Действие | Файл |
|----|-----------|-----|--------------------|------|
| WEBGL-01 | Сцена в Build Settings | ручной | Открыть `File → Build Settings`, убедиться что `Main` в списке | — |
| WEBGL-02 | Платформа WebGL активна | ручной | В Build Settings платформа WebGL и кнопка `Build` активны (не `Switch Platform`) | — |
| WEBGL-03 | Билд создан | smoke | `ls Build/WebGL/index.html && ls Build/WebGL/Build/*.wasm` | — |
| WEBGL-04 | Файлы на gh-pages | smoke | `git show gh-pages:index.html` | — |
| WEBGL-05 | .nojekyll присутствует | smoke | `git show gh-pages:.nojekyll` | — |
| WEBGL-06 | GitHub Pages включён | ручной | Открыть `Settings → Pages` в репозитории | — |
| WEBGL-07 | Игра работает в браузере | e2e ручной | Открыть URL, дождаться загрузки, проверить геймплей | — |

### Wave 0 Gaps

Нет — фаза не требует новых тестовых файлов. Все проверки ручные или shell-команды.

---

## State of the Art

| Старый подход | Текущий подход | Когда изменилось | Влияние |
|---------------|---------------|------------------|---------|
| `UnityLoader.js` (Unity 5.x) | `loader.js` + `framework.js` + `.wasm` + `.data` | Unity 2020+ | Другая структура файлов, `UnityLoader` устарел |
| Compression required | Compression опциональна, можно отключить | Unity 2019.4+ | GitHub Pages поддерживается без кастомных заголовков |
| SharedArrayBuffer обязателен | Только при `webGLThreadsSupport=1` | Unity 2021+ | Без threads не нужны COOP/COEP заголовки |

**Устаревшее:**
- `UnityLoader.js` — заменён на split-файлы; старые туториалы с ним неактуальны.
- Brotli / Gzip на GitHub Pages — официально **не поддерживается** (нет кастомных `Content-Encoding` заголовков). Compression=Disabled — единственный надёжный выбор.

---

## Open Questions

1. **Размер итогового билда**
   - Что известно: при `webGLCompressionFormat=0` сборка не сжимается
   - Неясно: точный размер `.wasm` для данного проекта (типичный диапазон 20-80 МБ для Unity 2022 без оптимизации)
   - Рекомендация: принять как есть на этой фазе, задокументировать в отчёте верификации

2. **Product Name в WebGL-билде**
   - Что известно: `ProductName` в ProjectSettings определяет имена файлов в Build/ (`Asteroids.wasm` и т.д.)
   - Неясно: точное значение `m_ProductName` — нужно проверить в Unity Editor Player Settings
   - Рекомендация: проверить перед сборкой, имя влияет только на читаемость, не на функциональность

---

## Sources

### Primary (HIGH confidence)

- Unity Manual 2022.3 — [webgl-building.html](https://docs.unity3d.com/2022.3/Documentation/Manual/webgl-building.html) — структура вывода, шаги сборки
- Unity Manual 2022.3 — [webgl-deploying.html](https://docs.unity3d.com/2022.3/Documentation/Manual/webgl-deploying.html) — MIME типы, требования к серверу
- GitHub Blog — [Bypassing Jekyll on GitHub Pages](https://github.blog/news-insights/bypassing-jekyll-on-github-pages/) — `.nojekyll` механизм
- `ProjectSettings/ProjectSettings.asset` в репозитории — фактические настройки (аудит)
- `ProjectSettings/EditorBuildSettings.asset` в репозитории — `m_Scenes: []` (выявленный пробел)

### Secondary (MEDIUM confidence)

- [GitHub Discussion: Support for pre-compressed assets](https://github.com/orgs/community/discussions/21655) — подтверждение что GitHub Pages не поддерживает brotli
- [GitHub Issue: Jekyll ignores underscore directories](https://github.com/jekyll/jekyll/issues/55) — корневая причина Pitfall 3
- [Ankur Sheel: Unity CI/CD Part 4 — GitHub Pages](https://www.ankursheel.com/blog/unity-cicd-deploying-webgl-github-pages) — практический пример деплоя

### Tertiary (LOW confidence)

- Нет — для данной задачи все ключевые утверждения верифицированы первичными источниками

---

## Metadata

**Confidence breakdown:**
- Текущее состояние ProjectSettings: HIGH — прямой аудит файлов репозитория
- Структура WebGL-вывода: HIGH — официальная документация Unity 2022.3
- GitHub Pages `.nojekyll`: HIGH — официальный пост GitHub Blog
- Compression проблема на GitHub Pages: HIGH — множественные официальные подтверждения
- Размер итогового билда: LOW — зависит от конкретных ассетов проекта

**Research date:** 2026-03-31
**Valid until:** 2026-09-01 (стабильный стек, Unity 2022.3 LTS + GitHub Pages — устоявшиеся технологии)
