using System;
using System.Collections.Generic;
using UnityEngine;

namespace SelStrom.Asteroids
{
    public class GameObjectPool
    {
        private readonly Dictionary<int, Stack<GameObject>> _pool = new();

        public T Get<T>(GameObject prefab) where T : Component
        {
            var id = prefab.GetInstanceID();
            if (!_pool.ContainsKey(id)) { _pool[id] = new Stack<GameObject>(); }
            var go = _pool[id].Count > 0 ? _pool[id].Pop() : UnityEngine.Object.Instantiate(prefab);
            go.SetActive(true);
            return go.GetComponent<T>();
        }

        public void Release(GameObject go, GameObject prefab)
        {
            var id = prefab.GetInstanceID();
            if (!_pool.ContainsKey(id)) { throw new Exception($"[GameObjectPool] Unknown prefab id {id}"); }
            go.SetActive(false);
            _pool[id].Push(go);
        }

        public void Register(GameObject prefab)
        {
            var id = prefab.GetInstanceID();
            if (!_pool.ContainsKey(id)) { _pool[id] = new Stack<GameObject>(); }
        }
    }
}
