using Model.Components;
using UnityEngine;

namespace SelStrom.Asteroids
{
    // TNode — кортеж трёх компонентов
    public class ThrustSystem : BaseModelSystem<(ThrustComponent Thrust, MoveComponent Move, RotateComponent Rotate)>
    {
        protected override void UpdateNode(IGameEntityModel entity, (ThrustComponent Thrust, MoveComponent Move, RotateComponent Rotate) node, float deltaTime)
        {
            if (!node.Thrust.IsActive.Value) { return; }

            // Добавить скорость в направлении носа (D-07: ThrustUnitsPerSecond = 6 ед/с²)
            var addedVelocity = node.Move.Direction * node.Thrust.ThrustUnitsPerSecond * deltaTime;
            var currentVelocity = node.Move.Direction * node.Move.Speed.Value;
            var newVelocity = currentVelocity + addedVelocity;

            // Ограничение максимальной скорости (D-07: MaxSpeed = 15 ед/с)
            newVelocity = Vector2.ClampMagnitude(newVelocity, node.Thrust.MaxSpeed);

            node.Move.Speed.Value = newVelocity.magnitude;
            if (newVelocity.magnitude > 0.001f)
            {
                node.Move.Direction = newVelocity.normalized;
            }
        }
    }
}
