using UnityEngine;

namespace SelStrom.Asteroids
{
    public static class GameUtils
    {
        public static Vector2 GetRandomPositionOutsideRadius(Vector2 center, float minRadius, Vector2 gameArea)
        {
            Vector2 pos;
            do {
                pos = new Vector2(
                    Random.Range(-gameArea.x / 2f, gameArea.x / 2f),
                    Random.Range(-gameArea.y / 2f, gameArea.y / 2f));
            } while (Vector2.Distance(pos, center) < minRadius);
            return pos;
        }
    }
}
