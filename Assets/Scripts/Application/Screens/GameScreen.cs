namespace SelStrom.Asteroids
{
    public class GameScreen : AbstractScreen
    {
        private HudVisual _hudVisual;
        private int _highScore;

        public void Connect(HudVisual hudVisual, ShipViewModel shipVm)
        {
            _hudVisual = hudVisual;
            if (_hudVisual != null)
            {
                _hudVisual.Connect(new HudData { Ship = shipVm });
            }
        }

        public void UpdateHud(int score, int lives, int highScore)
        {
            _highScore = highScore;
            _hudVisual?.UpdateHud(score, lives, highScore);
        }
    }
}
