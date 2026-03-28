using Model.Components;
using SelStrom.Asteroids.Configs;
using UnityEngine;

namespace SelStrom.Asteroids
{
    public class ShipModel : IGameEntityModel
    {
        private bool _killed;

        public MoveComponent Move { get; } = new();
        public RotateComponent Rotate { get; } = new();
        public ThrustComponent Thrust { get; private set; }
        public GunComponent Gun { get; private set; }
        public LaserComponent Laser { get; } = new();

        public bool IsInvulnerable { get; set; }

        public void Setup(GameData configs)
        {
            Thrust = new ThrustComponent(configs.Ship.ThrustUnitsPerSecond, configs.Ship.MaxSpeed, configs.Ship.Damping);
            Gun = new GunComponent(configs.Ship.Gun);
            Laser.MaxShoots = configs.Laser.LaserMaxShoots;
            Laser.LaserUpdateDurationSec = configs.Laser.LaserUpdateDurationSec;
            Laser.BeamEffectLifetimeSec = configs.Laser.BeamEffectLifetimeSec;
            Laser.CurrentShoots.Value = configs.Laser.LaserMaxShoots;
        }

        public bool IsDead() => _killed;

        public void Kill()
        {
            _killed = true;
        }

        public void Reset()
        {
            _killed = false;
            IsInvulnerable = false;
            Move.Position.Value = Vector2.zero;
            Move.Speed.Value = 0f;
            Move.Direction = Vector2.up;
            Rotate.RotateDirection = 0f;
            Thrust.IsActive.Value = false;
            Gun.CurrentShoots = 0;
            Gun.Shooting = false;
            Laser.CurrentShoots.Value = Laser.MaxShoots;
            Laser.ReloadTimeLeft.Value = 0f;
        }

        public void AcceptWith(IGroupVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
