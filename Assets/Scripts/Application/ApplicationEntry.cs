using System;
using System.Collections;
using SelStrom.Asteroids.Configs;
using UnityEngine;

namespace SelStrom.Asteroids
{
    public class ApplicationEntry : MonoBehaviour, IApplicationComponent
    {
        [SerializeField] private GameData _configs;
        [SerializeField] private HudVisual _hudVisual;
        [SerializeField] private TitleScreenView _titleScreenView;
        [SerializeField] private GameObject _titleScreenGo;
        [SerializeField] private GameObject _hudGo;
        [SerializeField] private GameOverView _gameOverView;
        [SerializeField] private GameObject _gameOverGo;
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private LeaderboardView _leaderboardView;
        [SerializeField] private GameObject _leaderboardGo;

        public event Action<float> OnUpdate;
        public event Action OnPause;
        public event Action OnResume;

        private Application _application;

        // UgsService создаётся программно — не SerializeField (паттерн AudioManager, D-08)
        private UgsService _ugsService;

        private void Awake()
        {
            // Создать UgsService с LeaderboardId из конфига (LEAD-06)
            _ugsService = new UgsService(_configs != null ? _configs.LeaderboardId : "asteroids_highscores");

            _application = new Application();
            _application.Connect(this, _configs, _hudVisual, _titleScreenView, _titleScreenGo, _hudGo,
                _gameOverView, _gameOverGo,
                _audioManager, _leaderboardView, _leaderboardGo,
                _ugsService);
        }

        private void Start()
        {
            // LEAD-01: запустить guest sign-in до или параллельно со стартом игры
            StartCoroutine(InitUgsCoroutine());
            _application.Start();
        }

        private void Update()
        {
            OnUpdate?.Invoke(Time.deltaTime);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) { OnPause?.Invoke(); } else { OnResume?.Invoke(); }
        }

        private void OnDestroy()
        {
            _application?.Dispose();
        }

        // LEAD-01: инициализация UGS — анонимный вход
        private IEnumerator InitUgsCoroutine()
        {
            if (_ugsService == null) { yield break; }
            var task = _ugsService.InitializeAsync();
            // Ждём завершения без throw — обрабатываем ошибку graceful
            while (!task.IsCompleted)
            {
                yield return null;
            }
            if (task.IsFaulted)
            {
                // LEAD-05: UGS недоступен — кнопки лидерборда остаются выключены
                Debug.LogWarning("[UGS] Инициализация не удалась: " + task.Exception?.Message);
            }
            else
            {
                Debug.Log("[UGS] Инициализация успешна. Игрок залогинен анонимно.");
            }
        }

        // D-09: Coroutine-обёртка для Task (паттерн из RESEARCH.md)
        public static IEnumerator RunAsync(System.Threading.Tasks.Task task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }
            if (task.IsFaulted && task.Exception != null)
            {
                throw task.Exception.InnerException ?? task.Exception;
            }
        }
    }
}
