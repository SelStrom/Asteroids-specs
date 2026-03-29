using System.Collections;
using SelStrom.Asteroids.Configs;
using UnityEngine;

namespace SelStrom.Asteroids
{
    public class Application
    {
        private IApplicationComponent _entry;
        private GameData _configs;
        private Model _model;
        private EntitiesCatalog _catalog;
        private Game _game;
        private PlayerInput _input;
        private GameObjectPool _pool;
        private ModelFactory _modelFactory;

        private HudVisual _hudVisual;
        private TitleScreenView _titleScreenView;
        private GameObject _titleScreenGo;
        private GameObject _hudGo;

        private GameOverView _gameOverView;
        private GameObject _gameOverGo;
        private GameOverScreen _gameOverScreen;
        private GameScreen _gameScreen;

        private AudioManager _audioManager;
        private LeaderboardScreen _leaderboardScreen;
        private LeaderboardView _leaderboardView;
        private GameObject _leaderboardGo;

        // Phase 7: UGS-сервис передаётся из ApplicationEntry (D-08)
        private UgsService _ugsService;

        // Свойства для RuntimeBridgeProxy — делегируют к _game (per D-07 из CONTEXT.md)
        public int Score => _game?.Score ?? 0;
        public int Wave => _game?.Wave ?? 0;
        public int Lives => _game?.Lives ?? 0;
        public bool IsRunning => _game?.IsRunning ?? false;

        public void Connect(IApplicationComponent entry, GameData configs, HudVisual hudVisual,
            TitleScreenView titleScreenView, GameObject titleScreenGo, GameObject hudGo,
            GameOverView gameOverView, GameObject gameOverGo,
            AudioManager audioManager = null,
            LeaderboardView leaderboardView = null, GameObject leaderboardGo = null,
            UgsService ugsService = null)
        {
            _entry = entry;
            _configs = configs;
            _hudVisual = hudVisual;
            _titleScreenView = titleScreenView;
            _titleScreenGo = titleScreenGo;
            _hudGo = hudGo;
            _gameOverView = gameOverView;
            _gameOverGo = gameOverGo;
            _audioManager = audioManager;
            _leaderboardView = leaderboardView;
            _leaderboardGo = leaderboardGo;
            _ugsService = ugsService;

            _entry.OnUpdate += OnUpdate;

            // TitleScreen экран — Phase 7: передаём OnLeaderboardOpen callback
            var titleScreen = new TitleScreen();
            titleScreen.Connect(_titleScreenView, OnGameStart, OnLeaderboardOpen);

            // GameOver экран — скрыт при старте
            if (_gameOverGo != null) { _gameOverGo.SetActive(false); }
            _gameOverScreen = new GameOverScreen();
            if (_gameOverView != null)
            {
                _gameOverScreen.Connect(_gameOverView, OnPlayAgain);
            }

            // LeaderboardScreen — скрыт при старте
            if (_leaderboardGo != null) { _leaderboardGo.SetActive(false); }
            _leaderboardScreen = new LeaderboardScreen();
            if (_leaderboardView != null)
            {
                _leaderboardScreen.Connect(_leaderboardView, OnLeaderboardBack);
            }
        }

        public void Start()
        {
            // Вычислить размер игровой области по камере
            var cam = Camera.main;
            var height = cam.orthographicSize * 2f; // 22.5 * 2 = 45
            var width = height * cam.aspect;         // 45 * (16/9) ≈ 80
            var gameArea = new Vector2(width, height);

            // Создать слои
            _pool = new GameObjectPool();
            _model = new Model();
            _model.GameArea = gameArea;
            _model.GetSystem<MoveSystem>().SetGameArea(gameArea);

            _modelFactory = new ModelFactory();
            _modelFactory.Connect(_model);

            _catalog = new EntitiesCatalog();
            _catalog.Connect(_configs, _modelFactory, _pool);

            _input = new PlayerInput();
            _input.Connect();

            _game = new Game();

            _gameScreen = new GameScreen();
        }

        private void OnGameStart()
        {
            if (_titleScreenGo != null) { _titleScreenGo.SetActive(false); }
            if (_hudGo != null) { _hudGo.SetActive(true); }

            // Подключить GameScreen к HUD до старта — иначе начальный onScoreChanged теряется
            _gameScreen.Connect(_hudVisual, null);

            // Подключить Game с callback для HUD и запустить
            _game.Connect(_configs, _catalog, _model, _input, OnGameOver, OnScoreChanged,
                onWaveBannerShow: wave => _hudVisual?.ShowWaveBanner(wave),
                onWaveBannerHide: () => _hudVisual?.HideWaveBanner(),
                onThrust: isActive => _audioManager?.PlayThrust(isActive),
                onShoot: () => _audioManager?.PlayShoot(),
                onExplodeShip: () => _audioManager?.PlayExplodeShip(),
                onExplodeAsteroid: size => _audioManager?.PlayExplodeAsteroid(size),
                onUfoTone: active => _audioManager?.SetUfoTone(active),
                onAsteroidCount: count => _audioManager?.SetAsteroidCount(count));

            if (_audioManager != null)
            {
                _audioManager.StopAll(); // сброс состояния при повторном старте
                _audioManager.SetAsteroidCount(12);
                _audioManager.StartBeat();
            }

            _game.Start();

            // Переподключить HUD с реальным ShipViewModel (создаётся внутри Start)
            _gameScreen.Connect(_hudVisual, _game.ShipViewModel);
        }

        private void OnGameOver()
        {
            if (_audioManager != null)
            {
                _audioManager.StopBeat();  // AUD-06: пульс стоп
                _audioManager.StopAll();   // AUD-02/05: тяга/UFO стоп (Pitfall 4)
            }

            // Показать Game Over экран
            if (_gameOverGo != null) { _gameOverGo.SetActive(true); }

            // D-02: предзаполнить имя из PlayerPrefs
            var playerName = PlayerPrefs.GetString("PlayerName", "");

            // LEAD-02: расширенный Show с именем и callbacks
            _gameOverScreen.Show(_game.Score, _game.HighScore, playerName,
                onSubmitScore: (name) => _entry.StartCoroutine(SubmitScoreCoroutine(name, _game.Score)),
                onLeaderboard: OnLeaderboardOpen);
        }

        // LEAD-03, LEAD-04: открыть LeaderboardScreen с реальными данными UGS
        private void OnLeaderboardOpen()
        {
            // Скрыть GameOver и TitleScreen если открыты
            if (_gameOverGo != null) { _gameOverGo.SetActive(false); }
            if (_titleScreenGo != null) { _titleScreenGo.SetActive(false); }

            // Показать LeaderboardScreen с реальными данными
            if (_leaderboardGo != null) { _leaderboardGo.SetActive(true); }
            if (_ugsService != null)
            {
                _entry.StartCoroutine(_leaderboardScreen.ShowWithDataCoroutine(_ugsService));
            }
            else
            {
                // UGS недоступен — показать пустой экран
                _leaderboardScreen.Show();
            }
        }

        // LEAD-02: отправка счёта через Coroutine-обёртку (D-09)
        private IEnumerator SubmitScoreCoroutine(string playerName, int score)
        {
            if (_ugsService == null)
            {
                _gameOverScreen.NotifySubmitError("UGS сервис недоступен.");
                yield break;
            }

            // D-02: сохранить имя в PlayerPrefs для следующего запуска
            if (!string.IsNullOrEmpty(playerName))
            {
                PlayerPrefs.SetString("PlayerName", playerName);
            }

            var task = _ugsService.SubmitScoreAsync(playerName, score);
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                // LEAD-05: показать ошибку в UI
                _gameOverScreen.NotifySubmitError("Ошибка отправки. Проверьте соединение.");
                Debug.LogWarning("[Application] Ошибка submit score: " + task.Exception?.Message);
            }
            else
            {
                // D-03: успех — отключить кнопку, перейти к LeaderboardScreen
                _gameOverScreen.NotifySubmitSuccess();
                // Краткая пауза для визуальной обратной связи
                yield return new WaitForSeconds(0.5f);
                OnLeaderboardOpen();
            }
        }

        private void OnPlayAgain()
        {
            // Скрыть Game Over экран (D-12)
            if (_gameOverGo != null) { _gameOverGo.SetActive(false); }
            _gameOverScreen.Hide();

            _audioManager?.StopAll(); // Pitfall 2: thrust loop стоп при Restart

            // Сбросить и перезапустить игру
            _game.Restart();

            // Показать HUD
            if (_hudGo != null) { _hudGo.SetActive(true); }
            _gameScreen.UpdateHud(0, 3, _game.HighScore);
        }

        private void OnScoreChanged(int score, int lives)
        {
            _gameScreen.UpdateHud(score, lives, _game.HighScore);
        }

        private void OnLeaderboardBack()
        {
            _leaderboardScreen?.Hide();
            if (_leaderboardGo != null) { _leaderboardGo.SetActive(false); }
            if (_titleScreenGo != null) { _titleScreenGo.SetActive(true); }
        }

        private void OnUpdate(float deltaTime)
        {
            _model?.Update(deltaTime);
            _game?.Update(deltaTime);
        }

        public void Dispose()
        {
            _entry.OnUpdate -= OnUpdate;
            _game?.Dispose();
            _input?.Dispose();
            _catalog?.Dispose();
            _model = null;
        }
    }
}
