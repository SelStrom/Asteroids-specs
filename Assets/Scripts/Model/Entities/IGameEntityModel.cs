namespace SelStrom.Asteroids
{
    public interface IGameEntityModel
    {
        bool IsDead();
        void Kill();
        void AcceptWith(IGroupVisitor visitor);
    }
}
