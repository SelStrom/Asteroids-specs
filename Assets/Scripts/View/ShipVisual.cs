using System;
using Shtl.Mvvm;
using UnityEngine;

namespace SelStrom.Asteroids
{
    public class ShipViewModel : AbstractViewModel
    {
        public ReactiveValue<Vector2> Position = new();
        public ReactiveValue<bool> IsThrusting = new();
        public ReactiveValue<bool> IsVisible = new(true);
        public ReactiveValue<float> Rotation = new();
        public Action<Collision2D> OnCollision;
    }

    public class ShipVisual : AbstractWidgetView<ShipViewModel>, IEntityView
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite _mainSprite;
        [SerializeField] private Sprite _thrustSprite;
        private int _prefabInstanceId;

        public int PrefabInstanceId => _prefabInstanceId;

        public void SetPrefabId(int id)
        {
            _prefabInstanceId = id;
        }

        protected override void OnConnected()
        {
            // Биндинг позиции: Position → Transform.position
            ViewModel.Position.Connect(val =>
                transform.position = new Vector3(val.x, val.y, transform.position.z));

            // Биндинг тяги: IsThrusting → смена спрайта (SHIP-08)
            ViewModel.IsThrusting.Connect(val =>
                _spriteRenderer.sprite = val ? _thrustSprite : _mainSprite);

            // Биндинг видимости: IsVisible → SpriteRenderer.enabled (мигание SHIP-07)
            ViewModel.IsVisible.Connect(val =>
                _spriteRenderer.enabled = val);

            // Биндинг поворота: Rotation (градусы) → Transform.rotation
            ViewModel.Rotation.Connect(angleDeg =>
                transform.rotation = Quaternion.Euler(0f, 0f, angleDeg - 90f));
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            // Коллизия → callback → Game (D-08)
            ViewModel?.OnCollision?.Invoke(col);
        }
    }
}
