using System;

namespace SelStrom.Asteroids
{
    public class GameOverScreen : AbstractScreen
    {
        private GameOverView _view;

        public void Connect(GameOverView view, Action onPlayAgain)
        {
            _view = view;
            var vm = new GameOverViewModel();
            vm.OnPlayAgainClicked = onPlayAgain;
            _view.Connect(vm);
        }

        // LEAD-02: расширенный Show с именем, onSubmitScore и onLeaderboard
        public void Show(int finalScore, int highScore, string playerName,
            Action<string> onSubmitScore, Action onLeaderboard)
        {
            if (_view == null) { return; }
            var vm = new GameOverViewModel
            {
                OnPlayAgainClicked = _view.ViewModel?.OnPlayAgainClicked,
                FinalScore = finalScore,
                HighScore = highScore,
                // D-02: предзаполнение имени из PlayerPrefs
                InitialPlayerName = playerName,
                // LEAD-02: callback для отправки счёта
                OnSubmitScore = onSubmitScore,
                // навигация к лидерборду
                OnLeaderboardClicked = onLeaderboard
            };
            _view.Connect(vm);
            _view.gameObject.SetActive(true);
        }

        // D-03: вызывается из Application после успешного submit
        public void NotifySubmitSuccess()
        {
            _view?.OnSubmitSuccess();
        }

        // LEAD-05: вызывается из Application при ошибке submit
        public void NotifySubmitError(string message)
        {
            _view?.OnSubmitError(message);
        }

        public void Hide()
        {
            if (_view != null) { _view.gameObject.SetActive(false); }
        }

        public override void Dispose()
        {
            base.Dispose();
            _view = null;
        }
    }
}
