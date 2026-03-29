namespace SelStrom.Asteroids
{
    /// <summary>
    /// Статический буфер состояния игры для MCP Runtime Bridge.
    /// Поля присутствуют в Assembly-CSharp (рантайм) чтобы McpUnityBridge мог читать их
    /// через рефлексию во время Play Mode. В WebGL-билде поля занимают минимум памяти
    /// и никогда не обновляются (ApplicationEntry вызывает UpdateState только в Play Mode).
    /// </summary>
    public static class RuntimeBridgeProxy
    {
        public static int Score;
        public static int Wave;
        public static int Lives;
        public static bool IsRunning;

        public static void UpdateState(int score, int wave, int lives, bool isRunning)
        {
            Score = score;
            Wave = wave;
            Lives = lives;
            IsRunning = isRunning;
        }

        public static void Reset()
        {
            Score = 0;
            Wave = 0;
            Lives = 0;
            IsRunning = false;
        }
    }
}
