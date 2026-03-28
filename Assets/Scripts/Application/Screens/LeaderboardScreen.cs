using System;

namespace SelStrom.Asteroids
{
    public class LeaderboardScreen : AbstractScreen
    {
        private LeaderboardView _view;
        private LeaderboardViewModel _viewModel;

        public void Connect(LeaderboardView view, Action onBack)
        {
            _view = view;
            _viewModel = new LeaderboardViewModel();
            _viewModel.OnBackClicked = onBack;
        }

        public void Show()
        {
            if (_view == null) { return; }
            _view.gameObject.SetActive(true);
            _view.Connect(_viewModel);
        }

        public void Hide()
        {
            if (_view == null) { return; }
            _view.Disconnect();
            _view.gameObject.SetActive(false);
        }

        public override void Dispose()
        {
            base.Dispose();
            _view = null;
            _viewModel = null;
        }
    }
}
