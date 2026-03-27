using UnityEngine;

namespace SelStrom.Asteroids.Configs
{
    [CreateAssetMenu(menuName = "Gun data")]
    public class GunData : ScriptableObject
    {
        public int MaxShoots;
        public float ReloadDurationSec;
    }
}
