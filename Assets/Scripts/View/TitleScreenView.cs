using System;
using Shtl.Mvvm;
using UnityEngine;
using UnityEngine.UI;

namespace SelStrom.Asteroids
{
    public class TitleScreenViewModel : AbstractViewModel
    {
        public Action OnPlayClicked;
    }

    public class TitleScreenView : AbstractWidgetView<TitleScreenViewModel>
    {
        [SerializeField] private Button _playButton;

        protected override void OnConnected()
        {
            _playButton.onClick.AddListener(() => ViewModel?.OnPlayClicked?.Invoke());
        }

        protected override void OnDisposed()
        {
            _playButton.onClick.RemoveAllListeners();
        }
    }
}
