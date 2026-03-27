#if UNITY_EDITOR
using SelStrom.Asteroids;
using SelStrom.Asteroids.Configs;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace SelStrom.Asteroids.Editor
{
    public static class Phase3Setup
    {
        [MenuItem("Asteroids/Setup Phase 3 Assets")]
        public static void SetupAll()
        {
            CreateConfigAssets();
            CreatePrefabs();
            SetupScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("[Phase3Setup] Done. All Phase 3 assets created.");
        }

        private static void CreateConfigAssets()
        {
            // UserGunData.asset — MaxShoots=5, ReloadDurationSec=2 (D-07)
            var userGun = ScriptableObject.CreateInstance<GunData>();
            userGun.MaxShoots = 5;
            userGun.ReloadDurationSec = 2f;
            CreateOrReplaceAsset(userGun, "Assets/Media/configs/UserGunData.asset");

            // UfoGunData.asset — MaxShoots=1, ReloadDurationSec=2 (D-07)
            var ufoGun = ScriptableObject.CreateInstance<GunData>();
            ufoGun.MaxShoots = 1;
            ufoGun.ReloadDurationSec = 2f;
            CreateOrReplaceAsset(ufoGun, "Assets/Media/configs/UfoGunData.asset");

            // GameData.asset — все числа из DATA_SCHEMA (D-07)
            var gameData = ScriptableObject.CreateInstance<GameData>();
            gameData.AsteroidInitialCount = 10;
            gameData.SpawnAllowedRadius = 20;
            gameData.SpawnNewEnemyDurationSec = 25f;
            gameData.LeaderboardId = "asteroids_highscores";
            // VfxBlowPrefab = null (D-06)
            gameData.Bullet = new GameData.BulletData
            {
                LifeTimeSeconds = 2,
                Speed = 20f
                // Prefab и EnemyPrefab — назначить вручную после создания prefabs
            };
            gameData.Laser = new GameData.LaserData
            {
                BeamEffectLifetimeSec = 0.5f,
                LaserUpdateDurationSec = 10,
                LaserMaxShoots = 3
            };
            gameData.Ship = new GameData.ShipData
            {
                ThrustUnitsPerSecond = 6f,
                MaxSpeed = 15f
                // Prefab, MainSprite, ThrustSprite, Gun — назначить через Inspector
            };
            CreateOrReplaceAsset(gameData, "Assets/Media/configs/GameData.asset");

            UnityEngine.Debug.Log("[Phase3Setup] Config assets created.");
        }

        private static void CreatePrefabs()
        {
            // Создать папку gui если не существует
            if (!AssetDatabase.IsValidFolder("Assets/Media/prefabs/gui"))
            {
                AssetDatabase.CreateFolder("Assets/Media/prefabs", "gui");
            }

            // ship.prefab — Layer 7 (Player), PolygonCollider2D, Rigidbody2D Kinematic GravityScale=0
            var shipGo = new GameObject("ship");
            shipGo.layer = 7; // Player layer (D-09)
            var shipSr = shipGo.AddComponent<SpriteRenderer>();
            shipSr.sortingOrder = 0;
            var shipRb = shipGo.AddComponent<Rigidbody2D>();
            shipRb.bodyType = RigidbodyType2D.Kinematic;
            shipRb.gravityScale = 0f; // Pitfall 5
            var shipCol = shipGo.AddComponent<PolygonCollider2D>();
            // Треугольник по UI-SPEC §Prefab Inventory
            shipCol.SetPath(0, new Vector2[]
            {
                new Vector2(0f, 1f),
                new Vector2(-0.5f, -0.5f),
                new Vector2(0.5f, -0.5f)
            });
            shipGo.AddComponent<ShipVisual>();
            SaveOrReplacePrefab(shipGo, "Assets/Media/prefabs/ship.prefab");
            Object.DestroyImmediate(shipGo);

            // bullet.prefab — Layer 9 (PlayerBullet), CircleCollider2D radius=0.2
            var bulletGo = new GameObject("bullet");
            bulletGo.layer = 9; // PlayerBullet layer
            bulletGo.AddComponent<SpriteRenderer>();
            var bulletRb = bulletGo.AddComponent<Rigidbody2D>();
            bulletRb.bodyType = RigidbodyType2D.Kinematic;
            bulletRb.gravityScale = 0f;
            var bulletCol = bulletGo.AddComponent<CircleCollider2D>();
            bulletCol.radius = 0.2f;
            bulletGo.AddComponent<BulletVisual>();
            SaveOrReplacePrefab(bulletGo, "Assets/Media/prefabs/bullet.prefab");
            Object.DestroyImmediate(bulletGo);

            // bullet_enemy.prefab — Layer 10 (EnemyBullet)
            var bulletEnemyGo = new GameObject("bullet_enemy");
            bulletEnemyGo.layer = 10; // EnemyBullet layer
            bulletEnemyGo.AddComponent<SpriteRenderer>();
            var beRb = bulletEnemyGo.AddComponent<Rigidbody2D>();
            beRb.bodyType = RigidbodyType2D.Kinematic;
            beRb.gravityScale = 0f;
            var beCol = bulletEnemyGo.AddComponent<CircleCollider2D>();
            beCol.radius = 0.2f;
            bulletEnemyGo.AddComponent<BulletVisual>();
            SaveOrReplacePrefab(bulletEnemyGo, "Assets/Media/prefabs/bullet_enemy.prefab");
            Object.DestroyImmediate(bulletEnemyGo);

            UnityEngine.Debug.Log("[Phase3Setup] Prefabs created.");
        }

        private static void SetupScene()
        {
            // Открыть Main.unity
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");

            // Настроить Main Camera
            var cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 22.5f; // UI-SPEC §Camera Contract
                cam.transform.position = new Vector3(0f, 0f, -10f);
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
            }

            // Удалить старый ApplicationEntry если есть
            var oldEntry = GameObject.Find("ApplicationEntry");
            if (oldEntry != null) { Object.DestroyImmediate(oldEntry); }

            // Удалить старый UI canvas если есть
            var oldUi = GameObject.Find("UI");
            if (oldUi != null) { Object.DestroyImmediate(oldUi); }

            // Создать ApplicationEntry GameObject
            var appEntryGo = new GameObject("ApplicationEntry");
            appEntryGo.AddComponent<ApplicationEntry>();

            // Создать Canvas
            var canvasGo = new GameObject("UI");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 1f;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0f; // Match Width
            canvasGo.AddComponent<GraphicRaycaster>();

            // EventSystem для UI
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<InputSystemUIInputModule>();

            // TitleScreen — активен при старте
            var titleGo = new GameObject("TitleScreen");
            titleGo.transform.SetParent(canvasGo.transform, false);
            var titleRect = titleGo.AddComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = titleRect.offsetMax = Vector2.zero;

            // Title label "ASTEROIDS" size=56
            var titleLabel = new GameObject("Title");
            titleLabel.transform.SetParent(titleGo.transform, false);
            var titleTmp = titleLabel.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "ASTEROIDS";
            titleTmp.fontSize = 56f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            var titleLabelRect = titleLabel.GetComponent<RectTransform>();
            titleLabelRect.anchorMin = new Vector2(0.5f, 1f);
            titleLabelRect.anchorMax = new Vector2(0.5f, 1f);
            titleLabelRect.anchoredPosition = new Vector2(0f, -120f);
            titleLabelRect.sizeDelta = new Vector2(600f, 80f);

            // Play button "PLAY"
            var playGo = new GameObject("Play");
            playGo.transform.SetParent(titleGo.transform, false);
            playGo.AddComponent<Button>();
            var playRect = playGo.GetComponent<RectTransform>();
            playRect.anchorMin = playRect.anchorMax = new Vector2(0.5f, 0.5f);
            playRect.anchoredPosition = new Vector2(0f, -40f);
            playRect.sizeDelta = new Vector2(200f, 60f);
            var playTextGo = new GameObject("Text");
            playTextGo.transform.SetParent(playGo.transform, false);
            var playTmp = playTextGo.AddComponent<TextMeshProUGUI>();
            playTmp.text = "PLAY";
            playTmp.fontSize = 28f;
            playTmp.alignment = TextAlignmentOptions.Center;
            var playTextRect = playTextGo.GetComponent<RectTransform>();
            playTextRect.anchorMin = Vector2.zero;
            playTextRect.anchorMax = Vector2.one;
            playTextRect.offsetMin = playTextRect.offsetMax = Vector2.zero;
            titleGo.AddComponent<TitleScreenView>();

            // HUD — неактивен при старте (active=false)
            var hudGo = new GameObject("Hud");
            hudGo.transform.SetParent(canvasGo.transform, false);
            hudGo.SetActive(false);
            var hudRect = hudGo.AddComponent<RectTransform>();
            hudRect.anchorMin = new Vector2(0f, 1f);
            hudRect.anchorMax = new Vector2(0f, 1f);
            hudRect.anchoredPosition = new Vector2(16f, -16f);
            hudRect.sizeDelta = new Vector2(300f, 150f);
            // 5 TMP меток с шагом 24px
            CreateHudLabel(hudGo.transform, "coordinates",      new Vector2(0f, 0f),    "X: 0.0  Y: 0.0");
            CreateHudLabel(hudGo.transform, "rotation_angle",   new Vector2(0f, -24f),  "Angle: 0");
            CreateHudLabel(hudGo.transform, "speed_text",       new Vector2(0f, -48f),  "Speed: 0.0");
            CreateHudLabel(hudGo.transform, "laser_shoot_count", new Vector2(0f, -72f), "Laser: 3/3");
            CreateHudLabel(hudGo.transform, "laser_reload_time", new Vector2(0f, -96f), "Reload: 0.0s");
            var hudVisual = hudGo.AddComponent<HudVisual>();

            // Назначить ссылки в ApplicationEntry
            var appEntry = appEntryGo.GetComponent<ApplicationEntry>();
            var serializedEntry = new SerializedObject(appEntry);
            serializedEntry.FindProperty("_hudVisual").objectReferenceValue = hudVisual;
            serializedEntry.FindProperty("_titleScreenView").objectReferenceValue = titleGo.GetComponent<TitleScreenView>();
            serializedEntry.FindProperty("_titleScreenGo").objectReferenceValue = titleGo;
            serializedEntry.FindProperty("_hudGo").objectReferenceValue = hudGo;
            serializedEntry.ApplyModifiedProperties();

            // Сохранить сцену
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            UnityEngine.Debug.Log("[Phase3Setup] Scene configured.");
        }

        private static void CreateHudLabel(Transform parent, string name, Vector2 pos, string defaultText)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(280f, 24f);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.fontSize = 14f;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.enableWordWrapping = false;
        }

        private static void CreateOrReplaceAsset(Object obj, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (existing != null) { AssetDatabase.DeleteAsset(path); }
            AssetDatabase.CreateAsset(obj, path);
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
