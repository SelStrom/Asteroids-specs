using UnityEngine;

namespace SelStrom.Asteroids
{
    public class EffectVisual : MonoBehaviour, IEntityView
    {
        private int _prefabInstanceId;

        public int PrefabInstanceId => _prefabInstanceId;

        public void SetPrefabId(int id)
        {
            _prefabInstanceId = id;
        }
    }
}
