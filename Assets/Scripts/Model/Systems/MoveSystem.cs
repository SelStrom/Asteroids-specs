using Model.Components;
using UnityEngine;

namespace SelStrom.Asteroids
{
    public class MoveSystem : BaseModelSystem<MoveComponent>
    {
        private Vector2 _gameArea;

        public void SetGameArea(Vector2 gameArea)
        {
            _gameArea = gameArea;
        }

        protected override void UpdateNode(IGameEntityModel entity, MoveComponent move, float deltaTime)
        {
            if (move.Speed.Value == 0f && move.Direction == Vector2.zero) { return; }

            var velocity = move.Direction * move.Speed.Value;
            var newPos = move.Position.Value + velocity * deltaTime;

            // Wrap-around: _gameArea.x = ширина (80 ед), _gameArea.y = высота (45 ед)
            newPos.x = WrapAxis(newPos.x, _gameArea.x);
            newPos.y = WrapAxis(newPos.y, _gameArea.y);

            move.Position.Value = newPos;
        }

        private static float WrapAxis(float val, float size)
        {
            var half = size / 2f;
            if (val > half) { return val - size; }
            if (val < -half) { return val + size; }
            return val;
        }
    }
}
