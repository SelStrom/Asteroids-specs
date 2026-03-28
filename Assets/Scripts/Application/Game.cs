using System;
using Model.Components;
using SelStrom.Asteroids.Configs;
using UnityEngine;

namespace SelStrom.Asteroids
{
    public class Game
    {
        private GameData _configs;
        private EntitiesCatalog _catalog;
        private Model _model;
        private PlayerInput _input;
        private Action _onGameOver;
        private Action<int, int> _onScoreChanged; // (score, lives) → HUD

        private ShipModel _ship;
        private bool _isRunning;

        // Поля прогрессии (волны, жизни, счёт)
        private int _waveNumber;        // текущая волна (1, 2, 3...)
        private int _lives;             // текущие жизни
        private int _highScore;         // лучший счёт сессии
        private int _asteroidCount;     // сколько астероидов живо
        private int _nextBonusLifeScore; // порог для следующей экстра-жизни

        // Публичные свойства для чтения состояния
        public int Score => _model.Score;
        public int Lives => _lives;
        public int HighScore => _highScore;
        public int WaveNumber => _waveNumber;

        public void Connect(GameData configs, EntitiesCatalog catalog, Model model,
                            PlayerInput input, Action onGameOver, Action<int, int> onScoreChanged = null)
        {
            _configs = configs;
            _catalog = catalog;
            _model = model;
            _input = input;
            _onGameOver = onGameOver;
            _onScoreChanged = onScoreChanged;
        }

        public void Start()
        {
            _isRunning = true;
            _waveNumber = 0;
            _lives = 3;                  // PROG-03: начинаем с 3 жизней
            _nextBonusLifeScore = 10000; // PROG-04: первая экстра-жизнь на 10 000

            // Сбросить счёт модели
            _model.Score = 0;

            // Подписаться на ввод
            _input.OnRotateAction += OnRotate;
            _input.OnTrustAction  += OnThrust;
            _input.OnAttackAction += OnAttack;
            _input.OnLaserAction  += OnLaser;

            // Создать корабль
            _ship = _catalog.CreateShip();

            // Подписаться на коллизии корабля
            if (_catalog.GetViewByModel(_ship) is ShipVisual shipVisual)
            {
                shipVisual.ViewModel.OnCollision = OnShipCollided;
            }

            // Подписаться на уничтожение сущностей
            _model.OnEntityDestroyed += OnEntityDestroyed;

            // Подписаться на выстрел и лазер
            _ship.Gun.OnShooting = OnUserGunShooting;
            _ship.Laser.OnLaserFired = OnUserLaserFired;

            // Запустить первую волну
            StartWave();

            // Уведомить HUD
            _onScoreChanged?.Invoke(_model.Score, _lives);
        }

        public void Stop()
        {
            if (_input == null) { return; }
            _isRunning = false;
            _input.OnRotateAction -= OnRotate;
            _input.OnTrustAction  -= OnThrust;
            _input.OnAttackAction -= OnAttack;
            _input.OnLaserAction  -= OnLaser;
            _model.OnEntityDestroyed -= OnEntityDestroyed;
        }

        public void Restart()
        {
            // Остановить без сброса High Score
            _isRunning = false;
            Stop(); // отписаться от ввода и событий

            // Очистить model (Score=0, убрать все сущности из систем)
            _model.CleanUp();

            // Очистить словари EntitiesCatalog — убрать мёртвые ключи от предыдущей сессии.
            // Без этого при повторном Release() возможны исключения и некорректное поведение пула.
            _catalog.Reset();

            // Снова запустить (Start() подпишет ввод, создаст корабль, запустит волну)
            Start();
        }

        private void StartWave()
        {
            _waveNumber++;
            // D-14: первая волна = 4, каждая следующая +1, макс. 12
            var asteroidCount = Mathf.Min(3 + _waveNumber, 12); // wave 1 → 4, wave 2 → 5, ... wave 9+ → 12
            _asteroidCount = 0;

            var gameArea = _model.GameArea;
            var shipPos = _ship != null ? _ship.Move.Position.Value : Vector2.zero;

            for (var i = 0; i < asteroidCount; i++)
            {
                // D-16: спаун вдали от корабля (SpawnAllowedRadius=20 из GameData)
                var pos = GameUtils.GetRandomPositionOutsideRadius(
                    shipPos, _configs.SpawnAllowedRadius, gameArea);

                // Случайное направление и скорость Large (2 ед/с)
                var dir = UnityEngine.Random.insideUnitCircle.normalized;
                var velocity = dir * 2f; // Large: скорость 2 ед/с (Claude's Discretion)

                var asteroid = _catalog.CreateAsteroid(_configs.AsteroidBig, pos, velocity);
                BindAsteroidCollision(asteroid);
                _asteroidCount++;
            }
        }

        private void BindAsteroidCollision(AsteroidModel asteroid)
        {
            if (_catalog.GetViewByModel(asteroid) is AsteroidVisual av)
            {
                av.ViewModel.OnCollision = col => OnAsteroidCollided(asteroid, col);
            }
        }

        private void OnRotate(float direction)
        {
            if (_ship == null || _ship.IsDead()) { return; }
            _ship.Rotate.RotateDirection = direction;
        }

        private void OnThrust(bool isActive)
        {
            if (_ship == null || _ship.IsDead()) { return; }
            _ship.Thrust.IsActive.Value = isActive;
        }

        private void OnAttack()
        {
            if (_ship == null || _ship.IsDead() || _ship.IsInvulnerable) { return; }
            _ship.Gun.Shooting = true;
        }

        private void OnLaser()
        {
            if (_ship == null || _ship.IsDead() || _ship.IsInvulnerable) { return; }
            _ship.Laser.IsLaserFiring = true;
        }

        private void OnUserGunShooting(GunComponent gun)
        {
            // Вычислить позицию и скорость пули из носа корабля (SHOT-02)
            var shipPos = _ship.Move.Position.Value;
            var shipDir = _ship.Move.Direction;
            var bulletPos = shipPos + shipDir * 0.5f; // небольшое смещение вперёд от носа
            var shipVelocity = shipDir * _ship.Move.Speed.Value;
            var bulletVelocity = shipDir * _configs.Bullet.Speed + shipVelocity; // SHOT-02

            var bullet = _catalog.CreateBullet(bulletPos, bulletVelocity);
            bullet.Gun = gun; // сохраняем ссылку на GunComponent для декремента CurrentShoots
        }

        private void OnUserLaserFired(LaserComponent laser)
        {
            // Лазер: луч от носа корабля (D-03)
            // В Phase 3 — просто лог
            Debug.Log("[Game] Laser fired");
        }

        private void OnShipCollided(Collision2D col)
        {
            if (_ship == null || _ship.IsInvulnerable) { return; }

            // Корабль погибает (D-05: без VFX, только скрыть)
            _ship.Kill();
        }

        private void OnAsteroidCollided(AsteroidModel asteroid, Collision2D col)
        {
            if (!_isRunning || asteroid.IsDead()) { return; }

            // Дробить только при попадании пули (не при столкновении с кораблём)
            var hitModel = _catalog.GetModelByGo(col.gameObject);
            if (hitModel is not BulletModel) { return; }

            // Начислить очки (D-01: DATA_SCHEMA значения)
            var data = GetAsteroidData(asteroid.Size);
            _model.Score += data.Score; // Big=1, Medium=2, Small=3

            // Проверить экстра-жизнь (PROG-04: каждые 10 000 очков, макс. 6)
            while (_model.Score >= _nextBonusLifeScore && _lives < 6)
            {
                _lives++;
                _nextBonusLifeScore += 10000;
            }

            // Уничтожить астероид
            asteroid.Kill();

            // Уведомить HUD
            _onScoreChanged?.Invoke(_model.Score, _lives);
        }

        private AsteroidData GetAsteroidData(int size)
        {
            return size switch {
                3 => _configs.AsteroidBig,
                2 => _configs.AsteroidMedium,
                _ => _configs.AsteroidSmall
            };
        }

        private void SpawnFragments(AsteroidModel asteroid)
        {
            // D-04: Large(3)→2 Medium(2), Medium(2)→2 Small(1), Small(1)→исчезает
            var childSize = asteroid.Size - 1;
            if (childSize <= 0) { return; } // Small — исчезает, осколков нет

            var childData = GetAsteroidData(childSize);
            // Скорости: Small=4, Medium=3 (осколки быстрее родителя, AST-06)
            var childSpeed = childSize == 1 ? 4f : 3f;
            var parentPos = asteroid.Move.Position.Value;

            for (var i = 0; i < 2; i++)
            {
                // D-05: случайные направления осколков
                var dir = UnityEngine.Random.insideUnitCircle.normalized;
                var vel = dir * childSpeed;
                var child = _catalog.CreateAsteroid(childData, parentPos, vel);
                BindAsteroidCollision(child);
                _asteroidCount++;
            }
        }

        private void OnEntityDestroyed(IGameEntityModel model)
        {
            if (model is BulletModel bullet)
            {
                // Декремент счётчика пуль в GunComponent
                if (bullet.Gun != null) { bullet.Gun.CurrentShoots--; }
            }

            if (model is AsteroidModel asteroid)
            {
                // Дробление (D-04, D-05)
                SpawnFragments(asteroid);
                _asteroidCount--;

                // Следующая волна когда все астероиды уничтожены (D-15)
                if (_asteroidCount <= 0 && _isRunning)
                {
                    StartWave();
                }
            }

            if (model is ShipModel)
            {
                _lives--;

                if (_lives <= 0)
                {
                    // Game Over (PROG-05)
                    _isRunning = false;

                    // Обновить High Score сессии (D-09, PROG-07)
                    if (_model.Score > _highScore)
                    {
                        _highScore = _model.Score;
                    }

                    _onGameOver?.Invoke();
                }
                else
                {
                    // Respawn через 2 секунды (SHIP-06)
                    _model.ActionScheduler.Schedule(2f, RespawnShip);
                }

                // Уведомить HUD
                _onScoreChanged?.Invoke(_model.Score, _lives);
            }

            _catalog.Release(model);
        }

        private void RespawnShip()
        {
            // Пересоздать корабль в центре (0, 0) (SHIP-06)
            _ship = _catalog.CreateShip();
            if (_catalog.GetViewByModel(_ship) is ShipVisual shipVisual)
            {
                shipVisual.ViewModel.OnCollision = OnShipCollided;
            }
            _ship.Gun.OnShooting = OnUserGunShooting;
            _ship.Laser.OnLaserFired = OnUserLaserFired;

            // Мигание неуязвимости 3 секунды (SHIP-07)
            _ship.IsInvulnerable = true;
            StartBlink(15, false); // 15 миганий × 0.2с = 3с (D-05)
        }

        private void StartBlink(int blinksLeft, bool visible)
        {
            if (_ship == null) { return; }

            if (blinksLeft <= 0)
            {
                // Мигание завершено — показать корабль постоянно
                if (_catalog.GetViewByModel(_ship) is ShipVisual sv)
                {
                    sv.ViewModel.IsVisible.Value = true;
                }
                _ship.IsInvulnerable = false;
                return;
            }

            if (_catalog.GetViewByModel(_ship) is ShipVisual shipVis)
            {
                shipVis.ViewModel.IsVisible.Value = visible;
            }

            _model.ActionScheduler.Schedule(0.15f, () => StartBlink(blinksLeft - 1, !visible));
        }

        public void Dispose()
        {
            Stop();
            _catalog = null;
            _model = null;
            _input = null;
        }
    }
}
