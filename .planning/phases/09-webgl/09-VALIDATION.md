---
phase: 09
slug: webgl
status: draft
nyquist_compliant: true
wave_0_complete: false
created: 2026-03-31
---

# Phase 09 — Validation Strategy

> Деплойная фаза — unit-тесты отсутствуют. Верификация через shell smoke-команды и ручные браузерные шаги.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | none — деплойная фаза, нет unit-тестового фреймворка |
| **Config file** | none |
| **Quick run command** | `ls Build/WebGL/index.html 2>/dev/null && echo OK || echo MISSING` |
| **Full suite command** | см. Per-Task Verification Map ниже |
| **Estimated runtime** | ~10 секунд (все shell-проверки) |

---

## Sampling Rate

- **После каждой задачи:** Run quick run command
- **После каждой волны:** Run full suite (все shell-проверки из таблицы ниже)
- **Перед `/gsd:verify-work`:** Все shell-проверки green + ручная проверка WEBGL-07

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | Status |
|---------|------|------|-------------|-----------|-------------------|--------|
| 09-01-01 | 01 | 1 | WEBGL-01 | shell | `grep -c "Main.unity" ProjectSettings/EditorBuildSettings.asset` | ⬜ pending |
| 09-01-02 | 01 | 1 | WEBGL-02, WEBGL-03 | manual | Ручная сборка + `ls Build/WebGL/index.html` | ⬜ pending |
| 09-02-01 | 02 | 2 | WEBGL-04, WEBGL-05 | shell | `git show gh-pages:.nojekyll > /dev/null 2>&1 && echo OK` | ⬜ pending |
| 09-02-02 | 02 | 2 | WEBGL-06, WEBGL-07 | manual | Открыть URL в браузере, убедиться что игра запускается | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

Нет. Инфраструктура не требует установки — используются только `git` и `ls`.

*Existing infrastructure covers all phase requirements.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| WebGL платформа переключена в Unity | WEBGL-02 | Unity Editor операция | `File → Build Settings` → убедиться что WebGL выбран и кнопка `Switch Platform` недоступна (платформа уже активна) |
| Успешная сборка | WEBGL-03 | Unity Editor build process | После Build: `ls -la Build/WebGL/index.html Build/WebGL/Build/*.wasm` — файлы существуют |
| GitHub Pages включён | WEBGL-06 | GitHub UI настройка | `Settings → Pages → Source: gh-pages / (root)` → Status green |
| Игра работает в браузере | WEBGL-07 | Браузерный E2E | Открыть `https://<user>.github.io/<repo>/`, дождаться загрузки, управление кораблём работает |

---

## Validation Sign-Off

- [x] All tasks have automated verify или manual steps
- [x] Нет 3 consecutive tasks без верификации
- [x] Wave 0 не требуется — нет устанавливаемых зависимостей
- [x] Нет watch-mode флагов
- [x] Feedback latency < 30s для shell-команд
- [x] `nyquist_compliant: true` выставлен в frontmatter

**Approval:** pending
