using Model.Components;

namespace SelStrom.Asteroids
{
    public class ShootToSystem : BaseModelSystem<ShootToComponent>
    {
        protected override void UpdateNode(IGameEntityModel entity, ShootToComponent shootTo, float deltaTime)
        {
            shootTo.Timer -= deltaTime;
            if (shootTo.Timer > 0f) { return; }
            shootTo.Timer = shootTo.ShootInterval;

            if (!(entity is UfoModel ufo)) { return; }
            shootTo.OnShoot?.Invoke(ufo);
        }
    }
}
