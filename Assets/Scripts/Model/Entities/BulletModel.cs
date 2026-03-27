using Model.Components;

namespace SelStrom.Asteroids
{
    public class BulletModel : IGameEntityModel
    {
        private bool _killed;

        public MoveComponent Move { get; } = new();
        public LifeTimeComponent LifeTime { get; private set; }
        public bool IsEnemy { get; set; }

        public void Setup(float lifeTime)
        {
            LifeTime = new LifeTimeComponent(lifeTime);
        }

        public bool IsDead() => _killed;

        public void Kill()
        {
            _killed = true;
        }

        public void AcceptWith(IGroupVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
