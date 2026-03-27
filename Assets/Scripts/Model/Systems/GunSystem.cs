using Model.Components;

namespace SelStrom.Asteroids
{
    public class GunSystem : BaseModelSystem<GunComponent>
    {
        protected override void UpdateNode(IGameEntityModel entity, GunComponent gun, float deltaTime)
        {
            if (gun.Shooting && gun.CurrentShoots < gun.Config.MaxShoots)
            {
                gun.CurrentShoots++;
                gun.OnShooting?.Invoke(gun);
            }
            gun.Shooting = false; // сброс флага после обработки
        }
    }
}
