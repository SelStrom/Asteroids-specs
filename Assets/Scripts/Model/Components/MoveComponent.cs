using Shtl.Mvvm;
using UnityEngine;

namespace Model.Components
{
    public class MoveComponent : IModelComponent
    {
        public ObservableValue<Vector2> Position = new();
        public ObservableValue<float> Speed = new();
        public ObservableValue<Vector2> Direction = new();
    }
}
