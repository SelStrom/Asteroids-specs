using System;
using Shtl.Mvvm;

namespace Model.Components
{
    public class LaserComponent : IModelComponent
    {
        public bool IsLaserFiring;
        public ObservableValue<int> CurrentShoots = new();
        public ObservableValue<float> ReloadTimeLeft = new();
        public int MaxShoots;
        public int LaserUpdateDurationSec;
        public float BeamEffectLifetimeSec;
        public Action<LaserComponent> OnLaserFired;
    }
}
