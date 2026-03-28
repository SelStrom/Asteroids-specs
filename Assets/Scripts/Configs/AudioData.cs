using UnityEngine;

namespace SelStrom.Asteroids.Configs
{
    [CreateAssetMenu(menuName = "Audio data")]
    public class AudioData : ScriptableObject
    {
        public AudioClip Shoot;
        public AudioClip Thrust;
        public AudioClip ExplodeShip;
        public AudioClip ExplodeAsteroidBig;
        public AudioClip ExplodeAsteroidMedium;
        public AudioClip ExplodeAsteroidSmall;
        public AudioClip UfoTone;
        public AudioClip BeatLow;
        public AudioClip BeatHigh;
    }
}
