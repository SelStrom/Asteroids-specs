using Model.Components;

namespace SelStrom.Asteroids
{
    public class LifeTimeSystem : BaseModelSystem<LifeTimeComponent>
    {
        protected override void UpdateNode(IGameEntityModel entity, LifeTimeComponent lifeTime, float deltaTime)
        {
            lifeTime.TimeLeft -= deltaTime;
            if (lifeTime.TimeLeft <= 0f)
            {
                entity.Kill();
            }
        }
    }
}
