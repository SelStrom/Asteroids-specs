# Спрайты игры Asteroids

## Добавление PNG-спрайтшита

1. Поместите PNG-файл спрайтшита в эту папку (`Assets/Media/sprites/`)
2. В Unity Inspector настройте Import Settings для PNG:
   - **Texture Type:** Sprite (2D and UI)
   - **Sprite Mode:** Multiple
   - **Filter Mode:** Point (No Filter)
   - **Compression:** None
3. Нажмите **Sprite Editor**, выберите режим нарезки:
   - **Automatic** — если спрайты разной ширины/высоты
   - **Grid by Cell Size** — если спрайты одинакового размера (укажите размер ячейки)
4. Нажмите **Slice**, затем **Apply**
5. Откройте `GameAtlas.spriteAtlas` в Inspector
6. Перетащите нарезанные спрайты (или всю папку) в секцию **Objects for Packing**
7. Нажмите **Pack Preview** для проверки

## Текущий статус

Ассет `GameAtlas.spriteAtlas` создан с настройками:
- Allow Rotation: false
- Padding: 4 px
- Generate Mip Maps: false
- Max Texture Size: 2048
- Format (WebGL): RGBA32

PNG-спрайтшит будет предоставлен пользователем. До его добавления атлас пустой.
