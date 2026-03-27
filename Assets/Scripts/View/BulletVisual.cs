using System;
using Shtl.Mvvm;
using UnityEngine;

namespace SelStrom.Asteroids
{
    public class BulletViewModel : AbstractViewModel
    {
        public ReactiveValue<Vector2> Position = new();
        public Action<Collision2D> OnCollision;
    }

    public class BulletVisual : AbstractWidgetView<BulletViewModel>, IEntityView
    {
        private int _prefabInstanceId;

        public int PrefabInstanceId => _prefabInstanceId;

        public void SetPrefabId(int id)
        {
            _prefabInstanceId = id;
        }

        protected override void OnConnected()
        {
            ViewModel.Position.Connect(val =>
                transform.position = new Vector3(val.x, val.y, transform.position.z));
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            ViewModel?.OnCollision?.Invoke(col);
        }
    }
}
