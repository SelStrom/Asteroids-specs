---
id: 260328-jv0
status: complete
date: 2026-03-28
commits:
  - 2c39de0
  - 6b3d110
---

# Quick Task 260328-jv0: Rescale world for PPU=16

## What was done

### Task 1: Collider sizes ×6.25 for PPU=16

После смены PPU 100→16 спрайты стали в 6.25x больше. Коллайдеры пересчитаны:

| Префаб | Было | Стало |
|--------|------|-------|
| asteroid_big (CircleCollider2D) | radius 0.4 | radius 2.5 |
| asteroid_medium (CircleCollider2D) | radius 0.22 | radius 1.375 |
| asteroid_small (CircleCollider2D) | radius 0.15 | radius 0.9375 |
| ship (PolygonCollider2D) | {0,1},{-0.5,-0.5},{0.5,-0.5} | {0,6.25},{-3.125,-3.125},{3.125,-3.125} |

Также исправлен Phase4Setup.cs чтобы при следующем запуске Setup не сбрасывал радиусы обратно.

### Task 2: Canvas + HUD layout

**Canvas:** `m_RenderMode: 1` (Screen Space Camera) → `m_RenderMode: 0` (Screen Space Overlay).
Убрана зависимость от камеры — Canvas всегда покрывает экран.

**HUD RectTransform:** pivot `(0.5, 0.5)` → `(0, 1)`.
С anchor=(0,1) и position=(16,-16) HUD теперь корректно находится в top-left углу.

**HUD children (5 debug-текстов):** anchor x=0 → x=0.5 — элементы центрированы в HUD.

**Phase4Setup** пересоздал score/lives UI в HUD (`_scoreText`, `_livesContainer`, `_lifeIconSprite`).

## Commits

- `2c39de0` — fix(quick-260328-jv0): коллайдеры ×6.25 под PPU=16 + Phase4Setup с правильными радиусами
- `6b3d110` — fix(quick-260328-jv0): Canvas→Overlay, HUD pivot (0,1), children centre-anchor
