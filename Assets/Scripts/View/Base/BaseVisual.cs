using UnityEngine;

namespace SelStrom.Asteroids
{
    public abstract class BaseVisual : MonoBehaviour, IEntityView
    {
        private int _prefabInstanceId;

        public int PrefabInstanceId => _prefabInstanceId;

        public void SetPrefabId(int id)
        {
            _prefabInstanceId = id;
        }
    }
}
