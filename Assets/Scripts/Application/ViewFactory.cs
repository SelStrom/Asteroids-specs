using UnityEngine;

namespace SelStrom.Asteroids
{
    public class ViewFactory
    {
        private GameObjectPool _pool;

        public void Connect(GameObjectPool pool)
        {
            _pool = pool;
        }

        public T Get<T>(GameObject prefab) where T : Component, IEntityView
        {
            var view = _pool.Get<T>(prefab);
            view.SetPrefabId(prefab.GetInstanceID());
            return view;
        }

        public void Release(IEntityView view, GameObject prefab)
        {
            _pool.Release(view.gameObject, prefab);
        }
    }
}
