using Model.Components;

namespace SelStrom.Asteroids
{
    public class GunSystem : BaseModelSystem<GunComponent>
    {
        protected override void UpdateNode(IGameEntityModel entity, GunComponent gun, float deltaTime)
        {
            if (gun.Config == null) { return; }
            if (gun.IsReloading)
            {
                gun.ReloadTimer -= deltaTime;
                if (gun.ReloadTimer <= 0f)
                {
                    gun.ReloadTimer = 0f;
                    gun.CurrentShoots = 0;
                }
                gun.Shooting = false;
                return;
            }
            if (gun.Shooting && gun.CurrentShoots < gun.Config.MaxShoots)
            {
                gun.CurrentShoots++;
                gun.OnShooting?.Invoke(gun);
                if (gun.CurrentShoots >= gun.Config.MaxShoots)
                {
                    gun.ReloadTimer = gun.Config.ReloadDurationSec;
                }
            }
            gun.Shooting = false; // сброс флага после обработки
        }
    }
}
