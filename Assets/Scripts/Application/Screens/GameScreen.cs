namespace SelStrom.Asteroids
{
    public class GameScreen : AbstractScreen
    {
        private HudVisual _hudVisual;

        public void Connect(HudVisual hudVisual, ShipViewModel shipVm)
        {
            _hudVisual = hudVisual;
            if (_hudVisual != null)
            {
                _hudVisual.Connect(new HudData { Ship = shipVm });
            }
        }
    }
}
