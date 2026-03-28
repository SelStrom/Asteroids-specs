using System;
using Shtl.Mvvm;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SelStrom.Asteroids
{
    public class TitleScreenViewModel : AbstractViewModel
    {
        public Action OnPlayClicked;
        public Action OnLeaderboardClicked; // VIS-06: новая кнопка Leaderboard
    }

    public class TitleScreenView : AbstractWidgetView<TitleScreenViewModel>
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _leaderboardButton;    // disabled до Phase 7 (D-11)
        [SerializeField] private TextMeshProUGUI _titleText;   // "ASTEROIDS"

        protected override void OnConnected()
        {
            if (_titleText != null) { _titleText.text = "ASTEROIDS"; }
            if (_playButton != null)
            {
                _playButton.onClick.AddListener(() => ViewModel?.OnPlayClicked?.Invoke());
            }
            if (_leaderboardButton != null)
            {
                _leaderboardButton.interactable = false; // disabled до Phase 7
                _leaderboardButton.onClick.AddListener(() => ViewModel?.OnLeaderboardClicked?.Invoke());
            }
        }

        protected override void OnDisposed()
        {
            if (_playButton != null) { _playButton.onClick.RemoveAllListeners(); }
            if (_leaderboardButton != null) { _leaderboardButton.onClick.RemoveAllListeners(); }
        }
    }
}
