namespace SelStrom.Asteroids
{
    public interface IGroupVisitor
    {
        void Visit(ShipModel model);
        void Visit(BulletModel model);
        void Visit(AsteroidModel model);   // Phase 4
        void Visit(UfoBigModel model);     // Phase 4-5
    }
}
