using System;
using Shtl.Mvvm;
using UnityEngine;

namespace SelStrom.Asteroids
{
    public class UfoViewModel : AbstractViewModel
    {
        public ReactiveValue<Vector2> Position = new();
        public ReactiveValue<Sprite> Sprite = new();
        public Action<Collision2D> OnCollision;
    }

    public class UfoVisual : AbstractWidgetView<UfoViewModel>, IEntityView
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        private int _prefabInstanceId;

        private void Awake()
        {
            if (_spriteRenderer == null) { _spriteRenderer = GetComponent<SpriteRenderer>(); }
        }

        public int PrefabInstanceId => _prefabInstanceId;

        public void SetPrefabId(int id)
        {
            _prefabInstanceId = id;
        }

        protected override void OnConnected()
        {
            if (_spriteRenderer == null) { _spriteRenderer = GetComponent<SpriteRenderer>(); }
            ViewModel.Position.Connect(val =>
                transform.position = new Vector3(val.x, val.y, transform.position.z));
            ViewModel.Sprite.Connect(val => {
                if (_spriteRenderer != null && val != null) { _spriteRenderer.sprite = val; }
            });
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
