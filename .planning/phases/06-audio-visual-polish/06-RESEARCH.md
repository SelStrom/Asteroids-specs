# Phase 6: Audio & Visual Polish - Research

**Researched:** 2026-03-28
**Domain:** Unity 2022.3 — AudioSource/AudioClip, ParticleSystem, Sprite Animation, UI Canvas
**Confidence:** HIGH

---

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
Нет явно заблокированных решений — discuss phase пропущен.

### Claude's Discretion
Все решения по реализации остаются на усмотрение Claude. Ориентиры — цели фазы, критерии успеха и конвенции кодебазы.

### Deferred Ideas (OUT OF SCOPE)
Нет — discuss phase пропущен.
</user_constraints>

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| VIS-01 | Все игровые объекты используют спрайты из текстурного атласа | Спрайты уже нарезаны в asteroids.png; атлас GameAtlas.spriteAtlas создан; нужно убедиться что все prefab ссылаются на sub-sprites, а не на текстуру целиком |
| VIS-02 | Фон — чёрный, без текстур | Camera.backgroundColor = black; уже используется Built-in RP без скайбокса |
| VIS-03 | Взрыв корабля: particle effect или анимация из спрайтов (минимум 4 фрейма) | Только один взрывной спрайт bullet_particle найден в атласе; нужно либо создать фреймы анимации вручную (4+ PNG/sub-sprite) либо использовать ParticleSystem с texture sheet; GameData.VfxBlowPrefab уже определён как поле |
| VIS-04 | Взрывы астероидов: particle effect соответствующего размера | ParticleSystem с масштабом по размеру астероида (size 3/2/1 → startSize 2x/1.5x/1x) |
| VIS-05 | HUD: счёт (верх-лево), жизни-иконки (верх-лево под счётом), High Score (верх-право) | HudVisual уже реализован в Phase 4 с этими полями; нужно убедиться что _scoreText, _highScoreText, _livesContainer, _lifeIconSprite назначены (Phase4Setup их создавал) |
| VIS-06 | Main Menu: название игры, кнопка Play, кнопка Leaderboard | TitleScreenView и TitleScreen уже существуют, но имеют только кнопку Play; нужно добавить название игры и кнопку Leaderboard (disabled до Phase 7) |
| VIS-07 | Game Over экран: финальный счёт, кнопка Submit Score, кнопка Play Again, кнопка Leaderboard | GameOverView и GameOverScreen уже реализованы в Phase 4 с этими кнопками (Submit и Leaderboard отключены — D-11) |
| VIS-08 | Leaderboard экран: Top-10 список, позиция игрока, кнопка Back | Экран не существует; нужно создать LeaderboardScreen / LeaderboardView; данные будут заглушками до Phase 7 |
| AUD-01 | Звук выстрела корабля | AudioSource.PlayOneShot; нужен AudioClip; вызов из Game.OnUserGunShooting |
| AUD-02 | Звук тяги корабля (зациклен пока зажата кнопка тяги) | AudioSource.loop=true с Play/Stop по IsThrusting; отдельный AudioSource на ShipVisual или GameManager |
| AUD-03 | Звук взрыва корабля | AudioSource.PlayOneShot; вызов при ShipModel.Kill() |
| AUD-04 | Звуки взрывов астероидов: Large, Medium, Small | 3 отдельных AudioClip; PlayOneShot в OnEntityDestroyed при AsteroidModel |
| AUD-05 | Звук UFO (повторяющийся тон пока UFO на экране) | AudioSource.loop=true; включить при CreateUfo, выключить при Kill/Exit |
| AUD-06 | Фоновый пульс из двух нот; темп нарастает при убывании астероидов | Coroutine или ActionScheduler с двумя AudioClip (low beat / high beat); период сокращается от ~1.0s до ~0.25s по числу астероидов |
| AUD-07 | Все звуки через AudioClip + AudioSource (пул или GameManager) | AudioManager MonoBehaviour с набором AudioSource-компонентов; один GameObject в сцене |
</phase_requirements>

---

## Summary

Phase 6 добавляет аудио и визуальную полировку к уже работающей игровой механике (Phases 2–5). Кодовая база построена на паттерне MVVM через пакет `com.shtl.mvvm` — View-компоненты реагируют на ReactiveValue из ViewModel. Для аудио наиболее чистое решение — отдельный `AudioManager` MonoBehaviour в сцене, который принимает события через Actions (callback-driven), а не опрашивает состояние в Update. Для визуальных взрывов единственный существующий спрайт `bullet_particle` в атласе не достаточен для 4+ фреймов; нужны либо дополнительные sub-sprites в PNG, либо ParticleSystem с минимальной художественной конфигурацией.

Важный контекст: в проекте уже есть `EffectVisual` — пустая заглушка класса. `GameData.VfxBlowPrefab` поле существует и равно null с Phase 3 (решение D-06: VfxBlowPrefab — backlog Phase 6). Phase 6 должна реализовать эти заглушки. HUD, GameOverScreen и TitleScreen уже функциональны — в Phase 6 нужно только добавить недостающие элементы (title text в TitleScreen, Leaderboard stub-экран).

**Первичная рекомендация:** Реализовать AudioManager как отдельный MonoBehaviour с именованными AudioSource-полями; взрывы через ParticleSystem с масштабом; фоновый пульс через IEnumerator корутину с динамическим интервалом.

---

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| UnityEngine.Audio | встроен в Unity 2022.3 | AudioSource, AudioClip, AudioMixer | Нативный API, WebGL-совместим |
| UnityEngine.ParticleSystem | встроен в Unity 2022.3 | Particle effects для взрывов | Нет зависимостей, встроен в Built-in RP |
| UnityEngine.UI + TMPro | встроен / com.unity.textmeshpro | Canvas UI для экранов | Уже используется в проекте |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| com.shtl.mvvm | c7bda1c (git hash) | ReactiveValue, AbstractWidgetView | Все View компоненты — уже в проекте |
| UnityEngine.Animation | встроен | Sprite sheet animation через Animator | Только если нужна анимация корабля из спрайтов; альтернатива ParticleSystem |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| ParticleSystem для взрывов | Sprite Animation (Animator + AnimationClip) | ParticleSystem проще масштабировать; Animation удобнее если уже есть спрайты взрывов |
| AudioManager singleton | AudioSource на каждом объекте | Centralised Manager проще контролировать; но требует передачи callback-ов |
| IEnumerator-корутина для пульса | ActionScheduler (уже в проекте) | ActionScheduler — простой рекурсивный schedule; корутина для loop читабельнее |

---

## Architecture Patterns

### Recommended Project Structure
```
Assets/
├── Scripts/
│   ├── Application/
│   │   └── AudioManager.cs          # новый: MonoBehaviour, все AudioSource
│   ├── View/
│   │   ├── EffectVisual.cs          # расширить: ParticleSystem управление
│   │   ├── TitleScreenView.cs       # расширить: добавить title text и Leaderboard кнопку
│   │   ├── LeaderboardView.cs       # новый: stub экран лидерборда
│   │   └── (остальное без изменений)
│   └── Application/
│       ├── Screens/
│       │   └── LeaderboardScreen.cs # новый: stub экран
│       └── Application.cs           # расширить: подключить AudioManager и LeaderboardScreen
├── Media/
│   ├── audio/                       # новая папка: AudioClip .wav/.ogg файлы
│   └── configs/
│       └── AudioData.asset          # новый: ScriptableObject для AudioClip ссылок
```

### Pattern 1: AudioManager с callback-подпиской
**Что:** MonoBehaviour в сцене с полями AudioSource (shoot, thrust, explodeShip, explodeAsteroidBig/Med/Small, ufoTone, beatLow, beatHigh). Принимает события через public Actions.
**Когда использовать:** Всегда в этом проекте — централизованный контроль, нет дублирования AudioSource на каждом prefab.

```csharp
// AudioManager.cs
public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource _shoot;
    [SerializeField] private AudioSource _thrust;
    [SerializeField] private AudioSource _explodeShip;
    [SerializeField] private AudioSource _explodeAsteroidBig;
    [SerializeField] private AudioSource _explodeAsteroidMedium;
    [SerializeField] private AudioSource _explodeAsteroidSmall;
    [SerializeField] private AudioSource _ufoTone;
    [SerializeField] private AudioSource _beatLow;
    [SerializeField] private AudioSource _beatHigh;

    public void PlayShoot() => _shoot.PlayOneShot(_shoot.clip);
    public void PlayThrust(bool isActive)
    {
        if (isActive && !_thrust.isPlaying) { _thrust.Play(); }
        else if (!isActive && _thrust.isPlaying) { _thrust.Stop(); }
    }
    public void PlayExplodeShip() => _explodeShip.PlayOneShot(_explodeShip.clip);
    public void PlayExplodeAsteroid(int size)
    {
        var src = size == 3 ? _explodeAsteroidBig
                : size == 2 ? _explodeAsteroidMedium
                : _explodeAsteroidSmall;
        src.PlayOneShot(src.clip);
    }
    public void SetUfoTone(bool active)
    {
        if (active && !_ufoTone.isPlaying) { _ufoTone.Play(); }
        else if (!active) { _ufoTone.Stop(); }
    }
    // ... пульс реализован ниже
}
```

### Pattern 2: Фоновый пульс через корутину
**Что:** IEnumerator чередует beat_low и beat_high с интервалом, вычисленным из числа астероидов. Game.cs через callback обновляет `_asteroidCount` — AudioManager использует его для расчёта темпа.
**Когда использовать:** AUD-06.

```csharp
// В AudioManager: корутина пульса
private int _asteroidCount = 12;

public void SetAsteroidCount(int count) => _asteroidCount = Mathf.Max(count, 0);

public void StartBeat() => StartCoroutine(BeatLoop());

private IEnumerator BeatLoop()
{
    var useLow = true;
    while (true)
    {
        // Интервал: от 1.0s (12 астероидов) до 0.25s (0 астероидов)
        var t = _asteroidCount / 12f; // 0..1
        var interval = Mathf.Lerp(0.25f, 1.0f, t);
        var src = useLow ? _beatLow : _beatHigh;
        src.PlayOneShot(src.clip);
        useLow = !useLow;
        yield return new WaitForSeconds(interval);
    }
}
```

### Pattern 3: ParticleSystem для взрывов (EffectVisual)
**Что:** EffectVisual расширяется — добавляется SerializedField для ParticleSystem; метод Play(float scale); возврат в пул по событию OnParticleSystemStopped.
**Когда использовать:** VIS-03, VIS-04.

```csharp
// EffectVisual.cs (расширить)
public class EffectVisual : MonoBehaviour, IEntityView
{
    [SerializeField] private ParticleSystem _particles;
    private int _prefabInstanceId;

    public int PrefabInstanceId => _prefabInstanceId;
    public void SetPrefabId(int id) { _prefabInstanceId = id; }

    public void Play(Vector2 position, float scale = 1f)
    {
        transform.position = new Vector3(position.x, position.y, 0f);
        var main = _particles.main;
        main.startSizeMultiplier = scale;
        _particles.Play();
    }
}
```

### Pattern 4: SetupPhase6 Editor Script
**Что:** Следуем конвенции проекта — Phase6Setup.cs в Assets/Editor/ с `[MenuItem("Asteroids/Setup Phase 6 Assets")]`. Создаёт AudioData.asset, настраивает AudioManager в сцене, добавляет LeaderboardScreen в Canvas.
**Когда использовать:** Всегда — все предыдущие фазы использовали этот подход.

### Anti-Patterns to Avoid
- **AudioSource на каждом prefab астероида/пули:** GameObject pool возвращает объекты и AudioSource может играть после deactivation. Используй централизованный AudioManager.
- **PlayOneShot без null-проверки clip:** В WebGL AudioClip может быть null если не назначен. Добавь `if (src.clip != null)` защиту.
- **Animator для одиночного взрыва:** Создание AnimatorController только для 1 animation clip — избыточно. Используй ParticleSystem или простую корутину с SpriteRenderer.
- **FindObjectOfType в Update:** AudioManager должен быть передан через Connect() или SerializedField в ApplicationEntry, а не искаться в Runtime.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Временной пульс с нарастанием | Кастомный таймер в Update | Coroutine + WaitForSeconds | Уже работает, WebGL-совместим |
| Проигрывание одного звука | Кастомный audio mixer | AudioSource.PlayOneShot | API для этого и создан |
| Particle effect масштаб | Ручная генерация частиц | ParticleSystem.main.startSizeMultiplier | Одна строка кода |
| UI layout кнопок | Ручной RectTransform | VerticalLayoutGroup + spacing | Меньше кода, адаптивно |

**Key insight:** Unity Audio API в 2022.3 полностью покрывает все нужды этой фазы без дополнительных пакетов.

---

## Common Pitfalls

### Pitfall 1: AudioClip не назначен — нет звука без ошибки
**Что идёт не так:** PlayOneShot(null) тихо ничего не делает в WebGL. В Editor тоже нет exception.
**Почему:** Unity не бросает исключение при null clip.
**Как избежать:** В Phase6Setup.cs проверить назначение clip для каждого AudioSource и залоггировать предупреждение. В AudioManager добавить guard: `if (src == null || src.clip == null) { Debug.LogWarning(...); return; }`.
**Признаки:** Тишина при событии без ошибок в Console.

### Pitfall 2: Thrust AudioSource продолжает играть после Restart
**Что идёт не так:** При Game.Restart() создаётся новый ShipModel, но loop-AudioSource может быть не остановлен если был активен в момент Restart.
**Почему:** AudioManager не получает Stop-событие при Restart если ship уничтожается через Kill без отдельного callback.
**Как избежать:** В AudioManager добавить метод `StopAll()` / `ResetState()`; вызывать из Application.OnPlayAgain перед Game.Restart().

### Pitfall 3: ParticleSystem возвращается в пул до завершения эффекта
**Что идёт не так:** EffectVisual возвращается в pool немедленно после вызова — particles исчезают мгновенно.
**Почему:** GameObject.SetActive(false) останавливает ParticleSystem немедленно.
**Как избежать:** Возвращать EffectVisual в пул только в OnParticleSystemStopped callback (или через ActionScheduler с задержкой равной duration частиц). Добавить флаг IsPendingReturn.

### Pitfall 4: Beat корутина не останавливается при Game Over
**Что идёт не так:** При Game Over (game.Stop()) beat корутина в AudioManager продолжает играть на экране Game Over.
**Почему:** Корутина привязана к AudioManager GameObject (который не деактивируется), а не к Game.
**Как избежать:** AudioManager.StopBeat() должен вызываться через callback из Application.OnGameOver.

### Pitfall 5: SetActive(false) на неактивных объектах при поиске
**Что идёт не так:** В Phase6Setup.cs `GameObject.Find("UI")` не найдёт неактивный Canvas.
**Почему:** GameObject.Find() не находит неактивные объекты (задокументировано в skill unity-ui).
**Как избежать:** Использовать `Resources.FindObjectsOfTypeAll<T>()` с фильтром по scene — как это уже делается в Phase4Setup.cs и Phase5Setup.cs.

### Pitfall 6: UFO tone не отключается при UFO exit (без Kill)
**Что идёт не так:** UFO уходит за экран (UFO-05) через `_activeUfo.Kill()` — это обрабатывается в OnEntityDestroyed. Но нужно убедиться что AudioManager.SetUfoTone(false) вызывается в обоих случаях: Kill() и CheckUfoExit().
**Как избежать:** Привязать SetUfoTone(false) к единой точке OnEntityDestroyed при UfoBigModel.

---

## Code Examples

### AudioData ScriptableObject
```csharp
// Source: конвенция проекта (GameData.cs, AsteroidData.cs)
[CreateAssetMenu(menuName = "Audio data")]
public class AudioData : ScriptableObject
{
    public AudioClip Shoot;
    public AudioClip Thrust;
    public AudioClip ExplodeShip;
    public AudioClip ExplodeAsteroidBig;
    public AudioClip ExplodeAsteroidMedium;
    public AudioClip ExplodeAsteroidSmall;
    public AudioClip UfoTone;
    public AudioClip BeatLow;
    public AudioClip BeatHigh;
}
```

### Подключение AudioManager к Game через Application.cs
```csharp
// В Application.cs — расширить OnGameStart()
private void OnGameStart()
{
    // ... существующий код ...
    _audioManager.StartBeat();
    // Подключить callbacks к Game через новый параметр или через events
}

private void OnGameOver()
{
    _audioManager.StopBeat();
    _audioManager.StopAll();
    // ... существующий код ...
}
```

### LeaderboardView — stub до Phase 7
```csharp
// Assets/Scripts/View/LeaderboardView.cs
public class LeaderboardViewModel : AbstractViewModel
{
    public Action OnBackClicked;
    // Top-10 данные — заглушка до Phase 7
    public string[] EntryNames = new string[0];
    public int[] EntryScores = new int[0];
}

public class LeaderboardView : AbstractWidgetView<LeaderboardViewModel>
{
    [SerializeField] private Button _backButton;
    [SerializeField] private Transform _entriesContainer;

    protected override void OnConnected()
    {
        if (_backButton != null)
        {
            _backButton.onClick.AddListener(() => ViewModel?.OnBackClicked?.Invoke());
        }
        // В Phase 7: заполнить _entriesContainer из ViewModel.EntryNames/Scores
    }

    protected override void OnDisposed()
    {
        if (_backButton != null) { _backButton.onClick.RemoveAllListeners(); }
    }
}
```

---

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| AudioSource на каждом GameObject | Централизованный AudioManager | — | Проще контроль над пулами и Restart |
| Animator для простых анимаций взрыва | ParticleSystem или простая корутина | Unity 5+ | Меньше assets, гибче масштабирование |

**Deprecated/outdated:**
- `AudioSource.Play()` без clip проверки: всегда добавлять null guard в 2022.3.

---

## Existing Codebase Analysis

### Что уже реализовано (не нужно делать заново)
- `HudVisual` — полная реализация Score, HighScore, LivesIcons, WaveBanner (Phase 4)
- `GameOverView` / `GameOverScreen` — полная реализация с кнопками Play Again, Submit Score (disabled), Leaderboard (disabled) (Phase 4)
- `TitleScreenView` / `TitleScreen` — базовая реализация с кнопкой Play
- `EffectVisual` — пустая заглушка, ожидает Phase 6
- `GameData.VfxBlowPrefab` — поле существует, равно null, ожидает Phase 6
- Все спрайты в `asteroids.png`: asteroid_big_1/2/3, asteroid_medium_1/2/3, asteroid_small_1/2/3, ship_idle, ship_throttle, bullet, bullet_particle, ufo_big

### Что нужно добавить/изменить в Phase 6
1. **AudioManager.cs** — новый MonoBehaviour
2. **AudioData.asset** — новый ScriptableObject с AudioClip полями
3. **EffectVisual.cs** — расширить ParticleSystem логикой
4. **GameData.cs** — добавить поле `AudioData Audio`
5. **ApplicationEntry.cs** — добавить SerializedField `AudioManager _audioManager`
6. **Application.cs** — подключить AudioManager callbacks к Game events
7. **Game.cs** — добавить callbacks для audio events (shoot, explode, ufo start/stop, asteroid count change)
8. **TitleScreenView.cs** — добавить title text и кнопку Leaderboard (disabled)
9. **LeaderboardView.cs** / **LeaderboardScreen.cs** — новые stub классы
10. **Phase6Setup.cs** — Editor script для создания AudioManager в сцене и assets

### Спрайты для взрывов
В текущем атласе есть только `bullet_particle` — один спрайт. Для VIS-03 (минимум 4 фрейма взрыва корабля) есть два варианта:
- **Вариант A (рекомендуется):** ParticleSystem с `bullet_particle` как текстурой — visually достаточно для аутентичного стиля Asteroids (разлёт частиц).
- **Вариант B:** Если пользователь предоставит дополнительные спрайты взрыва — использовать Animator с AnimationClip.

**Решение Phase 6:** Использовать ParticleSystem для обоих типов взрывов (VIS-03, VIS-04). Масштаб ParticleSystem пропорционален размеру астероида. Это соответствует "particle effect" из требований и не требует дополнительных художественных ресурсов.

---

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| Unity 2022.3 | Все | ✓ | 2022.3.60f1 | — |
| com.shtl.mvvm | View layer | ✓ | git:c7bda1c | — |
| com.unity.textmeshpro | UI текст | ✓ | встроен | — |
| AudioClip assets (.wav/.ogg) | AUD-01..07 | ✗ | — | Синтез через AudioClip.Create() или placeholder тишина |
| ParticleSystem | VIS-03, VIS-04 | ✓ | встроен в Built-in RP | — |

**Missing dependencies с fallback:**
- AudioClip файлы (звуки): не обнаружены в репозитории (`Assets/Media/audio/` отсутствует). Phase6Setup должен создать AudioData.asset со всеми полями, оставив clip поля null — игра запустится без звука. Реальные AudioClip будут назначены пользователем позже в Inspector. Код должен иметь null guard чтобы не крашиться при отсутствии clips.

---

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | Ручное тестирование (Play Mode) — нет автотестов в проекте |
| Config file | Нет (нет pytest.ini, jest.config и т.п.) |
| Quick run command | `Asteroids/Setup Phase 6 Assets` (MenuItem в Unity Editor) → Play Mode |
| Full suite command | MCP: compile → play → stop → проверить Console |

### Phase Requirements → Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| VIS-01 | Все объекты используют атлас | visual/manual | MCP: play, наблюдать спрайты | ✓ атлас готов |
| VIS-02 | Чёрный фон | visual/manual | MCP: play, наблюдать фон | ✓ |
| VIS-03 | Взрыв корабля 4+ фреймов | visual/manual | MCP: play, врезаться в астероид | ❌ Wave 0 |
| VIS-04 | Взрывы астероидов по размеру | visual/manual | MCP: play, стрелять в астероиды | ❌ Wave 0 |
| VIS-05 | HUD layout | visual/manual | MCP: play, проверить HUD | ✓ Phase 4 |
| VIS-06 | Main Menu layout | visual/manual | MCP: play, TitleScreen | ✓ частично |
| VIS-07 | Game Over экран | visual/manual | MCP: play до Game Over | ✓ Phase 4 |
| VIS-08 | Leaderboard экран | visual/manual | MCP: play, нажать Leaderboard | ❌ Wave 0 |
| AUD-01 | Звук выстрела | audio/manual | MCP: play, нажать Space | ❌ Wave 0 |
| AUD-02 | Звук тяги (loop) | audio/manual | MCP: play, зажать W | ❌ Wave 0 |
| AUD-03 | Взрыв корабля | audio/manual | MCP: play, погибнуть | ❌ Wave 0 |
| AUD-04 | Взрывы астероидов x3 | audio/manual | MCP: play, стрелять | ❌ Wave 0 |
| AUD-05 | UFO тон (loop) | audio/manual | MCP: play, ждать UFO | ❌ Wave 0 |
| AUD-06 | Фоновый пульс с нарастанием | audio/manual | MCP: play, уничтожать астероиды | ❌ Wave 0 |
| AUD-07 | Все через AudioSource | code review | compile | ❌ Wave 0 |

### Wave 0 Gaps
- [ ] `Assets/Scripts/Application/AudioManager.cs` — реализация всех AUD-01..07
- [ ] `Assets/Scripts/Configs/AudioData.cs` — ScriptableObject для AudioClip ссылок
- [ ] `Assets/Scripts/View/LeaderboardView.cs` — stub для VIS-08
- [ ] `Assets/Scripts/Application/Screens/LeaderboardScreen.cs` — stub
- [ ] `Assets/Editor/Phase6Setup.cs` — setup script

---

## Open Questions

1. **Наличие аудио ассетов**
   - Что известно: в репозитории нет ни одного AudioClip файла
   - Неясно: пользователь предоставит .wav/.ogg или нужно синтезировать простые звуки программно (AudioClip.Create с синусоидой)?
   - Рекомендация: Реализовать AudioManager с null-safe guard; в Phase6Setup создать AudioData.asset с пустыми полями; пользователь назначит clips вручную. Опционально — создать простые синтетические clips для тестирования.

2. **Взрыв корабля: требование "минимум 4 фрейма" (VIS-03)**
   - Что известно: в атласе только bullet_particle спрайт для частиц
   - Неясно: достаточно ли визуально ParticleSystem с одним спрайтом для выполнения требования VIS-03?
   - Рекомендация: Считать ParticleSystem с `burst emission` (несколько частиц одновременно с разными скоростями) эквивалентом "4+ фреймовой анимации". Требование говорит "particle effect ИЛИ анимация из спрайтов".

---

## Sources

### Primary (HIGH confidence)
- Unity 2022.3 built-in API — AudioSource, ParticleSystem, UnityEngine.UI (официальная документация)
- Кодовая база проекта (прямое чтение файлов) — Phase4Setup.cs, Phase5Setup.cs, Game.cs, EntitiesCatalog.cs, ApplicationEntry.cs

### Secondary (MEDIUM confidence)
- Паттерн AudioManager — стандартная конвенция Unity для centralized audio, задокументирована в официальных Unity tutorials

### Tertiary (LOW confidence)
- Нет

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — Unity 2022.3 built-in API, no external packages needed
- Architecture: HIGH — основан на прямом анализе кодовой базы и конвенций предыдущих фаз
- Pitfalls: HIGH — основан на реальных паттернах кодовой базы (pool, restart, SetActive)

**Research date:** 2026-03-28
**Valid until:** 2026-04-28 (стабильный стек)
