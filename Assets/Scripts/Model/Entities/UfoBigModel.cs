using Model.Components;

namespace SelStrom.Asteroids
{
    public class UfoBigModel : IGameEntityModel
    {
        private bool _killed;

        public MoveComponent Move { get; } = new();
        public GunComponent Gun { get; private set; }

        public bool IsDead() => _killed;

        public void Kill()
        {
            _killed = true;
        }

        public void SetGun(GunComponent gun)
        {
            Gun = gun;
        }

        public virtual void AcceptWith(IGroupVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    // UfoModel — подкласс с MoveToComponent (Phase 5)
    public class UfoModel : UfoBigModel
    {
        public MoveToComponent MoveTo { get; } = new();
        public ShootToComponent ShootTo { get; } = new();
    }
}
