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

        public void Connect(IApplicationComponent entry, GameData configs, HudVisual hudVisual,
            TitleScreenView titleScreenView, GameObject titleScreenGo, GameObject hudGo)
        {
            _entry = entry;
            _configs = configs;
            _hudVisual = hudVisual;
            _titleScreenView = titleScreenView;
            _titleScreenGo = titleScreenGo;
            _hudGo = hudGo;

            _entry.OnUpdate += OnUpdate;

            // TitleScreen экран
            var titleScreen = new TitleScreen();
            titleScreen.Connect(_titleScreenView, OnGameStart);
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
            _model.GetSystem<MoveSystem>().SetGameArea(gameArea);

            _modelFactory = new ModelFactory();
            _modelFactory.Connect(_model);

            _catalog = new EntitiesCatalog();
            _catalog.Connect(_configs, _modelFactory, _pool);

            _input = new PlayerInput();
            _input.Connect();

            _game = new Game();
            _game.Connect(_configs, _catalog, _model, _input, OnGameOver);
        }

        private void OnGameStart()
        {
            if (_titleScreenGo != null) { _titleScreenGo.SetActive(false); }
            if (_hudGo != null) { _hudGo.SetActive(true); }
            _game.Start();
        }

        private void OnGameOver()
        {
            // Phase 4 — пока только лог
            Debug.Log("[Application] Game Over");
        }

        private void OnUpdate(float deltaTime)
        {
            _model?.Update(deltaTime);
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
