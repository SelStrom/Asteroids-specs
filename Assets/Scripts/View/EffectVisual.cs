using System;
using UnityEngine;

namespace SelStrom.Asteroids
{
    public class EffectVisual : MonoBehaviour, IEntityView
    {
        [SerializeField] private ParticleSystem _particles;

        private int _prefabInstanceId;
        private Action<EffectVisual> _onFinished; // callback для возврата в пул

        public int PrefabInstanceId => _prefabInstanceId;

        public void SetPrefabId(int id)
        {
            _prefabInstanceId = id;
        }

        public void Initialize(Action<EffectVisual> onFinished)
        {
            _onFinished = onFinished;
        }

        public void Play(Vector2 position, float scale = 1f)
        {
            transform.position = new Vector3(position.x, position.y, 0f);
            if (_particles == null)
            {
                Debug.LogWarning("[EffectVisual] ParticleSystem не назначен");
                _onFinished?.Invoke(this);
                return;
            }
            var main = _particles.main;
            main.startSizeMultiplier = scale;
            _particles.Play();
        }

        // Вызывается Unity когда ParticleSystem завершился (StopAction = Callback)
        private void OnParticleSystemStopped()
        {
            _onFinished?.Invoke(this);
        }
    }
}
