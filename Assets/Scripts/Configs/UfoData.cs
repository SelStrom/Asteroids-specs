using UnityEngine;

namespace SelStrom.Asteroids.Configs
{
    [CreateAssetMenu(menuName = "Ufo data")]
    public class UfoData : BaseGameEntityData
    {
        public GameObject Prefab;
        public float Speed;
        public GunData Gun;
        public float ShootDurationSec;
    }
}
