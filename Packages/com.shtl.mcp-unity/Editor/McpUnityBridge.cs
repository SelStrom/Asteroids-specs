using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shtl.McpUnity.Editor
{
    /// <summary>
    /// HTTP-bridge для взаимодействия TypeScript MCP-сервера с Unity Editor.
    /// Стартует автоматически при загрузке Editor через [InitializeOnLoad].
    /// Слушает запросы на localhost:8765.
    /// Входящие запросы ставятся в ConcurrentQueue и обрабатываются в EditorApplication.update
    /// (главный поток), что надёжнее чем EditorApplication.delayCall из фонового потока.
    /// </summary>
    [InitializeOnLoad]
    public static class McpUnityBridge
    {
        private static HttpListener _listener;
        private static Thread _listenerThread;
        private static volatile bool _running;

        // Очередь входящих запросов — заполняется фоновым потоком, дренируется в главном
        private static readonly ConcurrentQueue<HttpListenerContext> _pendingRequests
            = new ConcurrentQueue<HttpListenerContext>();

        // Данные компиляции — заполняются через CompilationPipeline events
        private static readonly List<CompilerMessage> _compilationMessages = new List<CompilerMessage>();

        static McpUnityBridge()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            EditorApplication.quitting += OnEditorQuitting;
            EditorApplication.update += DrainQueue;
            SubscribeCompilation();
            StartServer();
        }

        private static void SubscribeCompilation()
        {
            CompilationPipeline.compilationStarted += _ =>
            {
                _compilationMessages.Clear();
            };
            CompilationPipeline.assemblyCompilationFinished += (assemblyName, messages) =>
            {
                _compilationMessages.AddRange(messages);
            };
            CompilationPipeline.compilationFinished += _ => { };
        }

        /// <summary>
        /// Вызывается каждый кадр в главном потоке Unity Editor.
        /// Обрабатывает все накопившиеся запросы из фонового потока.
        /// </summary>
        private static void DrainQueue()
        {
            while (_pendingRequests.TryDequeue(out HttpListenerContext ctx))
            {
                HandleRequest(ctx);
            }
        }

        private static void StartServer()
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add("http://localhost:8765/");
                _listener.Start();

                _running = true;
                _listenerThread = new Thread(ListenLoop);
                _listenerThread.IsBackground = true;
                _listenerThread.Name = "McpUnityBridgeThread";
                _listenerThread.Start();

                Debug.Log("[McpUnityBridge] Запущен на localhost:8765");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[McpUnityBridge] Ошибка запуска: {ex.Message}");
            }
        }

        private static void StopServer()
        {
            _running = false;

            // Сбрасываем очередь — все незавершённые запросы отменяем
            while (_pendingRequests.TryDequeue(out HttpListenerContext ctx))
            {
                try { ctx.Response.Abort(); } catch (Exception) { }
            }

            try
            {
                _listener?.Stop();
            }
            catch (Exception)
            {
                // Игнорируем ошибки при остановке
            }
            _listenerThread?.Join(1000);
            Debug.Log("[McpUnityBridge] Остановлен");
        }

        private static void ListenLoop()
        {
            while (_running)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    // Ставим в очередь — DrainQueue обработает в главном потоке
                    _pendingRequests.Enqueue(context);
                }
                catch (HttpListenerException)
                {
                    // Listener остановлен — выходим из цикла
                    break;
                }
                catch (Exception ex)
                {
                    if (_running)
                    {
                        Debug.LogError($"[McpUnityBridge] Ошибка в ListenLoop: {ex.Message}");
                    }
                }
            }
        }

        private static void HandleRequest(HttpListenerContext context)
        {
            try
            {
                string path = context.Request.Url.LocalPath;

                switch (path)
                {
                    case "/compile":
                        HandleCompile(context);
                        break;
                    case "/play":
                        HandlePlay(context);
                        break;
                    case "/stop":
                        HandleStop(context);
                        break;
                    case "/list_scenes":
                        HandleListScenes(context);
                        break;
                    case "/open_scene":
                    {
                        string scenePath = ReadBodyPath(context);
                        HandleOpenScene(context, scenePath);
                        break;
                    }
                    case "/import_asset":
                    {
                        string assetPath = ReadBodyPath(context);
                        HandleImportAsset(context, assetPath);
                        break;
                    }
                    case "/run_menu_item":
                        HandleRunMenuItem(context);
                        break;
                    case "/find_assets":
                        HandleFindAssets(context);
                        break;
                    case "/set_asset_field":
                        HandleSetAssetField(context);
                        break;
                    case "/set_scene_object_field":
                        HandleSetSceneObjectField(context);
                        break;
                    case "/get_game_state":
                        HandleGetGameState(context);
                        break;
                    default:
                        context.Response.StatusCode = 404;
                        SendJsonRaw(context, "{\"success\":false,\"message\":\"Unknown endpoint\"}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[McpUnityBridge] Ошибка HandleRequest: {ex.Message}");
                try
                {
                    SendJsonRaw(context, $"{{\"success\":false,\"message\":\"{EscapeJson(ex.Message)}\"}}");
                }
                catch (Exception)
                {
                    // Ответ уже отправлен или connection закрыт
                }
            }
        }

        private static string ReadBodyPath(HttpListenerContext context)
        {
            string body = "";
            using (StreamReader reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
            {
                body = reader.ReadToEnd();
            }

            // Простой парсинг { "path": "..." } без сторонних зависимостей
            string key = "\"path\"";
            int keyIndex = body.IndexOf(key, StringComparison.Ordinal);
            if (keyIndex < 0)
            {
                return "";
            }

            int colonIndex = body.IndexOf(':', keyIndex + key.Length);
            if (colonIndex < 0)
            {
                return "";
            }

            int startQuote = body.IndexOf('"', colonIndex + 1);
            if (startQuote < 0)
            {
                return "";
            }

            int endQuote = body.IndexOf('"', startQuote + 1);
            if (endQuote < 0)
            {
                return "";
            }

            return body.Substring(startQuote + 1, endQuote - startQuote - 1);
        }

        private static void HandleCompile(HttpListenerContext context)
        {
            AssetDatabase.Refresh();

            List<CompilerMessage> errors = _compilationMessages.FindAll(m => m.type == CompilerMessageType.Error);
            List<CompilerMessage> warnings = _compilationMessages.FindAll(m => m.type == CompilerMessageType.Warning);

            if (errors.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("{\"success\":false,\"errorCount\":");
                sb.Append(errors.Count);
                sb.Append(",\"errors\":[");

                for (int i = 0; i < errors.Count; i++)
                {
                    CompilerMessage e = errors[i];
                    if (i > 0)
                    {
                        sb.Append(",");
                    }
                    sb.Append("{\"file\":\"");
                    sb.Append(EscapeJson(e.file));
                    sb.Append("\",\"line\":");
                    sb.Append(e.line);
                    sb.Append(",\"column\":");
                    sb.Append(e.column);
                    sb.Append(",\"message\":\"");
                    sb.Append(EscapeJson(e.message));
                    sb.Append("\",\"severity\":\"error\"}");
                }

                sb.Append("]}");
                SendJsonRaw(context, sb.ToString());
            }
            else
            {
                SendJsonRaw(context, $"{{\"success\":true,\"message\":\"Compilation successful. {warnings.Count} warnings.\"}}");
            }
        }

        private static void HandlePlay(HttpListenerContext context)
        {
            EditorApplication.isPlaying = true;
            SendJsonRaw(context, "{\"success\":true,\"message\":\"Play Mode started\"}");
        }

        private static void HandleStop(HttpListenerContext context)
        {
            EditorApplication.isPlaying = false;
            SendJsonRaw(context, "{\"success\":true,\"message\":\"Play Mode stopped\"}");
        }

        private static void HandleListScenes(HttpListenerContext context)
        {
            string[] guids = AssetDatabase.FindAssets("t:Scene");
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"success\":true,\"scenes\":[");

            for (int i = 0; i < guids.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(",");
                }
                string scenePath = AssetDatabase.GUIDToAssetPath(guids[i]);
                sb.Append("\"");
                sb.Append(EscapeJson(scenePath));
                sb.Append("\"");
            }

            sb.Append("]}");
            SendJsonRaw(context, sb.ToString());
        }

        private static void HandleOpenScene(HttpListenerContext context, string path)
        {
            try
            {
                EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                SendJsonRaw(context, $"{{\"success\":true,\"message\":\"Opened scene: {EscapeJson(path)}\"}}");
            }
            catch (Exception ex)
            {
                SendJsonRaw(context, $"{{\"success\":false,\"message\":\"{EscapeJson(ex.Message)}\"}}");
            }
        }

        private static void HandleImportAsset(HttpListenerContext context, string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            SendJsonRaw(context, $"{{\"success\":true,\"message\":\"Imported asset: {EscapeJson(path)}\"}}");
        }

        /// <summary>
        /// Читает тело запроса и извлекает все строковые поля из плоского JSON-объекта.
        /// </summary>
        private static Dictionary<string, string> ReadBodyFields(HttpListenerContext context)
        {
            string body;
            using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
            {
                body = reader.ReadToEnd();
            }

            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            int pos = 0;
            int len = body.Length;

            while (pos < len)
            {
                int kOpen = body.IndexOf('"', pos);
                if (kOpen < 0) { break; }
                int kClose = body.IndexOf('"', kOpen + 1);
                if (kClose < 0) { break; }
                string key = body.Substring(kOpen + 1, kClose - kOpen - 1);
                pos = kClose + 1;

                int colon = body.IndexOf(':', pos);
                if (colon < 0) { break; }
                pos = colon + 1;

                while (pos < len && (body[pos] == ' ' || body[pos] == '\t' || body[pos] == '\r' || body[pos] == '\n'))
                {
                    pos++;
                }
                if (pos >= len) { break; }

                if (body[pos] == '"')
                {
                    int vClose = body.IndexOf('"', pos + 1);
                    if (vClose < 0) { break; }
                    result[key] = body.Substring(pos + 1, vClose - pos - 1);
                    pos = vClose + 1;
                }
                else
                {
                    // Не строковое значение — пропускаем до следующего разделителя
                    int nextSep = len;
                    int c1 = body.IndexOf(',', pos);
                    int c2 = body.IndexOf('}', pos);
                    if (c1 >= 0 && c1 < nextSep) { nextSep = c1; }
                    if (c2 >= 0 && c2 < nextSep) { nextSep = c2; }
                    pos = nextSep + 1;
                }
            }

            return result;
        }

        private static void HandleRunMenuItem(HttpListenerContext context)
        {
            var fields = ReadBodyFields(context);
            if (!fields.TryGetValue("menu_path", out string menuPath) || string.IsNullOrEmpty(menuPath))
            {
                SendJsonRaw(context, "{\"success\":false,\"message\":\"menu_path required\"}");
                return;
            }

            bool executed = EditorApplication.ExecuteMenuItem(menuPath);
            if (executed)
            {
                SendJsonRaw(context, $"{{\"success\":true,\"message\":\"Executed: {EscapeJson(menuPath)}\"}}");
            }
            else
            {
                SendJsonRaw(context, $"{{\"success\":false,\"message\":\"MenuItem not found: {EscapeJson(menuPath)}\"}}");
            }
        }

        private static void HandleFindAssets(HttpListenerContext context)
        {
            var fields = ReadBodyFields(context);
            fields.TryGetValue("type", out string type);
            fields.TryGetValue("name", out string name);

            var filterParts = new List<string>();
            if (!string.IsNullOrEmpty(type)) { filterParts.Add($"t:{type}"); }
            if (!string.IsNullOrEmpty(name)) { filterParts.Add(name); }
            string filter = string.Join(" ", filterParts);

            string[] guids = AssetDatabase.FindAssets(filter);

            StringBuilder sb = new StringBuilder();
            sb.Append("{\"success\":true,\"assets\":[");
            for (int i = 0; i < guids.Length; i++)
            {
                if (i > 0) { sb.Append(","); }
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                sb.Append("\"");
                sb.Append(EscapeJson(path));
                sb.Append("\"");
            }
            sb.Append("]}");
            SendJsonRaw(context, sb.ToString());
        }

        private static void HandleSetAssetField(HttpListenerContext context)
        {
            var fields = ReadBodyFields(context);

            if (!fields.TryGetValue("asset_path", out string assetPath) || string.IsNullOrEmpty(assetPath))
            {
                SendJsonRaw(context, "{\"success\":false,\"message\":\"asset_path required\"}");
                return;
            }
            if (!fields.TryGetValue("field_path", out string fieldPath) || string.IsNullOrEmpty(fieldPath))
            {
                SendJsonRaw(context, "{\"success\":false,\"message\":\"field_path required\"}");
                return;
            }
            fields.TryGetValue("value_asset_path", out string valueAssetPath);
            fields.TryGetValue("value_asset_name", out string valueAssetName);

            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset == null)
            {
                SendJsonRaw(context, $"{{\"success\":false,\"message\":\"Asset not found: {EscapeJson(assetPath)}\"}}");
                return;
            }

            SerializedObject so = new SerializedObject(asset);
            SerializedProperty prop = so.FindProperty(fieldPath);
            if (prop == null)
            {
                SendJsonRaw(context, $"{{\"success\":false,\"message\":\"Property not found: {EscapeJson(fieldPath)}\"}}");
                return;
            }

            if (string.IsNullOrEmpty(valueAssetPath))
            {
                prop.objectReferenceValue = null;
            }
            else
            {
                UnityEngine.Object valueObj = null;
                if (!string.IsNullOrEmpty(valueAssetName))
                {
                    // Загружаем под-ассет по имени (например, спрайт из атласа)
                    UnityEngine.Object[] all = AssetDatabase.LoadAllAssetsAtPath(valueAssetPath);
                    foreach (var a in all)
                    {
                        if (a != null && a.name == valueAssetName)
                        {
                            valueObj = a;
                            break;
                        }
                    }
                    if (valueObj == null)
                    {
                        SendJsonRaw(context, $"{{\"success\":false,\"message\":\"Sub-asset '{EscapeJson(valueAssetName)}' not found in {EscapeJson(valueAssetPath)}\"}}");
                        return;
                    }
                }
                else
                {
                    valueObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(valueAssetPath);
                    if (valueObj == null)
                    {
                        SendJsonRaw(context, $"{{\"success\":false,\"message\":\"Value asset not found: {EscapeJson(valueAssetPath)}\"}}");
                        return;
                    }
                }
                prop.objectReferenceValue = valueObj;
            }

            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();

            SendJsonRaw(context, $"{{\"success\":true,\"message\":\"Set {EscapeJson(fieldPath)} on {EscapeJson(assetPath)}\"}}");
        }

        private static void HandleSetSceneObjectField(HttpListenerContext context)
        {
            var fields = ReadBodyFields(context);

            if (!fields.TryGetValue("object_name", out string objectName) || string.IsNullOrEmpty(objectName))
            {
                SendJsonRaw(context, "{\"success\":false,\"message\":\"object_name required\"}");
                return;
            }
            if (!fields.TryGetValue("component_type", out string componentTypeName) || string.IsNullOrEmpty(componentTypeName))
            {
                SendJsonRaw(context, "{\"success\":false,\"message\":\"component_type required\"}");
                return;
            }
            if (!fields.TryGetValue("field_path", out string fieldPath) || string.IsNullOrEmpty(fieldPath))
            {
                SendJsonRaw(context, "{\"success\":false,\"message\":\"field_path required\"}");
                return;
            }
            fields.TryGetValue("value_asset_path", out string valueAssetPath);
            fields.TryGetValue("value_asset_name", out string valueAssetName);
            fields.TryGetValue("value_object_name", out string valueObjectName);
            fields.TryGetValue("value_component_type", out string valueComponentType);

            GameObject go = GameObject.Find(objectName);
            if (go == null)
            {
                SendJsonRaw(context, $"{{\"success\":false,\"message\":\"GameObject not found: {EscapeJson(objectName)}\"}}");
                return;
            }

            Component component = null;
            foreach (Component c in go.GetComponents<Component>())
            {
                if (c != null && c.GetType().Name == componentTypeName)
                {
                    component = c;
                    break;
                }
            }
            if (component == null)
            {
                SendJsonRaw(context, $"{{\"success\":false,\"message\":\"Component '{EscapeJson(componentTypeName)}' not found on '{EscapeJson(objectName)}'\"}}");
                return;
            }

            SerializedObject so = new SerializedObject(component);
            SerializedProperty prop = so.FindProperty(fieldPath);
            if (prop == null)
            {
                SendJsonRaw(context, $"{{\"success\":false,\"message\":\"Property not found: {EscapeJson(fieldPath)}\"}}");
                return;
            }

            if (!string.IsNullOrEmpty(valueObjectName))
            {
                // Ссылка на объект сцены по имени
                GameObject valueGo = GameObject.Find(valueObjectName);
                if (valueGo == null)
                {
                    SendJsonRaw(context, $"{{\"success\":false,\"message\":\"Value GameObject not found: {EscapeJson(valueObjectName)}\"}}");
                    return;
                }

                if (!string.IsNullOrEmpty(valueComponentType))
                {
                    Component found = null;
                    foreach (Component c in valueGo.GetComponents<Component>())
                    {
                        if (c != null && c.GetType().Name == valueComponentType)
                        {
                            found = c;
                            break;
                        }
                    }
                    if (found == null)
                    {
                        SendJsonRaw(context, $"{{\"success\":false,\"message\":\"Component '{EscapeJson(valueComponentType)}' not found on '{EscapeJson(valueObjectName)}'\"}}");
                        return;
                    }
                    prop.objectReferenceValue = found;
                }
                else
                {
                    prop.objectReferenceValue = valueGo;
                }
            }
            else if (string.IsNullOrEmpty(valueAssetPath))
            {
                prop.objectReferenceValue = null;
            }
            else
            {
                UnityEngine.Object valueObj = null;
                if (!string.IsNullOrEmpty(valueAssetName))
                {
                    UnityEngine.Object[] all = AssetDatabase.LoadAllAssetsAtPath(valueAssetPath);
                    foreach (var a in all)
                    {
                        if (a != null && a.name == valueAssetName)
                        {
                            valueObj = a;
                            break;
                        }
                    }
                    if (valueObj == null)
                    {
                        SendJsonRaw(context, $"{{\"success\":false,\"message\":\"Sub-asset '{EscapeJson(valueAssetName)}' not found in {EscapeJson(valueAssetPath)}\"}}");
                        return;
                    }
                }
                else
                {
                    valueObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(valueAssetPath);
                    if (valueObj == null)
                    {
                        SendJsonRaw(context, $"{{\"success\":false,\"message\":\"Value asset not found: {EscapeJson(valueAssetPath)}\"}}");
                        return;
                    }
                }
                prop.objectReferenceValue = valueObj;
            }

            so.ApplyModifiedProperties();

            Scene activeScene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);

            SendJsonRaw(context, $"{{\"success\":true,\"message\":\"Set {EscapeJson(fieldPath)} on {EscapeJson(componentTypeName)} ({EscapeJson(objectName)})\"}}");
        }

        private static void HandleGetGameState(HttpListenerContext context)
        {
            var type = System.Type.GetType("SelStrom.Asteroids.RuntimeBridgeProxy, Assembly-CSharp");

            if (type == null || !EditorApplication.isPlaying)
            {
                SendJsonRaw(context, "{\"success\":true,\"score\":0,\"wave\":0,\"lives\":0,\"isPlaying\":false}");
                return;
            }

            var score = (int)type.GetField("Score").GetValue(null);
            var wave = (int)type.GetField("Wave").GetValue(null);
            var lives = (int)type.GetField("Lives").GetValue(null);
            var isRunning = (bool)type.GetField("IsRunning").GetValue(null);
            var isPlaying = EditorApplication.isPlaying && isRunning;

            var sb = new System.Text.StringBuilder();
            sb.Append("{\"success\":true,\"score\":");
            sb.Append(score);
            sb.Append(",\"wave\":");
            sb.Append(wave);
            sb.Append(",\"lives\":");
            sb.Append(lives);
            sb.Append(",\"isPlaying\":");
            sb.Append(isPlaying ? "true" : "false");
            sb.Append("}");

            SendJsonRaw(context, sb.ToString());
        }

        private static void SendJsonRaw(HttpListenerContext context, string json)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                context.Response.ContentType = "application/json; charset=utf-8";
                context.Response.ContentLength64 = bytes.Length;
                context.Response.OutputStream.Write(bytes, 0, bytes.Length);
                context.Response.OutputStream.Close();
            }
            catch (System.Net.Sockets.SocketException)
            {
                // Клиент отключился до получения ответа
            }
            catch (System.ObjectDisposedException)
            {
                // Соединение уже закрыто
            }
            catch (System.IO.IOException)
            {
                // Сетевая ошибка при записи ответа (например, socket shutdown)
            }
        }

        /// <summary>
        /// Экранирует строку для JSON: заменяет \, ", перевод строки и т.д.
        /// </summary>
        private static string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        private static void OnBeforeAssemblyReload()
        {
            StopServer();
        }

        private static void OnEditorQuitting()
        {
            StopServer();
        }
    }
}
