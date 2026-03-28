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

        // Показать экран с актуальным счётом (вызывается при Game Over)
        public void Show(int finalScore, int highScore)
        {
            if (_view == null) { return; }
            var vm = new GameOverViewModel
            {
                OnPlayAgainClicked = _view.ViewModel?.OnPlayAgainClicked,
                FinalScore = finalScore,
                HighScore = highScore
            };
            _view.Connect(vm);
            _view.gameObject.SetActive(true);
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
