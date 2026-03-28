#if UNITY_EDITOR
using SelStrom.Asteroids.Configs;
using UnityEditor;
using UnityEngine;

namespace SelStrom.Asteroids.Editor
{
    /// <summary>
    /// Автоматически запускает Phase4Setup при старте редактора, если asteroid prefabs ещё не назначены.
    /// </summary>
    public static class Phase4AutoSetup
    {
        [InitializeOnLoadMethod]
        private static void TrySetupIfNeeded()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) { return; }

            var bigData = AssetDatabase.LoadAssetAtPath<AsteroidData>("Assets/Media/configs/AsteroidBigData.asset");
            if (bigData == null || bigData.Prefab != null)
            {
                return; // уже настроено или файл не существует
            }

            Debug.Log("[Phase4AutoSetup] Asteroid configs не настроены — запускаю Phase4Setup автоматически...");
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode) { Phase4Setup.SetupAll(); }
            };
        }
    }
}
#endif
