using UnityEngine;

namespace SelStrom.Asteroids
{
    public interface IEntityView
    {
        GameObject gameObject { get; }
        int PrefabInstanceId { get; }
        void SetPrefabId(int id);
    }
}
