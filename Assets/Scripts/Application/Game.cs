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

        private ShipModel _ship;
        private bool _isRunning;

        public void Connect(GameData configs, EntitiesCatalog catalog, Model model, PlayerInput input, Action onGameOver)
        {
            _configs = configs;
            _catalog = catalog;
            _model = model;
            _input = input;
            _onGameOver = onGameOver;
        }

        public void Start()
        {
            _isRunning = true;

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
        }

        public void Stop()
        {
            _isRunning = false;
            _input.OnRotateAction -= OnRotate;
            _input.OnTrustAction  -= OnThrust;
            _input.OnAttackAction -= OnAttack;
            _input.OnLaserAction  -= OnLaser;
            _model.OnEntityDestroyed -= OnEntityDestroyed;
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

        private void OnEntityDestroyed(IGameEntityModel model)
        {
            if (model is BulletModel bullet)
            {
                // Декремент счётчика пуль в GunComponent
                if (bullet.Gun != null) { bullet.Gun.CurrentShoots--; }
            }

            if (model is ShipModel)
            {
                // Respawn через 2 секунды (D-05, SHIP-06)
                _model.ActionScheduler.Schedule(2f, RespawnShip);
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
