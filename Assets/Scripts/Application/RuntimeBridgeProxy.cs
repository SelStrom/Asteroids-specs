namespace SelStrom.Asteroids
{
    /// <summary>
    /// Статический буфер состояния игры для MCP Runtime Bridge.
    /// В Editor-сборке хранит Score/Wave/Lives/IsRunning, обновляется каждый кадр.
    /// В WebGL-сборке компилируется в no-op заглушки — нет зависимости от Editor API.
    /// </summary>
    public static class RuntimeBridgeProxy
    {
#if UNITY_EDITOR
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
#else
        // WebGL no-op заглушки — компилируются без Editor API
        public static void UpdateState(int score, int wave, int lives, bool isRunning) { }
        public static void Reset() { }
#endif
    }
}
