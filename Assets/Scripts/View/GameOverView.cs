using System;
using Shtl.Mvvm;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SelStrom.Asteroids
{
    public class GameOverViewModel : AbstractViewModel
    {
        public Action OnPlayAgainClicked;   // PROG-08: Play Again
        public int FinalScore;              // финальный счёт для отображения
        public int HighScore;              // High Score сессии (PROG-07)
    }

    public class GameOverView : AbstractWidgetView<GameOverViewModel>
    {
        [SerializeField] private TextMeshProUGUI _scoreText;      // "Score: N"
        [SerializeField] private TextMeshProUGUI _highScoreText;  // "Best: N"
        [SerializeField] private Button _playAgainButton;         // активна (PROG-08)
        [SerializeField] private Button _submitScoreButton;       // disabled (D-11)
        [SerializeField] private Button _leaderboardButton;       // disabled (D-11)

        protected override void OnConnected()
        {
            // Отобразить счёт
            if (_scoreText != null) { _scoreText.text = $"Score: {ViewModel.FinalScore}"; }
            if (_highScoreText != null) { _highScoreText.text = $"Best: {ViewModel.HighScore}"; }

            // Play Again — активна (D-11)
            if (_playAgainButton != null)
            {
                _playAgainButton.interactable = true;
                _playAgainButton.onClick.AddListener(() => ViewModel.OnPlayAgainClicked?.Invoke());
            }

            // Submit Score — disabled до Phase 7 (D-11)
            if (_submitScoreButton != null) { _submitScoreButton.interactable = false; }

            // Leaderboard — disabled до Phase 7 (D-11)
            if (_leaderboardButton != null) { _leaderboardButton.interactable = false; }
        }

        protected override void OnDisposed()
        {
            if (_playAgainButton != null) { _playAgainButton.onClick.RemoveAllListeners(); }
        }
    }
}
