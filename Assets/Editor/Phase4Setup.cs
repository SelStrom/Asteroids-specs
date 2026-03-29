#if UNITY_EDITOR
using SelStrom.Asteroids;
using SelStrom.Asteroids.Configs;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SelStrom.Asteroids.Editor
{
    public static class Phase4Setup
    {
        // GUID текстурного атласа asteroids.png
        private const string AtlasGuid = "cf467e92e508b5878eb24c5a126421e0";

        [MenuItem("Asteroids/Setup Phase 4 Assets")]
        public static void SetupAll()
        {
            CreateAsteroidConfigs();
            CreateAsteroidPrefabs();
            UpdateGameData();
            SetupSceneGameOver();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("[Phase4Setup] Готово. Все Phase 4 assets созданы.");
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
            UnityEngine.Debug.LogWarning($"[Phase4Setup] Спрайт не найден: {spriteName}");
            return null;
        }

        private static void CreateAsteroidConfigs()
        {
            // Обновить или создать AsteroidBigData.asset — Score=1, 3 big-спрайта
            UpdateOrCreateAsteroidConfig(
                "Assets/Media/configs/AsteroidBigData.asset",
                score: 1,
                sprites: new[] { "asteroid_big_1", "asteroid_big_2", "asteroid_big_3" }
            );

            // Обновить или создать AsteroidMediumData.asset — Score=2, 3 medium-спрайта
            UpdateOrCreateAsteroidConfig(
                "Assets/Media/configs/AsteroidMediumData.asset",
                score: 2,
                sprites: new[] { "asteroid_medium_1", "asteroid_medium_2", "asteroid_medium_3" }
            );

            // Обновить или создать AsteroidSmallData.asset — Score=3, 3 small-спрайта
            UpdateOrCreateAsteroidConfig(
                "Assets/Media/configs/AsteroidSmallData.asset",
                score: 3,
                sprites: new[] { "asteroid_small_1", "asteroid_small_2", "asteroid_small_3" }
            );

            UnityEngine.Debug.Log("[Phase4Setup] AsteroidData конфиги обновлены.");
        }

        private static void UpdateOrCreateAsteroidConfig(string path, int score, string[] sprites)
        {
            var existing = AssetDatabase.LoadAssetAtPath<AsteroidData>(path);
            if (existing == null)
            {
                existing = ScriptableObject.CreateInstance<AsteroidData>();
                AssetDatabase.CreateAsset(existing, path);
            }

            existing.Score = score;
            existing.SpriteVariants = System.Array.ConvertAll(sprites, LoadSprite);
            EditorUtility.SetDirty(existing);
        }

        private static void CreateAsteroidPrefabs()
        {
            // Prefabs уже могут существовать как YAML-файлы — обновить компоненты через PrefabUtility
            // Если prefab уже есть — пересохранить с правильными параметрами; если нет — создать
            // Радиусы рассчитаны для PPU=16 (старые значения × 6.25 = 100/16)
            CreateOrUpdateAsteroidPrefab("asteroid_big", "Assets/Media/prefabs/asteroid_big.prefab",
                layer: 8, colliderRadius: 2.5f);
            CreateOrUpdateAsteroidPrefab("asteroid_medium", "Assets/Media/prefabs/asteroid_medium.prefab",
                layer: 8, colliderRadius: 1.375f);
            CreateOrUpdateAsteroidPrefab("asteroid_small", "Assets/Media/prefabs/asteroid_small.prefab",
                layer: 8, colliderRadius: 0.9375f);

            UnityEngine.Debug.Log("[Phase4Setup] Asteroid prefabs обновлены.");
        }

        private static void CreateOrUpdateAsteroidPrefab(string name, string path, int layer, float colliderRadius)
        {
            var go = new GameObject(name);
            go.layer = layer;
            go.AddComponent<SpriteRenderer>();
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.useFullKinematicContacts = true;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = colliderRadius;
            var asteroidVisual = go.AddComponent<AsteroidVisual>();
            // Назначить _spriteRenderer сериализованно — чтобы prefab содержал правильную ссылку
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                var so = new UnityEditor.SerializedObject(asteroidVisual);
                var srProp = so.FindProperty("_spriteRenderer");
                if (srProp != null) { srProp.objectReferenceValue = sr; so.ApplyModifiedProperties(); }
            }
            SaveOrReplacePrefab(go, path);
            Object.DestroyImmediate(go);
        }

        private static void UpdateGameData()
        {
            // Загрузить существующий GameData.asset и добавить ссылки на asteroid configs
            var gameData = AssetDatabase.LoadAssetAtPath<GameData>("Assets/Media/configs/GameData.asset");
            if (gameData == null)
            {
                UnityEngine.Debug.LogError("[Phase4Setup] GameData.asset не найден!");
                return;
            }

            var bigData = AssetDatabase.LoadAssetAtPath<AsteroidData>("Assets/Media/configs/AsteroidBigData.asset");
            var medData = AssetDatabase.LoadAssetAtPath<AsteroidData>("Assets/Media/configs/AsteroidMediumData.asset");
            var smallData = AssetDatabase.LoadAssetAtPath<AsteroidData>("Assets/Media/configs/AsteroidSmallData.asset");

            var bigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Media/prefabs/asteroid_big.prefab");
            var medPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Media/prefabs/asteroid_medium.prefab");
            var smallPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Media/prefabs/asteroid_small.prefab");

            // Назначить prefabs в configs
            if (bigData != null && bigPrefab != null)
            {
                bigData.Prefab = bigPrefab;
                EditorUtility.SetDirty(bigData);
            }
            if (medData != null && medPrefab != null)
            {
                medData.Prefab = medPrefab;
                EditorUtility.SetDirty(medData);
            }
            if (smallData != null && smallPrefab != null)
            {
                smallData.Prefab = smallPrefab;
                EditorUtility.SetDirty(smallData);
            }

            // Назначить configs в GameData
            var serializedGameData = new SerializedObject(gameData);
            serializedGameData.FindProperty("AsteroidBig").objectReferenceValue = bigData;
            serializedGameData.FindProperty("AsteroidMedium").objectReferenceValue = medData;
            serializedGameData.FindProperty("AsteroidSmall").objectReferenceValue = smallData;

            // Назначить UserGunData → GameData.Ship.Gun (если ещё не назначен)
            var userGunData = AssetDatabase.LoadAssetAtPath<GunData>("Assets/Media/configs/UserGunData.asset");
            var shipProp = serializedGameData.FindProperty("Ship");
            if (shipProp != null && userGunData != null)
            {
                var gunProp = shipProp.FindPropertyRelative("Gun");
                if (gunProp != null && gunProp.objectReferenceValue == null)
                {
                    gunProp.objectReferenceValue = userGunData;
                }

                // Назначить спрайты корабля
                var mainSpriteProp = shipProp.FindPropertyRelative("MainSprite");
                var thrustSpriteProp = shipProp.FindPropertyRelative("ThrustSprite");
                if (mainSpriteProp != null && mainSpriteProp.objectReferenceValue == null)
                {
                    mainSpriteProp.objectReferenceValue = LoadSprite("ship_idle");
                }
                if (thrustSpriteProp != null && thrustSpriteProp.objectReferenceValue == null)
                {
                    thrustSpriteProp.objectReferenceValue = LoadSprite("ship_throttle");
                }

                // Назначить Damping если не задан
                var dampingProp = shipProp.FindPropertyRelative("Damping");
                if (dampingProp != null && dampingProp.floatValue == 0f)
                {
                    dampingProp.floatValue = 2f; // 2 ед/с² — плавное торможение
                }
            }

            // Назначить BulletSprite если не задан
            var bulletProp = serializedGameData.FindProperty("Bullet");
            if (bulletProp != null)
            {
                var bulletSpriteProp = bulletProp.FindPropertyRelative("BulletSprite");
                if (bulletSpriteProp != null && bulletSpriteProp.objectReferenceValue == null)
                {
                    bulletSpriteProp.objectReferenceValue = LoadSprite("bullet");
                }
            }

            serializedGameData.ApplyModifiedProperties();

            UnityEngine.Debug.Log("[Phase4Setup] GameData.asset обновлён с ссылками на asteroid configs.");
        }

        private static void SetupSceneGameOver()
        {
            // Открыть Main.unity
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");

            // Найти Canvas UI
            var canvasGo = GameObject.Find("UI");
            if (canvasGo == null)
            {
                UnityEngine.Debug.LogError("[Phase4Setup] UI Canvas не найден в сцене! Запустите Phase3Setup сначала.");
                return;
            }

            // Удалить все существующие GameOverScreen включая неактивные
            var existingGameOverScreens = Object.FindObjectsOfType<GameOverView>(true);
            foreach (var existing in existingGameOverScreens)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            // GameOverScreen — скрыт при старте (SetActive=false)
            var gameOverGo = new GameObject("GameOverScreen");
            gameOverGo.transform.SetParent(canvasGo.transform, false);
            var gameOverRect = gameOverGo.AddComponent<RectTransform>();
            gameOverRect.anchorMin = Vector2.zero;
            gameOverRect.anchorMax = Vector2.one;
            gameOverRect.offsetMin = gameOverRect.offsetMax = Vector2.zero;
            var gameOverImage = gameOverGo.AddComponent<Image>();
            gameOverImage.color = new Color(0f, 0f, 0f, 0.75f); // полупрозрачный фон
            gameOverGo.SetActive(false); // D-11: скрыт по умолчанию

            // "GAME OVER" заголовок
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(gameOverGo.transform, false);
            var titleRect = titleGo.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = new Vector2(0f, 120f);
            titleRect.sizeDelta = new Vector2(600f, 80f);
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "GAME OVER";
            titleTmp.fontSize = 64f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.color = Color.red;

            // Score text "Score: 0"
            var scoreLabelGo = new GameObject("ScoreText");
            scoreLabelGo.transform.SetParent(gameOverGo.transform, false);
            var scoreLabelRect = scoreLabelGo.AddComponent<RectTransform>();
            scoreLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
            scoreLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
            scoreLabelRect.anchoredPosition = new Vector2(0f, 40f);
            scoreLabelRect.sizeDelta = new Vector2(400f, 50f);
            var scoreTmp = scoreLabelGo.AddComponent<TextMeshProUGUI>();
            scoreTmp.text = "Score: 0";
            scoreTmp.fontSize = 36f;
            scoreTmp.alignment = TextAlignmentOptions.Center;
            scoreTmp.color = Color.white;

            // HighScore text "Best: 0"
            var highScoreLabelGo = new GameObject("HighScoreText");
            highScoreLabelGo.transform.SetParent(gameOverGo.transform, false);
            var highScoreLabelRect = highScoreLabelGo.AddComponent<RectTransform>();
            highScoreLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
            highScoreLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
            highScoreLabelRect.anchoredPosition = new Vector2(0f, -10f);
            highScoreLabelRect.sizeDelta = new Vector2(400f, 50f);
            var highScoreTmp = highScoreLabelGo.AddComponent<TextMeshProUGUI>();
            highScoreTmp.text = "Best: 0";
            highScoreTmp.fontSize = 28f;
            highScoreTmp.alignment = TextAlignmentOptions.Center;
            highScoreTmp.color = Color.yellow;

            // Play Again button — активна (D-11)
            var playAgainGo = new GameObject("PlayAgainButton", typeof(RectTransform));
            playAgainGo.transform.SetParent(gameOverGo.transform, false);
            var paRect = playAgainGo.GetComponent<RectTransform>();
            paRect.anchorMin = paRect.anchorMax = new Vector2(0.5f, 0.5f);
            paRect.anchoredPosition = new Vector2(0f, -70f);
            paRect.sizeDelta = new Vector2(220f, 60f);
            var paImage = playAgainGo.AddComponent<Image>();
            paImage.color = new Color(0.2f, 0.6f, 0.2f, 1f); // зелёный
            var paButton = playAgainGo.AddComponent<Button>();
            var paTextGo = new GameObject("Text");
            paTextGo.transform.SetParent(playAgainGo.transform, false);
            var paTmp = paTextGo.AddComponent<TextMeshProUGUI>();
            paTmp.text = "PLAY AGAIN";
            paTmp.fontSize = 24f;
            paTmp.alignment = TextAlignmentOptions.Center;
            paTmp.color = Color.white;
            var paTextRect = paTextGo.GetComponent<RectTransform>();
            paTextRect.anchorMin = Vector2.zero;
            paTextRect.anchorMax = Vector2.one;
            paTextRect.offsetMin = paTextRect.offsetMax = Vector2.zero;

            // Submit Score button — disabled (D-11)
            var submitGo = new GameObject("SubmitScoreButton", typeof(RectTransform));
            submitGo.transform.SetParent(gameOverGo.transform, false);
            var submitRect = submitGo.GetComponent<RectTransform>();
            submitRect.anchorMin = submitRect.anchorMax = new Vector2(0.5f, 0.5f);
            submitRect.anchoredPosition = new Vector2(-120f, -140f);
            submitRect.sizeDelta = new Vector2(200f, 50f);
            var submitImage = submitGo.AddComponent<Image>();
            submitImage.color = new Color(0.4f, 0.4f, 0.4f, 1f); // серый — disabled
            var submitButton = submitGo.AddComponent<Button>();
            submitButton.interactable = false; // D-11
            var submitTextGo = new GameObject("Text");
            submitTextGo.transform.SetParent(submitGo.transform, false);
            var submitTmp = submitTextGo.AddComponent<TextMeshProUGUI>();
            submitTmp.text = "SUBMIT SCORE";
            submitTmp.fontSize = 18f;
            submitTmp.alignment = TextAlignmentOptions.Center;
            submitTmp.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            var submitTextRect = submitTextGo.GetComponent<RectTransform>();
            submitTextRect.anchorMin = Vector2.zero;
            submitTextRect.anchorMax = Vector2.one;
            submitTextRect.offsetMin = submitTextRect.offsetMax = Vector2.zero;

            // Leaderboard button — disabled (D-11)
            var lbGo = new GameObject("LeaderboardButton", typeof(RectTransform));
            lbGo.transform.SetParent(gameOverGo.transform, false);
            var lbRect = lbGo.GetComponent<RectTransform>();
            lbRect.anchorMin = lbRect.anchorMax = new Vector2(0.5f, 0.5f);
            lbRect.anchoredPosition = new Vector2(120f, -140f);
            lbRect.sizeDelta = new Vector2(200f, 50f);
            var lbImage = lbGo.AddComponent<Image>();
            lbImage.color = new Color(0.4f, 0.4f, 0.4f, 1f); // серый — disabled
            var lbButton = lbGo.AddComponent<Button>();
            lbButton.interactable = false; // D-11
            var lbTextGo = new GameObject("Text");
            lbTextGo.transform.SetParent(lbGo.transform, false);
            var lbTmp = lbTextGo.AddComponent<TextMeshProUGUI>();
            lbTmp.text = "LEADERBOARD";
            lbTmp.fontSize = 18f;
            lbTmp.alignment = TextAlignmentOptions.Center;
            lbTmp.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            var lbTextRect = lbTextGo.GetComponent<RectTransform>();
            lbTextRect.anchorMin = Vector2.zero;
            lbTextRect.anchorMax = Vector2.one;
            lbTextRect.offsetMin = lbTextRect.offsetMax = Vector2.zero;

            // Добавить GameOverView компонент и связать поля
            var gameOverView = gameOverGo.AddComponent<GameOverView>();
            var serializedGov = new SerializedObject(gameOverView);
            serializedGov.FindProperty("_scoreText").objectReferenceValue = scoreTmp;
            serializedGov.FindProperty("_highScoreText").objectReferenceValue = highScoreTmp;
            serializedGov.FindProperty("_playAgainButton").objectReferenceValue = paButton;
            serializedGov.FindProperty("_submitScoreButton").objectReferenceValue = submitButton;
            serializedGov.FindProperty("_leaderboardButton").objectReferenceValue = lbButton;
            serializedGov.ApplyModifiedProperties();

            // Обновить HUD: добавить Score, HighScore, LivesContainer поля
            SetupHud(scene);

            // Назначить _gameOverView и _gameOverGo в ApplicationEntry
            var appEntryGo = GameObject.Find("ApplicationEntry");
            if (appEntryGo != null)
            {
                var appEntry = appEntryGo.GetComponent<ApplicationEntry>();
                if (appEntry != null)
                {
                    var serializedEntry = new SerializedObject(appEntry);
                    serializedEntry.FindProperty("_gameOverView").objectReferenceValue = gameOverView;
                    serializedEntry.FindProperty("_gameOverGo").objectReferenceValue = gameOverGo;
                    serializedEntry.ApplyModifiedProperties();
                }
            }

            // Назначить GameData в ApplicationEntry (если не назначен)
            AssignConfigsToEntry();

            // Сохранить сцену
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            UnityEngine.Debug.Log("[Phase4Setup] Сцена обновлена: GameOverScreen добавлен.");
        }

        private static void SetupHud(UnityEngine.SceneManagement.Scene scene)
        {
            // GameObject.Find не ищет неактивные объекты — используем Resources.FindObjectsOfTypeAll
            var hudVisuals = Resources.FindObjectsOfTypeAll<HudVisual>();
            var hudVisual = System.Array.Find(hudVisuals, v => v.gameObject.scene == scene);
            var hudGo = hudVisual != null ? hudVisual.gameObject : null;
            if (hudGo == null)
            {
                UnityEngine.Debug.LogWarning("[Phase4Setup] Hud GameObject не найден в сцене.");
                return;
            }

            UnityEngine.Debug.Log($"[Phase4Setup] Найден HudVisual на '{hudGo.name}'. Настраиваем поля...");

            // Растянуть Hud на весь экран — иначе дочерние элементы не могут использовать разные углы Canvas
            var hudRect = hudGo.GetComponent<RectTransform>();
            hudRect.anchorMin = Vector2.zero;
            hudRect.anchorMax = Vector2.one;
            hudRect.offsetMin = Vector2.zero;
            hudRect.offsetMax = Vector2.zero;

            // Удалить существующие элементы
            var existingScore = hudGo.transform.Find("score_text");
            if (existingScore != null) { Object.DestroyImmediate(existingScore.gameObject); }

            var existingHighScore = hudGo.transform.Find("high_score_text");
            if (existingHighScore != null) { Object.DestroyImmediate(existingHighScore.gameObject); }

            var existingLives = hudGo.transform.Find("lives_container");
            if (existingLives != null) { Object.DestroyImmediate(existingLives.gameObject); }

            // Score (верх-лево): anchor (0,1), pivot (0,1) — left top-aligned
            var scoreGo = new GameObject("score_text");
            scoreGo.transform.SetParent(hudGo.transform, false);
            var scoreRect = scoreGo.AddComponent<RectTransform>();
            scoreRect.pivot = new Vector2(0f, 1f);
            scoreRect.anchorMin = scoreRect.anchorMax = new Vector2(0f, 1f);
            scoreRect.anchoredPosition = new Vector2(16f, -16f);
            scoreRect.sizeDelta = new Vector2(160f, 36f);
            var scoreTmp = scoreGo.AddComponent<TextMeshProUGUI>();
            scoreTmp.text = "0";
            scoreTmp.fontSize = 30f;
            scoreTmp.fontStyle = FontStyles.Bold;
            scoreTmp.alignment = TextAlignmentOptions.Left;
            scoreTmp.color = Color.white;

            // Lives (под Score, верх-лево)
            var livesGo = new GameObject("lives_container");
            livesGo.transform.SetParent(hudGo.transform, false);
            var livesRect = livesGo.AddComponent<RectTransform>();
            livesRect.pivot = new Vector2(0f, 1f);
            livesRect.anchorMin = livesRect.anchorMax = new Vector2(0f, 1f);
            livesRect.anchoredPosition = new Vector2(16f, -58f);
            livesRect.sizeDelta = new Vector2(120f, 28f);
            var hlg = livesGo.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // HighScore (верх-право): anchor (1,1), pivot (1,1) — right top-aligned
            var highScoreGo = new GameObject("high_score_text");
            highScoreGo.transform.SetParent(hudGo.transform, false);
            var highScoreRect = highScoreGo.AddComponent<RectTransform>();
            highScoreRect.pivot = new Vector2(1f, 1f);
            highScoreRect.anchorMin = highScoreRect.anchorMax = new Vector2(1f, 1f);
            highScoreRect.anchoredPosition = new Vector2(-16f, -16f);
            highScoreRect.sizeDelta = new Vector2(180f, 30f);
            var highScoreTmp = highScoreGo.AddComponent<TextMeshProUGUI>();
            highScoreTmp.text = "BEST: 0";
            highScoreTmp.fontSize = 22f;
            highScoreTmp.alignment = TextAlignmentOptions.Right;
            highScoreTmp.color = Color.yellow;

            // Назначить ссылки в HudVisual через SerializedObject
            var serializedHud = new SerializedObject(hudVisual);
            serializedHud.FindProperty("_scoreText").objectReferenceValue = scoreTmp;
            serializedHud.FindProperty("_highScoreText").objectReferenceValue = highScoreTmp;
            serializedHud.FindProperty("_livesContainer").objectReferenceValue = livesGo.transform;

            // Назначить lifeIconSprite — ship_idle спрайт из PNG
            var shipSprite = LoadSprite("ship_idle");
            if (shipSprite != null)
            {
                serializedHud.FindProperty("_lifeIconSprite").objectReferenceValue = shipSprite;
            }

            serializedHud.ApplyModifiedProperties();

            // Диагностика: проверить что поля назначены
            var checkScore = serializedHud.FindProperty("_scoreText").objectReferenceValue;
            var checkHighScore = serializedHud.FindProperty("_highScoreText").objectReferenceValue;
            var checkLives = serializedHud.FindProperty("_livesContainer").objectReferenceValue;
            var checkIcon = serializedHud.FindProperty("_lifeIconSprite").objectReferenceValue;

            UnityEngine.Debug.Log($"[Phase4Setup] HUD обновлён:" +
                $"\n  _scoreText={checkScore?.name ?? "NULL (проверить Hud объект в сцене)"}" +
                $"\n  _highScoreText={checkHighScore?.name ?? "NULL"}" +
                $"\n  _livesContainer={checkLives?.name ?? "NULL"}" +
                $"\n  _lifeIconSprite={checkIcon?.name ?? "NULL (ship_idle спрайт не найден в PNG)"}");

        }

        private static void AssignConfigsToEntry()
        {
            var appEntryGo = GameObject.Find("ApplicationEntry");
            if (appEntryGo == null) { return; }

            var appEntry = appEntryGo.GetComponent<ApplicationEntry>();
            if (appEntry == null) { return; }

            var gameData = AssetDatabase.LoadAssetAtPath<GameData>("Assets/Media/configs/GameData.asset");
            if (gameData == null) { return; }

            var serializedEntry = new SerializedObject(appEntry);
            // Назначить _configs если ещё не назначен
            var configsProp = serializedEntry.FindProperty("_configs");
            if (configsProp.objectReferenceValue == null)
            {
                configsProp.objectReferenceValue = gameData;
                serializedEntry.ApplyModifiedProperties();
            }
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
