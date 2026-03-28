using System;
using Shtl.Mvvm;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SelStrom.Asteroids
{
    public class LeaderboardViewModel : AbstractViewModel
    {
        public Action OnBackClicked;
        // Данные Top-10 — заглушки до Phase 7
        public string[] EntryNames = new string[0];
        public int[] EntryScores = new int[0];
    }

    public class LeaderboardView : AbstractWidgetView<LeaderboardViewModel>
    {
        [SerializeField] private TextMeshProUGUI _titleText;       // "LEADERBOARD"
        [SerializeField] private TextMeshProUGUI _placeholderText; // "Загрузка..." stub
        [SerializeField] private Button _backButton;

        protected override void OnConnected()
        {
            if (_titleText != null) { _titleText.text = "LEADERBOARD"; }
            if (_placeholderText != null) { _placeholderText.text = "— Доступно в Phase 7 —"; }
            if (_backButton != null)
            {
                _backButton.onClick.AddListener(() => ViewModel?.OnBackClicked?.Invoke());
            }
        }

        protected override void OnDisposed()
        {
            if (_backButton != null) { _backButton.onClick.RemoveAllListeners(); }
        }
    }
}
