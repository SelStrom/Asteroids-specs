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
    public static class Phase7Setup
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";
        private const string GameDataPath = "Assets/Media/configs/GameData.asset";

        [MenuItem("Asteroids/Setup Phase 7 Assets")]
        public static void SetupAll()
        {
            UpdateGameOverScreen();
            UpdateLeaderboardScreen();
            EnableLeaderboardButton();
            UpdateGameDataLeaderboardId();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("[Phase7Setup] Готово. Все Phase 7 assets настроены.");
        }

        // --- Шаг 1: Обновить GameOverScreen — добавить InputField и errorText ---

        private static void UpdateGameOverScreen()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);

            // Найти GameOverView в сцене
            var views = Resources.FindObjectsOfTypeAll<GameOverView>();
            var view = System.Array.Find(views, v => v.gameObject.scene == scene);
            if (view == null)
            {
                UnityEngine.Debug.LogError("[Phase7Setup] GameOverView не найден в сцене!");
                return;
            }

            var screenGo = view.gameObject;

            // Удалить старые элементы если они уже есть (idempotency)
            var existingInput = screenGo.transform.Find("player_name_input");
            if (existingInput != null) { Object.DestroyImmediate(existingInput.gameObject); }
            var existingError = screenGo.transform.Find("error_text");
            if (existingError != null) { Object.DestroyImmediate(existingError.gameObject); }

            // Создать player_name_input (TMP_InputField)
            var inputGo = new GameObject("player_name_input");
            inputGo.transform.SetParent(screenGo.transform, false);
            var inputRect = inputGo.AddComponent<RectTransform>();
            inputRect.pivot = new Vector2(0.5f, 0.5f);
            inputRect.anchorMin = new Vector2(0.5f, 0.5f);
            inputRect.anchorMax = new Vector2(0.5f, 0.5f);
            // Располагаем над кнопкой Submit Score (примерно по центру-верхней части)
            inputRect.anchoredPosition = new Vector2(0f, 20f);
            inputRect.sizeDelta = new Vector2(240f, 40f);

            var inputImage = inputGo.AddComponent<Image>();
            inputImage.color = new Color(0.102f, 0.102f, 0.102f, 1f); // #1A1A1A

            // Дочерний placeholder текст
            var placeholderGo = new GameObject("Placeholder");
            placeholderGo.transform.SetParent(inputGo.transform, false);
            var placeholderRect = placeholderGo.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = new Vector2(8f, 2f);
            placeholderRect.offsetMax = new Vector2(-8f, -2f);
            var placeholderTmp = placeholderGo.AddComponent<TextMeshProUGUI>();
            placeholderTmp.text = "Введите имя";
            placeholderTmp.fontSize = 18f;
            placeholderTmp.color = new Color(1f, 1f, 1f, 0.5f);
            placeholderTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Дочерний текст ввода
            var textAreaGo = new GameObject("Text Area");
            textAreaGo.transform.SetParent(inputGo.transform, false);
            var textAreaRect = textAreaGo.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(8f, 2f);
            textAreaRect.offsetMax = new Vector2(-8f, -2f);
            var textAreaMask = textAreaGo.AddComponent<RectMask2D>();

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(textAreaGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var textTmp = textGo.AddComponent<TextMeshProUGUI>();
            textTmp.text = "";
            textTmp.fontSize = 18f;
            textTmp.color = Color.white;
            textTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Добавить TMP_InputField и настроить
            var inputField = inputGo.AddComponent<TMP_InputField>();
            inputField.characterLimit = 16;
            inputField.textViewport = textAreaRect;
            inputField.textComponent = textTmp;
            inputField.placeholder = placeholderTmp;
            inputField.contentType = TMP_InputField.ContentType.Alphanumeric;

            // Создать error_text (TextMeshProUGUI) для GameOverView
            var errorGo = new GameObject("error_text");
            errorGo.transform.SetParent(screenGo.transform, false);
            var errorRect = errorGo.AddComponent<RectTransform>();
            errorRect.pivot = new Vector2(0.5f, 0.5f);
            errorRect.anchorMin = new Vector2(0.5f, 0.5f);
            errorRect.anchorMax = new Vector2(0.5f, 0.5f);
            errorRect.anchoredPosition = new Vector2(0f, -120f);
            errorRect.sizeDelta = new Vector2(400f, 30f);
            var errorTmp = errorGo.AddComponent<TextMeshProUGUI>();
            errorTmp.text = "";
            errorTmp.fontSize = 14f;
            errorTmp.color = new Color(1f, 0.267f, 0.267f, 1f); // #FF4444
            errorTmp.alignment = TextAlignmentOptions.Center;
            errorGo.SetActive(false);

            // Назначить SerializedFields через SerializedObject
            var soView = new SerializedObject(view);
            soView.FindProperty("_playerNameInput").objectReferenceValue = inputField;
            soView.FindProperty("_errorText").objectReferenceValue = errorTmp;
            soView.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            UnityEngine.Debug.Log("[Phase7Setup] GameOverScreen: player_name_input и error_text добавлены.");
        }

        // --- Шаг 2: Обновить LeaderboardScreen — добавить 10 строк, разделитель, playerEntry, errorText ---

        private static void UpdateLeaderboardScreen()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);

            // Найти LeaderboardView в сцене
            var views = Resources.FindObjectsOfTypeAll<LeaderboardView>();
            var view = System.Array.Find(views, v => v.gameObject.scene == scene);
            if (view == null)
            {
                UnityEngine.Debug.LogError("[Phase7Setup] LeaderboardView не найден в сцене!");
                return;
            }

            var screenGo = view.gameObject;

            // Скрыть существующий placeholder — он используется как "Загрузка..."
            var placeholderTransform = screenGo.transform.Find("placeholder_text");
            if (placeholderTransform != null)
            {
                placeholderTransform.gameObject.SetActive(false);
            }

            // Удалить старые элементы если они уже есть (idempotency)
            for (int i = 0; i < 10; i++)
            {
                var old = screenGo.transform.Find($"_entry_{i}");
                if (old != null) { Object.DestroyImmediate(old.gameObject); }
            }
            var oldSep = screenGo.transform.Find("_separator");
            if (oldSep != null) { Object.DestroyImmediate(oldSep.gameObject); }
            var oldPlayerEntry = screenGo.transform.Find("_player_entry");
            if (oldPlayerEntry != null) { Object.DestroyImmediate(oldPlayerEntry.gameObject); }
            var oldError = screenGo.transform.Find("_error_text");
            if (oldError != null) { Object.DestroyImmediate(oldError.gameObject); }

            // Создать 10 строк Top-10
            // Layout: под заголовком (Y=-80 заголовок, 2xl=48px под ним → первая строка Y=-160)
            // Шаг: высота строки ~22px + sm=8px = 30px
            var entryTexts = new TextMeshProUGUI[10];
            float startY = -160f;
            float stepY = -30f;

            for (int i = 0; i < 10; i++)
            {
                var entryGo = new GameObject($"_entry_{i}");
                entryGo.transform.SetParent(screenGo.transform, false);
                var entryRect = entryGo.AddComponent<RectTransform>();
                entryRect.pivot = new Vector2(0.5f, 1f);
                entryRect.anchorMin = new Vector2(0.5f, 1f);
                entryRect.anchorMax = new Vector2(0.5f, 1f);
                entryRect.anchoredPosition = new Vector2(0f, startY + i * stepY);
                entryRect.sizeDelta = new Vector2(500f, 22f);
                var entryTmp = entryGo.AddComponent<TextMeshProUGUI>();
                entryTmp.text = "";
                entryTmp.fontSize = 18f;
                entryTmp.color = Color.white;
                entryTmp.alignment = TextAlignmentOptions.MidlineLeft;
                entryGo.SetActive(false);
                entryTexts[i] = entryTmp;
            }

            // Разделитель: Y после 10-й строки + xl=32px
            // Последняя строка: startY + 9 * stepY = -160 + 9 * (-30) = -160 - 270 = -430
            // Разделитель: -430 - 22 (высота строки) - 32 = -484 → округлим до -490
            float separatorY = startY + 9 * stepY - 22f - 32f;
            var separatorGo = new GameObject("_separator");
            separatorGo.transform.SetParent(screenGo.transform, false);
            var separatorRect = separatorGo.AddComponent<RectTransform>();
            separatorRect.pivot = new Vector2(0.5f, 1f);
            separatorRect.anchorMin = new Vector2(0.5f, 1f);
            separatorRect.anchorMax = new Vector2(0.5f, 1f);
            separatorRect.anchoredPosition = new Vector2(0f, separatorY);
            separatorRect.sizeDelta = new Vector2(500f, 1f);
            var separatorImage = separatorGo.AddComponent<Image>();
            separatorImage.color = Color.white;

            // Строка текущего игрока: Y после разделителя + lg=24px
            float playerEntryY = separatorY - 1f - 24f;
            var playerEntryGo = new GameObject("_player_entry");
            playerEntryGo.transform.SetParent(screenGo.transform, false);
            var playerEntryRect = playerEntryGo.AddComponent<RectTransform>();
            playerEntryRect.pivot = new Vector2(0.5f, 1f);
            playerEntryRect.anchorMin = new Vector2(0.5f, 1f);
            playerEntryRect.anchorMax = new Vector2(0.5f, 1f);
            playerEntryRect.anchoredPosition = new Vector2(0f, playerEntryY);
            playerEntryRect.sizeDelta = new Vector2(500f, 22f);
            var playerEntryTmp = playerEntryGo.AddComponent<TextMeshProUGUI>();
            playerEntryTmp.text = "";
            playerEntryTmp.fontSize = 18f;
            playerEntryTmp.color = Color.white;
            playerEntryTmp.alignment = TextAlignmentOptions.MidlineLeft;
            playerEntryGo.SetActive(false);

            // errorText для LeaderboardView
            float errorY = playerEntryY - 22f - 16f;
            var errorGo = new GameObject("_error_text");
            errorGo.transform.SetParent(screenGo.transform, false);
            var errorRect = errorGo.AddComponent<RectTransform>();
            errorRect.pivot = new Vector2(0.5f, 1f);
            errorRect.anchorMin = new Vector2(0.5f, 1f);
            errorRect.anchorMax = new Vector2(0.5f, 1f);
            errorRect.anchoredPosition = new Vector2(0f, errorY);
            errorRect.sizeDelta = new Vector2(500f, 30f);
            var errorTmp = errorGo.AddComponent<TextMeshProUGUI>();
            errorTmp.text = "";
            errorTmp.fontSize = 14f;
            errorTmp.color = new Color(1f, 0.267f, 0.267f, 1f); // #FF4444
            errorTmp.alignment = TextAlignmentOptions.Center;
            errorGo.SetActive(false);

            // Назначить SerializedFields через SerializedObject
            var soView = new SerializedObject(view);
            var entryTextsProp = soView.FindProperty("_entryTexts");
            entryTextsProp.arraySize = 10;
            for (int i = 0; i < 10; i++)
            {
                entryTextsProp.GetArrayElementAtIndex(i).objectReferenceValue = entryTexts[i];
            }
            soView.FindProperty("_playerEntryText").objectReferenceValue = playerEntryTmp;
            soView.FindProperty("_errorText").objectReferenceValue = errorTmp;
            soView.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            UnityEngine.Debug.Log("[Phase7Setup] LeaderboardScreen: 10 строк Top-10, разделитель, playerEntry, errorText добавлены.");
        }

        // --- Шаг 3: Активировать Leaderboard кнопку в TitleScreen ---

        private static void EnableLeaderboardButton()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);

            var titleViews = Resources.FindObjectsOfTypeAll<TitleScreenView>();
            var titleView = System.Array.Find(titleViews, v => v.gameObject.scene == scene);
            if (titleView == null)
            {
                UnityEngine.Debug.LogError("[Phase7Setup] TitleScreenView не найден в сцене!");
                return;
            }

            // Получить кнопку через SerializedObject
            var soTitle = new SerializedObject(titleView);
            var btnProp = soTitle.FindProperty("_leaderboardButton");
            if (btnProp != null && btnProp.objectReferenceValue is Button button)
            {
                button.interactable = true;
                EditorUtility.SetDirty(button);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            UnityEngine.Debug.Log("[Phase7Setup] TitleScreen: _leaderboardButton.interactable = true.");
        }

        // --- Шаг 4: Проверить и установить LeaderboardId в GameData ---

        private static void UpdateGameDataLeaderboardId()
        {
            var gameData = AssetDatabase.LoadAssetAtPath<GameData>(GameDataPath);
            if (gameData == null)
            {
                UnityEngine.Debug.LogError("[Phase7Setup] GameData.asset не найден!");
                return;
            }

            var soGameData = new SerializedObject(gameData);
            var leaderboardIdProp = soGameData.FindProperty("LeaderboardId");
            if (leaderboardIdProp != null && string.IsNullOrEmpty(leaderboardIdProp.stringValue))
            {
                leaderboardIdProp.stringValue = "asteroids_highscores";
                soGameData.ApplyModifiedProperties();
                EditorUtility.SetDirty(gameData);
            }

            UnityEngine.Debug.Log($"[Phase7Setup] GameData.LeaderboardId = '{leaderboardIdProp?.stringValue}'.");
        }
    }
}
#endif
