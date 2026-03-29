using System;
using Shtl.Mvvm;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SelStrom.Asteroids
{
    public class GameOverViewModel : AbstractViewModel
    {
        public Action OnPlayAgainClicked;    // PROG-08: Play Again
        public Action<string> OnSubmitScore; // LEAD-02: (playerName) → отправить счёт
        public Action OnLeaderboardClicked;  // навигация к LeaderboardScreen
        public int FinalScore;               // финальный счёт
        public int HighScore;                // High Score сессии (PROG-07)
        public string InitialPlayerName;     // предзаполнение из PlayerPrefs (D-02)
    }

    public class GameOverView : AbstractWidgetView<GameOverViewModel>
    {
        [SerializeField] private TextMeshProUGUI _scoreText;         // "Score: N"
        [SerializeField] private TextMeshProUGUI _highScoreText;     // "Best: N"
        [SerializeField] private Button _playAgainButton;            // активна (PROG-08)
        [SerializeField] private Button _submitScoreButton;          // активна в Phase 7
        [SerializeField] private Button _leaderboardButton;          // активна в Phase 7
        // D-01: InputField для имени — новый элемент, Phase7Setup добавит в Canvas
        [SerializeField] private TMP_InputField _playerNameInput;
        // LEAD-05: errorText — скрыт по умолчанию, цвет #FF4444
        [SerializeField] private TextMeshProUGUI _errorText;

        protected override void OnConnected()
        {
            // Отобразить счёт
            if (_scoreText != null) { _scoreText.text = $"Score: {ViewModel.FinalScore}"; }
            if (_highScoreText != null) { _highScoreText.text = $"Best: {ViewModel.HighScore}"; }

            // D-01, D-02: InputField с именем игрока, предзаполнение из PlayerPrefs
            if (_playerNameInput != null)
            {
                _playerNameInput.characterLimit = 16; // LEAD-02: max 16 символов
                _playerNameInput.text = ViewModel.InitialPlayerName ?? "";
                // D-02: сохранять имя при изменении
                _playerNameInput.onEndEdit.AddListener(name => PlayerPrefs.SetString("PlayerName", name));
            }

            // Play Again
            if (_playAgainButton != null)
            {
                _playAgainButton.interactable = true;
                _playAgainButton.onClick.AddListener(() => ViewModel?.OnPlayAgainClicked?.Invoke());
            }

            // Submit Score — активна в Phase 7 (D-03: станет неактивной после успешного submit)
            if (_submitScoreButton != null)
            {
                _submitScoreButton.interactable = true;
                _submitScoreButton.onClick.AddListener(OnSubmitScoreClicked);
            }

            // Leaderboard кнопка — активна в Phase 7
            if (_leaderboardButton != null)
            {
                _leaderboardButton.interactable = true;
                _leaderboardButton.onClick.AddListener(() => ViewModel?.OnLeaderboardClicked?.Invoke());
            }

            // Скрыть errorText по умолчанию
            if (_errorText != null) { _errorText.gameObject.SetActive(false); }
        }

        private void OnSubmitScoreClicked()
        {
            if (ViewModel?.OnSubmitScore == null) { return; }
            // D-02: сохранить имя в PlayerPrefs
            var name = _playerNameInput != null ? _playerNameInput.text : "";
            if (!string.IsNullOrEmpty(name))
            {
                PlayerPrefs.SetString("PlayerName", name);
            }
            // Передать имя в Application для отправки через UgsService
            ViewModel.OnSubmitScore.Invoke(name);
        }

        // D-03: вызывается из Application после успешного submit
        public void OnSubmitSuccess()
        {
            if (_submitScoreButton != null) { _submitScoreButton.interactable = false; }
            if (_playerNameInput != null) { _playerNameInput.interactable = false; }
            if (_errorText != null) { _errorText.gameObject.SetActive(false); }
        }

        // LEAD-05: вызывается из Application при ошибке submit
        public void OnSubmitError(string message)
        {
            if (_submitScoreButton != null) { _submitScoreButton.interactable = true; }
            if (_errorText != null)
            {
                _errorText.text = message;
                _errorText.gameObject.SetActive(true);
            }
        }

        protected override void OnDisposed()
        {
            if (_playAgainButton != null) { _playAgainButton.onClick.RemoveAllListeners(); }
            if (_submitScoreButton != null) { _submitScoreButton.onClick.RemoveAllListeners(); }
            if (_leaderboardButton != null) { _leaderboardButton.onClick.RemoveAllListeners(); }
            if (_playerNameInput != null) { _playerNameInput.onEndEdit.RemoveAllListeners(); }
        }
    }
}
