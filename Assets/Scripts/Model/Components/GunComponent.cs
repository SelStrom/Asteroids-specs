using System;
using SelStrom.Asteroids.Configs;

namespace Model.Components
{
    public class GunComponent : IModelComponent
    {
        public bool Shooting;
        public int CurrentShoots;
        public GunData Config;
        public Action<GunComponent> OnShooting;

        public GunComponent(GunData config)
        {
            Config = config;
        }
    }
}
