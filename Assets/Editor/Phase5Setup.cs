#if UNITY_EDITOR
using SelStrom.Asteroids;
using SelStrom.Asteroids.Configs;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace SelStrom.Asteroids.Editor
{
    public static class Phase5Setup
    {
        private const string AtlasGuid = "cf467e92e508b5878eb24c5a126421e0";

        [MenuItem("Asteroids/Setup Phase 5 Assets")]
        public static void SetupAll()
        {
            CreateUfoGunConfigs();
            CreateUfoConfigs();
            CreateUfoPrefabs();
            UpdateGameData();
            SetupWaveBanner();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("[Phase5Setup] Готово. Все Phase 5 assets созданы.");
        }

        private static Sprite LoadSprite(string spriteName)
        {
            var atlasPath = AssetDatabase.GUIDToAssetPath(AtlasGuid);
            var allSprites = AssetDatabase.LoadAllAssetsAtPath(atlasPath);
            foreach (var obj in allSprites)
            {
                if (obj is Sprite sprite && sprite.name == spriteName)
                {
                    return sprite;
                }
            }
            UnityEngine.Debug.LogWarning($"[Phase5Setup] Спрайт не найден: {spriteName}");
            return null;
        }

        private static void CreateUfoGunConfigs()
        {
            // UfoBigGunData.asset — Large UFO пушка: 1 выстрел, перезарядка 2 сек
            var bigGunPath = "Assets/Media/configs/UfoBigGunData.asset";
            var bigGun = AssetDatabase.LoadAssetAtPath<GunData>(bigGunPath);
            if (bigGun == null)
            {
                bigGun = ScriptableObject.CreateInstance<GunData>();
                AssetDatabase.CreateAsset(bigGun, bigGunPath);
            }
            bigGun.MaxShoots = 1;
            bigGun.ReloadDurationSec = 2f;
            EditorUtility.SetDirty(bigGun);

            // UfoSmallGunData.asset — Small UFO пушка: 1 выстрел, перезарядка в ShootToSystem
            var smallGunPath = "Assets/Media/configs/UfoSmallGunData.asset";
            var smallGun = AssetDatabase.LoadAssetAtPath<GunData>(smallGunPath);
            if (smallGun == null)
            {
                smallGun = ScriptableObject.CreateInstance<GunData>();
                AssetDatabase.CreateAsset(smallGun, smallGunPath);
            }
            smallGun.MaxShoots = 1;
            smallGun.ReloadDurationSec = 0.1f;
            EditorUtility.SetDirty(smallGun);

            UnityEngine.Debug.Log("[Phase5Setup] UFO GunData конфиги созданы/обновлены.");
        }

        private static void CreateUfoConfigs()
        {
            var bigGun = AssetDatabase.LoadAssetAtPath<GunData>("Assets/Media/configs/UfoBigGunData.asset");
            var smallGun = AssetDatabase.LoadAssetAtPath<GunData>("Assets/Media/configs/UfoSmallGunData.asset");

            // UfoBigData.asset — Large UFO: Score=200, Speed=8
            var bigPath = "Assets/Media/configs/UfoBigData.asset";
            var bigData = AssetDatabase.LoadAssetAtPath<UfoData>(bigPath);
            if (bigData == null)
            {
                bigData = ScriptableObject.CreateInstance<UfoData>();
                AssetDatabase.CreateAsset(bigData, bigPath);
            }
            bigData.Score = 200;
            bigData.Speed = 8f;
            bigData.ShootDurationSec = 1.5f;
            bigData.Gun = bigGun;
            EditorUtility.SetDirty(bigData);

            // UfoSmallData.asset — Small UFO: Score=1000, Speed=10
            var smallPath = "Assets/Media/configs/UfoSmallData.asset";
            var smallData = AssetDatabase.LoadAssetAtPath<UfoData>(smallPath);
            if (smallData == null)
            {
                smallData = ScriptableObject.CreateInstance<UfoData>();
                AssetDatabase.CreateAsset(smallData, smallPath);
            }
            smallData.Score = 1000;
            smallData.Speed = 10f;
            smallData.ShootDurationSec = 2f;
            smallData.Gun = smallGun;
            EditorUtility.SetDirty(smallData);

            UnityEngine.Debug.Log("[Phase5Setup] UfoData конфиги созданы/обновлены.");
        }

        private static void CreateUfoPrefabs()
        {
            // ufo_big.prefab — layer 8 (Enemies), collider radius 0.75
            CreateOrUpdateUfoPrefab("ufo_big", "Assets/Media/prefabs/ufo_big.prefab",
                layer: 8, colliderRadius: 0.75f, spriteName: "ufo_big");

            // ufo_small.prefab — layer 8 (Enemies), collider radius 0.4
            CreateOrUpdateUfoPrefab("ufo_small", "Assets/Media/prefabs/ufo_small.prefab",
                layer: 8, colliderRadius: 0.4f, spriteName: "ufo_small");

            // Назначить prefabs в UfoData configs
            var bigData = AssetDatabase.LoadAssetAtPath<UfoData>("Assets/Media/configs/UfoBigData.asset");
            var smallData = AssetDatabase.LoadAssetAtPath<UfoData>("Assets/Media/configs/UfoSmallData.asset");
            var bigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Media/prefabs/ufo_big.prefab");
            var smallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Media/prefabs/ufo_small.prefab");

            if (bigData != null && bigPrefab != null) { bigData.Prefab = bigPrefab; EditorUtility.SetDirty(bigData); }
            if (smallData != null && smallPrefab != null) { smallData.Prefab = smallPrefab; EditorUtility.SetDirty(smallData); }

            UnityEngine.Debug.Log("[Phase5Setup] UFO prefabs созданы/обновлены.");
        }

        private static void CreateOrUpdateUfoPrefab(string name, string path, int layer, float colliderRadius, string spriteName)
        {
            var go = new GameObject(name);
            go.layer = layer;

            var sr = go.AddComponent<SpriteRenderer>();
            var sprite = LoadSprite(spriteName);
            if (sprite != null) { sr.sprite = sprite; }

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.useFullKinematicContacts = true;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = colliderRadius;

            var ufoVisual = go.AddComponent<UfoVisual>();
            // Назначить _spriteRenderer сериализованно
            var so = new SerializedObject(ufoVisual);
            var srProp = so.FindProperty("_spriteRenderer");
            if (srProp != null) { srProp.objectReferenceValue = sr; so.ApplyModifiedProperties(); }

            SaveOrReplacePrefab(go, path);
            Object.DestroyImmediate(go);
        }

        private static void UpdateGameData()
        {
            var gameData = AssetDatabase.LoadAssetAtPath<GameData>("Assets/Media/configs/GameData.asset");
            if (gameData == null)
            {
                UnityEngine.Debug.LogError("[Phase5Setup] GameData.asset не найден!");
                return;
            }

            var bigData = AssetDatabase.LoadAssetAtPath<UfoData>("Assets/Media/configs/UfoBigData.asset");
            var smallData = AssetDatabase.LoadAssetAtPath<UfoData>("Assets/Media/configs/UfoSmallData.asset");

            var serializedGameData = new SerializedObject(gameData);
            if (bigData != null)
            {
                serializedGameData.FindProperty("UfoBig").objectReferenceValue = bigData;
            }
            if (smallData != null)
            {
                serializedGameData.FindProperty("Ufo").objectReferenceValue = smallData;
            }
            serializedGameData.ApplyModifiedProperties();

            UnityEngine.Debug.Log("[Phase5Setup] GameData.asset обновлён с UFO конфигами.");
        }

        private static void SetupWaveBanner()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");

            // Найти HudVisual через Resources (поддерживает неактивные объекты)
            var hudVisuals = Resources.FindObjectsOfTypeAll<HudVisual>();
            var hudVisual = System.Array.Find(hudVisuals, v => v.gameObject.scene == scene);
            if (hudVisual == null)
            {
                UnityEngine.Debug.LogError("[Phase5Setup] HudVisual не найден в сцене!");
                return;
            }

            var hudGo = hudVisual.gameObject;

            // Удалить существующий wave_banner_text если есть
            var existing = hudGo.transform.Find("wave_banner_text");
            if (existing != null) { Object.DestroyImmediate(existing.gameObject); }

            // Wave Banner — по центру экрана, скрыт по умолчанию
            var bannerGo = new GameObject("wave_banner_text");
            bannerGo.transform.SetParent(hudGo.transform, false);
            var bannerRect = bannerGo.AddComponent<RectTransform>();
            bannerRect.pivot = new Vector2(0.5f, 0.5f);
            bannerRect.anchorMin = new Vector2(0.5f, 0.5f);
            bannerRect.anchorMax = new Vector2(0.5f, 0.5f);
            bannerRect.anchoredPosition = new Vector2(0f, 60f);
            bannerRect.sizeDelta = new Vector2(400f, 60f);
            var bannerTmp = bannerGo.AddComponent<TextMeshProUGUI>();
            bannerTmp.text = "WAVE 1";
            bannerTmp.fontSize = 48f;
            bannerTmp.fontStyle = FontStyles.Bold;
            bannerTmp.alignment = TextAlignmentOptions.Center;
            bannerTmp.color = Color.white;
            bannerGo.SetActive(false); // скрыт по умолчанию

            // Назначить _waveBannerText в HudVisual
            var serializedHud = new SerializedObject(hudVisual);
            serializedHud.FindProperty("_waveBannerText").objectReferenceValue = bannerTmp;
            serializedHud.ApplyModifiedProperties();

            // Сохранить сцену
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

            UnityEngine.Debug.Log("[Phase5Setup] Wave banner добавлен в HUD.");
        }

        private static void SaveOrReplacePrefab(GameObject go, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) { AssetDatabase.DeleteAsset(path); }
            PrefabUtility.SaveAsPrefabAsset(go, path);
        }
    }
}
#endif
