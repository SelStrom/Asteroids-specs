using Shtl.Mvvm;

namespace Model.Components
{
    public class ThrustComponent : IModelComponent
    {
        public ObservableValue<bool> IsActive = new();
        public float ThrustUnitsPerSecond;
        public float MaxSpeed;

        public ThrustComponent(float thrustUnitsPerSecond, float maxSpeed)
        {
            ThrustUnitsPerSecond = thrustUnitsPerSecond;
            MaxSpeed = maxSpeed;
        }
    }
}
