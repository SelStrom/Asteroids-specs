using System;
using Shtl.Mvvm;
using UnityEngine;

namespace SelStrom.Asteroids
{
    public class AsteroidViewModel : AbstractViewModel
    {
        public ReactiveValue<Vector2> Position = new();
        public ReactiveValue<Sprite> Sprite = new();
        public Action<Collision2D> OnCollision;
    }

    public class AsteroidVisual : AbstractWidgetView<AsteroidViewModel>, IEntityView
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        private int _prefabInstanceId;
        private float _angularSpeed;

        private void Awake()
        {
            if (_spriteRenderer == null) { _spriteRenderer = GetComponent<SpriteRenderer>(); }
        }

        public int PrefabInstanceId => _prefabInstanceId;

        public void SetPrefabId(int id)
        {
            _prefabInstanceId = id;
        }

        public void SetAngularSpeed(float speed)
        {
            _angularSpeed = speed;
        }

        protected override void OnConnected()
        {
            // Защита: если Awake не успел назначить (редко, но при реиспользовании объектов пула)
            if (_spriteRenderer == null) { _spriteRenderer = GetComponent<SpriteRenderer>(); }
            ViewModel.Position.Connect(val =>
                transform.position = new Vector3(val.x, val.y, transform.position.z));
            ViewModel.Sprite.Connect(val => {
                if (_spriteRenderer != null && val != null) { _spriteRenderer.sprite = val; }
            });
        }

        private void Update()
        {
            transform.Rotate(0f, 0f, _angularSpeed * Time.deltaTime);
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            ViewModel?.OnCollision?.Invoke(col);
        }

        public new void Dispose()
        {
            base.Dispose();
        }
    }
}
