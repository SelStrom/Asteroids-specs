using System;
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

        public event Action<float> OnUpdate;
        public event Action OnPause;
        public event Action OnResume;

        private Application _application;

        private void Awake()
        {
            _application = new Application();
            _application.Connect(this, _configs, _hudVisual, _titleScreenView, _titleScreenGo, _hudGo,
                _gameOverView, _gameOverGo);
        }

        private void Start()
        {
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
    }
}
