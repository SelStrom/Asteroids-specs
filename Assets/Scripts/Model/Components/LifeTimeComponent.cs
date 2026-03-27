namespace Model.Components
{
    public class LifeTimeComponent : IModelComponent
    {
        public float TimeLeft;

        public LifeTimeComponent(float lifeTime)
        {
            TimeLeft = lifeTime;
        }
    }
}
