using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        [SerializeField] private TextMeshProUGUI _scoreText;        // Score верх-лево (PROG-02)
        [SerializeField] private TextMeshProUGUI _highScoreText;    // HighScore верх-право (PROG-07)
        [SerializeField] private Transform _livesContainer;         // контейнер для иконок жизней (D-08)
        [SerializeField] private Sprite _lifeIconSprite;            // Ship спрайт для иконок (D-08)
        [SerializeField] private TextMeshProUGUI _waveBannerText;   // Wave banner по центру экрана

        private ShipViewModel _ship;
        private readonly List<GameObject> _lifeIcons = new();

        public void Connect(HudData data)
        {
            _ship = data.Ship;
        }

        public void UpdateHud(int score, int lives, int highScore)
        {
            if (_scoreText != null) { _scoreText.text = score.ToString(); }
            if (_highScoreText != null) { _highScoreText.text = highScore.ToString(); }
            UpdateLivesIcons(lives);
        }

        private void UpdateLivesIcons(int lives)
        {
            if (_livesContainer == null || _lifeIconSprite == null) { return; }

            // Удалить лишние иконки
            while (_lifeIcons.Count > lives)
            {
                var last = _lifeIcons[_lifeIcons.Count - 1];
                _lifeIcons.RemoveAt(_lifeIcons.Count - 1);
                Destroy(last);
            }

            // Добавить недостающие иконки
            while (_lifeIcons.Count < lives)
            {
                var iconGo = new GameObject("LifeIcon");
                iconGo.transform.SetParent(_livesContainer, false);
                var img = iconGo.AddComponent<Image>();
                img.sprite = _lifeIconSprite;
                // Размер 20×20px (D-08)
                var rt = iconGo.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(20f, 20f);
                _lifeIcons.Add(iconGo);
            }
        }

        public void ShowWaveBanner(int wave)
        {
            if (_waveBannerText == null) { return; }
            _waveBannerText.text = $"WAVE {wave}";
            _waveBannerText.gameObject.SetActive(true);
        }

        public void HideWaveBanner()
        {
            if (_waveBannerText == null) { return; }
            _waveBannerText.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_ship == null) { return; }
            var pos = _ship.Position.Value;
            if (_coordinates != null) { _coordinates.text = $"X: {pos.x:F1}  Y: {pos.y:F1}"; }
            if (_rotationAngle != null) { _rotationAngle.text = $"{_ship.Rotation.Value:F1}°"; }
            if (_speedText != null) { _speedText.text = $"{_ship.Speed.Value:F1}"; }
            if (_laserShootCount != null) { _laserShootCount.text = $"{_ship.LaserCount.Value}"; }
            if (_laserReloadTime != null) { _laserReloadTime.text = $"{_ship.LaserReloadTime.Value:F2}s"; }
        }
    }
}
