using UnityEngine;

namespace SelStrom.Asteroids.Configs
{
    [CreateAssetMenu(menuName = "Asteroid data")]
    public class AsteroidData : BaseGameEntityData
    {
        public GameObject Prefab;
        public Sprite[] SpriteVariants;
    }
}
