using System;
using SelStrom.Asteroids;

namespace Model.Components
{
    public class ShootToComponent : IModelComponent
    {
        public float ShootInterval;
        public float Timer;
        public Action<UfoModel> OnShoot;
    }
}
