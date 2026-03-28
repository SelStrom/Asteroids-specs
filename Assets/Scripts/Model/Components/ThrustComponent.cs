using Shtl.Mvvm;

namespace Model.Components
{
    public class ThrustComponent : IModelComponent
    {
        public ObservableValue<bool> IsActive = new();
        public float ThrustUnitsPerSecond;
        public float MaxSpeed;
        public float Damping;

        public ThrustComponent(float thrustUnitsPerSecond, float maxSpeed, float damping)
        {
            ThrustUnitsPerSecond = thrustUnitsPerSecond;
            MaxSpeed = maxSpeed;
            Damping = damping;
        }
    }
}
