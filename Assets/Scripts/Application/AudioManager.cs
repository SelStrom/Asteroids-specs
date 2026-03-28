using SelStrom.Asteroids.Configs;
using UnityEngine;

namespace SelStrom.Asteroids
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioData _data;

        [Header("AudioSources")]
        [SerializeField] private AudioSource _shoot;
        [SerializeField] private AudioSource _thrust;
        [SerializeField] private AudioSource _explodeShip;
        [SerializeField] private AudioSource _explodeAsteroidBig;
        [SerializeField] private AudioSource _explodeAsteroidMedium;
        [SerializeField] private AudioSource _explodeAsteroidSmall;
        [SerializeField] private AudioSource _ufoTone;
        [SerializeField] private AudioSource _beatLow;
        [SerializeField] private AudioSource _beatHigh;

        private int _asteroidCount = 12;
        private bool _beatRunning;

        // AUD-01: Звук выстрела корабля
        public void PlayShoot()
        {
            PlayOneShot(_shoot);
        }

        // AUD-02: Звук тяги (зациклен пока зажата кнопка)
        public void PlayThrust(bool isActive)
        {
            if (_thrust == null) { return; }
            if (isActive && !_thrust.isPlaying)
            {
                _thrust.Play();
            }
            else if (!isActive && _thrust.isPlaying)
            {
                _thrust.Stop();
            }
        }

        // AUD-03: Взрыв корабля
        public void PlayExplodeShip()
        {
            PlayOneShot(_explodeShip);
        }

        // AUD-04: Взрыв астероида по размеру (size: 3=big, 2=medium, 1=small)
        public void PlayExplodeAsteroid(int size)
        {
            var src = size == 3 ? _explodeAsteroidBig
                    : size == 2 ? _explodeAsteroidMedium
                    : _explodeAsteroidSmall;
            PlayOneShot(src);
        }

        // AUD-05: Тон UFO (зациклен пока UFO на экране)
        public void SetUfoTone(bool active)
        {
            if (_ufoTone == null) { return; }
            if (active && !_ufoTone.isPlaying)
            {
                _ufoTone.Play();
            }
            else if (!active)
            {
                _ufoTone.Stop();
            }
        }

        // AUD-06: Запустить фоновый пульс
        public void StartBeat()
        {
            if (_beatRunning) { return; }
            _beatRunning = true;
            StartCoroutine(BeatLoop());
        }

        // AUD-06: Остановить фоновый пульс
        public void StopBeat()
        {
            _beatRunning = false;
            StopAllCoroutines();
        }

        // AUD-06: Обновить число астероидов для расчёта темпа
        public void SetAsteroidCount(int count)
        {
            _asteroidCount = Mathf.Max(count, 0);
        }

        // Остановить все звуки (для Restart и GameOver)
        public void StopAll()
        {
            StopBeat();
            if (_thrust != null && _thrust.isPlaying) { _thrust.Stop(); }
            if (_ufoTone != null && _ufoTone.isPlaying) { _ufoTone.Stop(); }
        }

        // Назначить AudioClip из AudioData (вызывается из Phase6Setup или Awake)
        public void ApplyAudioData()
        {
            if (_data == null) { return; }
            SetClip(_shoot, _data.Shoot);
            SetClipLoop(_thrust, _data.Thrust);
            SetClip(_explodeShip, _data.ExplodeShip);
            SetClip(_explodeAsteroidBig, _data.ExplodeAsteroidBig);
            SetClip(_explodeAsteroidMedium, _data.ExplodeAsteroidMedium);
            SetClip(_explodeAsteroidSmall, _data.ExplodeAsteroidSmall);
            SetClipLoop(_ufoTone, _data.UfoTone);
            SetClip(_beatLow, _data.BeatLow);
            SetClip(_beatHigh, _data.BeatHigh);
        }

        private void Awake()
        {
            ApplyAudioData();
        }

        private System.Collections.IEnumerator BeatLoop()
        {
            var useLow = true;
            while (_beatRunning)
            {
                // Интервал: от 1.0s (12 астероидов) до 0.25s (0 астероидов)
                var t = _asteroidCount / 12f;
                var interval = Mathf.Lerp(0.25f, 1.0f, t);
                var src = useLow ? _beatLow : _beatHigh;
                PlayOneShot(src);
                useLow = !useLow;
                yield return new WaitForSeconds(interval);
            }
        }

        private void PlayOneShot(AudioSource src)
        {
            if (src == null || src.clip == null)
            {
                // null guard — нет crash при отсутствии AudioClip (см. Pitfall 1 в RESEARCH.md)
                return;
            }
            src.PlayOneShot(src.clip);
        }

        private static void SetClip(AudioSource src, AudioClip clip)
        {
            if (src != null) { src.clip = clip; }
        }

        private static void SetClipLoop(AudioSource src, AudioClip clip)
        {
            if (src == null) { return; }
            src.clip = clip;
            src.loop = true;
        }
    }
}
