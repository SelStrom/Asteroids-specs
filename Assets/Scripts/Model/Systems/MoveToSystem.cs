using Model.Components;
using UnityEngine;

namespace SelStrom.Asteroids
{
    public class MoveToSystem : BaseModelSystem<MoveToComponent>
    {
        protected override void UpdateNode(IGameEntityModel entity, MoveToComponent moveTo, float deltaTime)
        {
            if (!(entity is UfoModel ufo)) { return; }
            var currentPos = ufo.Move.Position.Value;
            var direction = (moveTo.Target - currentPos).normalized;
            var distance = moveTo.Speed * deltaTime;
            ufo.Move.Position.Value = currentPos + direction * distance;
        }
    }
}
