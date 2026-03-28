using Model.Components;
using UnityEngine;

namespace SelStrom.Asteroids
{
    public class RotateSystem : BaseModelSystem<RotateComponent>
    {
        protected override void UpdateNode(IGameEntityModel entity, RotateComponent rotate, float deltaTime)
        {
            if (rotate.RotateDirection == 0f) { return; }

            // RotateDirection: -1 = влево (counter-clockwise), +1 = вправо (clockwise)
            // В 2D: угол в градусах, ось Z. Поворот влево = увеличение угла.
            if (entity is ShipModel ship)
            {
                var angleDelta = -rotate.RotateDirection * rotate.RotateSpeed * deltaTime;
                var currentAngle = Mathf.Atan2(ship.Move.Direction.Value.y, ship.Move.Direction.Value.x) * Mathf.Rad2Deg;
                var newAngle = (currentAngle + angleDelta) * Mathf.Deg2Rad;
                ship.Move.Direction.Value = new Vector2(Mathf.Cos(newAngle), Mathf.Sin(newAngle)).normalized;
            }
        }
    }
}
