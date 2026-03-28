#if UNITY_EDITOR
using SelStrom.Asteroids;
using SelStrom.Asteroids.Configs;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace SelStrom.Asteroids.Editor
{
    public static class Phase6Setup
    {
        private const string AtlasGuid = "cf467e92e508b5878eb24c5a126421e0";
        private const string AudioDataPath = "Assets/Media/configs/AudioData.asset";
        private const string VfxBlowPrefabPath = "Assets/Media/prefabs/vfx_blow.prefab";
        private const string VfxBlowMatPath = "Assets/Media/effects/vfx_blow_mat.mat";
        private const string ScenePath = "Assets/Scenes/Main.unity";

        [MenuItem("Asteroids/Setup Phase 6 Assets")]
        public static void SetupAll()
        {
            CreateAudioDataAsset();
            CreateVfxBlowPrefab();
            SetupAudioManagerInScene();
            SetupLeaderboardScreen();
            UpdateTitleScreen();
            UpdateCamera();
            UpdateGameData();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("[Phase6Setup] Готово. Все Phase 6 assets созданы.");
        }

        // --- Вспомогательные методы ---

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
            UnityEngine.Debug.LogWarning($"[Phase6Setup] Спрайт не найден: {spriteName}");
            return null;
        }

        private static void SaveOrReplacePrefab(GameObject go, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) { AssetDatabase.DeleteAsset(path); }
            PrefabUtility.SaveAsPrefabAsset(go, path);
        }

        // --- Шаг 1: AudioData.asset ---

        private static void CreateAudioDataAsset()
        {
            var audioData = AssetDatabase.LoadAssetAtPath<AudioData>(AudioDataPath);
            if (audioData == null)
            {
                audioData = ScriptableObject.CreateInstance<AudioData>();
                AssetDatabase.CreateAsset(audioData, AudioDataPath);
            }
            // AudioClip поля оставить null — пользователь назначит вручную
            EditorUtility.SetDirty(audioData);
            UnityEngine.Debug.Log("[Phase6Setup] AudioData.asset создан/найден.");
        }

        // --- Шаг 2: vfx_blow.prefab с ParticleSystem и EffectVisual ---

        private static void CreateVfxBlowPrefab()
        {
            var go = new GameObject("vfx_blow");
            go.layer = 0;

            var ps = go.AddComponent<ParticleSystem>();

            // Настроить MainModule
            var main = ps.main;
            main.loop = false;
            main.stopAction = ParticleSystemStopAction.Callback; // критично для OnParticleSystemStopped
            main.startLifetime = 0.5f;
            main.startSpeed = 5f;
            main.startSize = 0.8f;
            main.maxParticles = 20;

            // Burst: 15 частиц при старте
            var emission = ps.emission;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, 15));

            // Настроить Renderer — использовать встроенный Default-Particle материал (гарантированно работает)
            var psr = go.GetComponent<ParticleSystemRenderer>();
            if (psr != null)
            {
                psr.renderMode = ParticleSystemRenderMode.Billboard;
                var mat = Resources.GetBuiltinResource<Material>("Default-Particle.mat");
                psr.sharedMaterial = mat;
            }

            // Добавить EffectVisual компонент
            var effectVisual = go.AddComponent<EffectVisual>();

            // Назначить ParticleSystem в SerializedField _particles
            var so = new SerializedObject(effectVisual);
            var particlesProp = so.FindProperty("_particles");
            if (particlesProp != null)
            {
                particlesProp.objectReferenceValue = ps;
                so.ApplyModifiedProperties();
            }

            SaveOrReplacePrefab(go, VfxBlowPrefabPath);
            Object.DestroyImmediate(go);

            UnityEngine.Debug.Log("[Phase6Setup] vfx_blow.prefab создан.");
        }

        // --- Шаг 3: AudioManager в сцене ---

        private static void SetupAudioManagerInScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);

            // Удалить существующий AudioManager если есть
            var existingManagers = Resources.FindObjectsOfTypeAll<AudioManager>();
            foreach (var existing in existingManagers)
            {
                if (existing.gameObject.scene == scene)
                {
                    Object.DestroyImmediate(existing.gameObject);
                }
            }

            // Создать AudioManager GameObject на root сцены
            var audioManagerGo = new GameObject("AudioManager");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(audioManagerGo, scene);
            var audioManager = audioManagerGo.AddComponent<AudioManager>();

            // Загрузить AudioData
            var audioData = AssetDatabase.LoadAssetAtPath<AudioData>(AudioDataPath);

            // Создать 9 дочерних AudioSource
            var shootSrc = CreateAudioSourceChild(audioManagerGo, "shoot", loop: false);
            var thrustSrc = CreateAudioSourceChild(audioManagerGo, "thrust", loop: true);
            var explodeShipSrc = CreateAudioSourceChild(audioManagerGo, "explodeShip", loop: false);
            var explodeAsteroidBigSrc = CreateAudioSourceChild(audioManagerGo, "explodeAsteroidBig", loop: false);
            var explodeAsteroidMediumSrc = CreateAudioSourceChild(audioManagerGo, "explodeAsteroidMedium", loop: false);
            var explodeAsteroidSmallSrc = CreateAudioSourceChild(audioManagerGo, "explodeAsteroidSmall", loop: false);
            var ufoToneSrc = CreateAudioSourceChild(audioManagerGo, "ufoTone", loop: true);
            var beatLowSrc = CreateAudioSourceChild(audioManagerGo, "beatLow", loop: false);
            var beatHighSrc = CreateAudioSourceChild(audioManagerGo, "beatHigh", loop: false);

            // Назначить AudioSource и AudioData в AudioManager через SerializedObject
            var soManager = new SerializedObject(audioManager);
            soManager.FindProperty("_data").objectReferenceValue = audioData;
            soManager.FindProperty("_shoot").objectReferenceValue = shootSrc;
            soManager.FindProperty("_thrust").objectReferenceValue = thrustSrc;
            soManager.FindProperty("_explodeShip").objectReferenceValue = explodeShipSrc;
            soManager.FindProperty("_explodeAsteroidBig").objectReferenceValue = explodeAsteroidBigSrc;
            soManager.FindProperty("_explodeAsteroidMedium").objectReferenceValue = explodeAsteroidMediumSrc;
            soManager.FindProperty("_explodeAsteroidSmall").objectReferenceValue = explodeAsteroidSmallSrc;
            soManager.FindProperty("_ufoTone").objectReferenceValue = ufoToneSrc;
            soManager.FindProperty("_beatLow").objectReferenceValue = beatLowSrc;
            soManager.FindProperty("_beatHigh").objectReferenceValue = beatHighSrc;
            soManager.ApplyModifiedProperties();

            // Назначить AudioManager в ApplicationEntry
            var appEntries = Resources.FindObjectsOfTypeAll<ApplicationEntry>();
            var appEntry = System.Array.Find(appEntries, e => e.gameObject.scene == scene);
            if (appEntry != null)
            {
                var soEntry = new SerializedObject(appEntry);
                soEntry.FindProperty("_audioManager").objectReferenceValue = audioManager;
                soEntry.ApplyModifiedProperties();
            }
            else
            {
                UnityEngine.Debug.LogWarning("[Phase6Setup] ApplicationEntry не найден в сцене!");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            UnityEngine.Debug.Log("[Phase6Setup] AudioManager с 9 AudioSource добавлен в сцену.");
        }

        private static AudioSource CreateAudioSourceChild(GameObject parent, string name, bool loop)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            var src = child.AddComponent<AudioSource>();
            src.loop = loop;
            src.playOnAwake = false;
            return src;
        }

        // --- Шаг 4: LeaderboardScreen в Canvas ---

        private static void SetupLeaderboardScreen()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);

            // Удалить существующий LeaderboardScreen если есть
            var existingViews = Resources.FindObjectsOfTypeAll<LeaderboardView>();
            foreach (var existing in existingViews)
            {
                if (existing.gameObject.scene == scene)
                {
                    Object.DestroyImmediate(existing.gameObject);
                }
            }

            // Найти UI Canvas
            var canvasGo = GameObject.Find("UI");
            if (canvasGo == null)
            {
                UnityEngine.Debug.LogError("[Phase6Setup] UI Canvas не найден в сцене!");
                return;
            }

            // Создать LeaderboardScreen
            var leaderboardGo = new GameObject("LeaderboardScreen");
            leaderboardGo.transform.SetParent(canvasGo.transform, false);
            var leaderboardRect = leaderboardGo.AddComponent<RectTransform>();
            leaderboardRect.anchorMin = Vector2.zero;
            leaderboardRect.anchorMax = Vector2.one;
            leaderboardRect.offsetMin = Vector2.zero;
            leaderboardRect.offsetMax = Vector2.zero;
            var leaderboardImage = leaderboardGo.AddComponent<Image>();
            leaderboardImage.color = new Color(0f, 0f, 0f, 0.9f);

            // "LEADERBOARD" заголовок
            var titleGo = new GameObject("title_text");
            titleGo.transform.SetParent(leaderboardGo.transform, false);
            var titleRect = titleGo.AddComponent<RectTransform>();
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -80f);
            titleRect.sizeDelta = new Vector2(600f, 80f);
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "LEADERBOARD";
            titleTmp.fontSize = 64f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = Color.white;

            // Placeholder text
            var placeholderGo = new GameObject("placeholder_text");
            placeholderGo.transform.SetParent(leaderboardGo.transform, false);
            var placeholderRect = placeholderGo.AddComponent<RectTransform>();
            placeholderRect.pivot = new Vector2(0.5f, 0.5f);
            placeholderRect.anchorMin = new Vector2(0.5f, 0.5f);
            placeholderRect.anchorMax = new Vector2(0.5f, 0.5f);
            placeholderRect.anchoredPosition = Vector2.zero;
            placeholderRect.sizeDelta = new Vector2(500f, 50f);
            var placeholderTmp = placeholderGo.AddComponent<TextMeshProUGUI>();
            placeholderTmp.text = "— Доступно в Phase 7 —";
            placeholderTmp.fontSize = 32f;
            placeholderTmp.alignment = TextAlignmentOptions.Center;
            placeholderTmp.color = Color.gray;

            // Back button
            var backGo = new GameObject("back_button", typeof(RectTransform));
            backGo.transform.SetParent(leaderboardGo.transform, false);
            var backRect = backGo.GetComponent<RectTransform>();
            backRect.pivot = new Vector2(0.5f, 0f);
            backRect.anchorMin = new Vector2(0.5f, 0f);
            backRect.anchorMax = new Vector2(0.5f, 0f);
            backRect.anchoredPosition = new Vector2(0f, 60f);
            backRect.sizeDelta = new Vector2(180f, 50f);
            var backImage = backGo.AddComponent<Image>();
            backImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            var backButton = backGo.AddComponent<Button>();
            var backTextGo = new GameObject("Text");
            backTextGo.transform.SetParent(backGo.transform, false);
            var backTmp = backTextGo.AddComponent<TextMeshProUGUI>();
            backTmp.text = "< BACK";
            backTmp.fontSize = 24f;
            backTmp.alignment = TextAlignmentOptions.Center;
            backTmp.color = Color.white;
            var backTextRect = backTextGo.GetComponent<RectTransform>();
            backTextRect.anchorMin = Vector2.zero;
            backTextRect.anchorMax = Vector2.one;
            backTextRect.offsetMin = backTextRect.offsetMax = Vector2.zero;

            // Добавить LeaderboardView компонент и назначить поля
            var leaderboardView = leaderboardGo.AddComponent<LeaderboardView>();
            var soLeaderboard = new SerializedObject(leaderboardView);
            soLeaderboard.FindProperty("_titleText").objectReferenceValue = titleTmp;
            soLeaderboard.FindProperty("_placeholderText").objectReferenceValue = placeholderTmp;
            soLeaderboard.FindProperty("_backButton").objectReferenceValue = backButton;
            soLeaderboard.ApplyModifiedProperties();

            // Скрыть экран по умолчанию
            leaderboardGo.SetActive(false);

            // Назначить LeaderboardView и leaderboardGo в ApplicationEntry
            var appEntries = Resources.FindObjectsOfTypeAll<ApplicationEntry>();
            var appEntry = System.Array.Find(appEntries, e => e.gameObject.scene == scene);
            if (appEntry != null)
            {
                var soEntry = new SerializedObject(appEntry);
                soEntry.FindProperty("_leaderboardView").objectReferenceValue = leaderboardView;
                soEntry.FindProperty("_leaderboardGo").objectReferenceValue = leaderboardGo;
                soEntry.ApplyModifiedProperties();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            UnityEngine.Debug.Log("[Phase6Setup] LeaderboardScreen создан и добавлен в Canvas.");
        }

        // --- Шаг 5: Обновить TitleScreen ---

        private static void UpdateTitleScreen()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);

            var titleViews = Resources.FindObjectsOfTypeAll<TitleScreenView>();
            var titleView = System.Array.Find(titleViews, v => v.gameObject.scene == scene);
            if (titleView == null)
            {
                UnityEngine.Debug.LogError("[Phase6Setup] TitleScreenView не найден в сцене!");
                return;
            }

            var titleGo = titleView.gameObject;

            // Удалить старый Title (Phase3Setup legacy)
            var oldTitle = titleGo.transform.Find("Title");
            if (oldTitle != null)
            {
                Object.DestroyImmediate(oldTitle.gameObject);
            }

            // Удалить существующий _titleText (по ссылке из SerializedObject), затем по имени
            var soTitleClean = new SerializedObject(titleView);
            var existingTitleRef = soTitleClean.FindProperty("_titleText")?.objectReferenceValue as TextMeshProUGUI;
            if (existingTitleRef != null && existingTitleRef.gameObject != null)
            {
                Object.DestroyImmediate(existingTitleRef.gameObject);
            }
            var existingTitle = titleGo.transform.Find("title_text");
            if (existingTitle != null) { Object.DestroyImmediate(existingTitle.gameObject); }
            var existingLbBtn = titleGo.transform.Find("leaderboard_button");
            if (existingLbBtn != null) { Object.DestroyImmediate(existingLbBtn.gameObject); }

            // "ASTEROIDS" заголовок — по центру
            var titleTextGo = new GameObject("title_text");
            titleTextGo.transform.SetParent(titleGo.transform, false);
            var titleTextRect = titleTextGo.AddComponent<RectTransform>();
            titleTextRect.pivot = new Vector2(0.5f, 0.5f);
            titleTextRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleTextRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleTextRect.anchoredPosition = new Vector2(0f, 100f);
            titleTextRect.sizeDelta = new Vector2(600f, 120f);
            var titleTmp = titleTextGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "ASTEROIDS";
            titleTmp.fontSize = 96f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = Color.white;

            // Leaderboard button — под Play кнопкой (disabled до Phase 7)
            var lbBtnGo = new GameObject("leaderboard_button", typeof(RectTransform));
            lbBtnGo.transform.SetParent(titleGo.transform, false);
            var lbBtnRect = lbBtnGo.GetComponent<RectTransform>();
            lbBtnRect.pivot = new Vector2(0.5f, 0.5f);
            lbBtnRect.anchorMin = new Vector2(0.5f, 0.5f);
            lbBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
            lbBtnRect.anchoredPosition = new Vector2(0f, -80f);
            lbBtnRect.sizeDelta = new Vector2(220f, 50f);
            var lbBtnImage = lbBtnGo.AddComponent<Image>();
            lbBtnImage.color = new Color(0.4f, 0.4f, 0.4f, 1f);
            var lbButton = lbBtnGo.AddComponent<Button>();
            lbButton.interactable = false; // disabled до Phase 7
            var lbTextGo = new GameObject("Text");
            lbTextGo.transform.SetParent(lbBtnGo.transform, false);
            var lbTmp = lbTextGo.AddComponent<TextMeshProUGUI>();
            lbTmp.text = "LEADERBOARD";
            lbTmp.fontSize = 20f;
            lbTmp.alignment = TextAlignmentOptions.Center;
            lbTmp.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            var lbTextRect = lbTextGo.GetComponent<RectTransform>();
            lbTextRect.anchorMin = Vector2.zero;
            lbTextRect.anchorMax = Vector2.one;
            lbTextRect.offsetMin = lbTextRect.offsetMax = Vector2.zero;

            // Назначить поля в TitleScreenView через SerializedObject
            var soTitle = new SerializedObject(titleView);
            soTitle.FindProperty("_titleText").objectReferenceValue = titleTmp;
            soTitle.FindProperty("_leaderboardButton").objectReferenceValue = lbButton;
            soTitle.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            UnityEngine.Debug.Log("[Phase6Setup] TitleScreen обновлён: заголовок ASTEROIDS и кнопка Leaderboard.");
        }

        // --- Шаг 6: Камера — чёрный фон ---

        private static void UpdateCamera()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);

            var cam = Camera.main;
            if (cam == null)
            {
                // Попробовать найти через Resources (поддерживает неактивные)
                var cameras = Resources.FindObjectsOfTypeAll<Camera>();
                cam = System.Array.Find(cameras, c => c.gameObject.scene == scene);
            }

            if (cam == null)
            {
                UnityEngine.Debug.LogError("[Phase6Setup] Camera не найдена в сцене!");
                return;
            }

            cam.backgroundColor = Color.black;
            cam.clearFlags = CameraClearFlags.SolidColor;
            EditorUtility.SetDirty(cam);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            UnityEngine.Debug.Log("[Phase6Setup] Camera.backgroundColor = black.");
        }

        // --- Шаг 7: GameData — ссылка на AudioData ---

        private static void UpdateGameData()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);

            var gameData = AssetDatabase.LoadAssetAtPath<GameData>("Assets/Media/configs/GameData.asset");
            if (gameData == null)
            {
                UnityEngine.Debug.LogError("[Phase6Setup] GameData.asset не найден!");
                return;
            }

            var audioData = AssetDatabase.LoadAssetAtPath<AudioData>(AudioDataPath);
            var vfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(VfxBlowPrefabPath);

            var soGameData = new SerializedObject(gameData);
            if (audioData != null)
            {
                soGameData.FindProperty("Audio").objectReferenceValue = audioData;
            }
            if (vfxPrefab != null)
            {
                soGameData.FindProperty("VfxBlowPrefab").objectReferenceValue = vfxPrefab;
            }
            soGameData.ApplyModifiedProperties();

            // Назначить VfxBlowPrefab в ApplicationEntry (через GameData или напрямую если нужно)
            var appEntries = Resources.FindObjectsOfTypeAll<ApplicationEntry>();
            var appEntry = System.Array.Find(appEntries, e => e.gameObject.scene == scene);
            if (appEntry != null)
            {
                // Проверить что configs назначен
                var soEntry = new SerializedObject(appEntry);
                var configsProp = soEntry.FindProperty("_configs");
                if (configsProp.objectReferenceValue == null)
                {
                    configsProp.objectReferenceValue = gameData;
                    soEntry.ApplyModifiedProperties();
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            UnityEngine.Debug.Log("[Phase6Setup] GameData.asset обновлён: Audio и VfxBlowPrefab назначены.");
        }
    }
}
#endif
