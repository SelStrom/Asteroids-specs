using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Shtl.McpUnity.Editor
{
    /// <summary>
    /// HTTP-bridge для взаимодействия TypeScript MCP-сервера с Unity Editor.
    /// Стартует автоматически при загрузке Editor через [InitializeOnLoad].
    /// Слушает запросы на localhost:8765.
    /// </summary>
    [InitializeOnLoad]
    public static class McpUnityBridge
    {
        private static HttpListener _listener;
        private static Thread _listenerThread;
        private static volatile bool _running;

        // Данные компиляции — заполняются через CompilationPipeline events
        private static readonly List<CompilerMessage> _compilationMessages = new List<CompilerMessage>();

        static McpUnityBridge()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            EditorApplication.quitting += OnEditorQuitting;
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
                    // Dispatching в главный поток Unity
                    EditorApplication.delayCall += () =>
                    {
                        if (_running)
                        {
                            HandleRequest(context);
                        }
                        else
                        {
                            context.Response.Abort();
                        }
                    };
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
