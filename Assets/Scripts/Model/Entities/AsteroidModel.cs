using Model.Components;

namespace SelStrom.Asteroids
{
    public class AsteroidModel : IGameEntityModel
    {
        private bool _killed;

        public MoveComponent Move { get; } = new();
        public int Size { get; set; } // 1=Small, 2=Medium, 3=Big

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
