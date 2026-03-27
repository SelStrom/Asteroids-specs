using TMPro;
using UnityEngine;

namespace SelStrom.Asteroids
{
    public struct HudData
    {
        public ShipViewModel Ship;
    }

    public class HudVisual : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _coordinates;
        [SerializeField] private TextMeshProUGUI _rotationAngle;
        [SerializeField] private TextMeshProUGUI _speedText;
        [SerializeField] private TextMeshProUGUI _laserShootCount;
        [SerializeField] private TextMeshProUGUI _laserReloadTime;

        private ShipViewModel _ship;

        public void Connect(HudData data)
        {
            _ship = data.Ship;
        }

        private void Update()
        {
            if (_ship == null) { return; }
            var pos = _ship.Position.Value;
            if (_coordinates != null) { _coordinates.text = $"X: {pos.x:F1}  Y: {pos.y:F1}"; }
        }
    }
}
