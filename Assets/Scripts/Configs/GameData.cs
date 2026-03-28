using System;
using UnityEngine;

namespace SelStrom.Asteroids.Configs
{
    [CreateAssetMenu(menuName = "Game data")]
    public class GameData : ScriptableObject
    {
        [Header("Gameplay")]
        public int AsteroidInitialCount = 10;
        public int SpawnAllowedRadius = 20;
        public float SpawnNewEnemyDurationSec = 25f;

        [Space]
        public GameObject VfxBlowPrefab; // null в Phase 3 (D-06)

        [Space]
        public UfoData UfoBig;
        public UfoData Ufo;
        public AsteroidData AsteroidBig;
        public AsteroidData AsteroidMedium;
        public AsteroidData AsteroidSmall;

        [Space]
        public BulletData Bullet;
        public LaserData Laser;
        public ShipData Ship;

        [Space]
        public string LeaderboardId = "asteroids_highscores";

        [Serializable]
        public struct BulletData
        {
            public GameObject Prefab;
            public GameObject EnemyPrefab;
            public int LifeTimeSeconds;  // int как в DATA_SCHEMA: значение 2
            public float Speed;          // 20 ед/с
            public Sprite BulletSprite;
        }

        [Serializable]
        public struct LaserData
        {
            public GameObject Prefab;
            public float BeamEffectLifetimeSec;   // 0.5 с
            public int LaserUpdateDurationSec;    // 10 с
            public int LaserMaxShoots;            // 3 заряда
        }

        [Serializable]
        public struct ShipData
        {
            public GameObject Prefab;
            public Sprite MainSprite;
            public Sprite ThrustSprite;
            public float ThrustUnitsPerSecond;   // 6 ед/с²
            public float MaxSpeed;               // 15 ед/с
            public float Damping;                // затухание скорости без тяги, ед/с²
            public GunData Gun;
        }
    }
}
