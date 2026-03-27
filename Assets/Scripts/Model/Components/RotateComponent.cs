namespace Model.Components
{
    public class RotateComponent : IModelComponent
    {
        public float RotateDirection; // -1, 0, 1
        public float RotateSpeed = 180f; // градусов/сек
    }
}
