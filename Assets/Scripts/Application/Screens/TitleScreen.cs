using System;

namespace SelStrom.Asteroids
{
    public class TitleScreen : AbstractScreen
    {
        private TitleScreenView _view;

        // Расширить Connect: добавить onLeaderboard callback для Phase 7
        public void Connect(TitleScreenView view, Action onStart, Action onLeaderboard = null)
        {
            _view = view;
            var vm = new TitleScreenViewModel();
            vm.OnPlayClicked = onStart;
            vm.OnLeaderboardClicked = onLeaderboard;
            _view.Connect(vm);
        }
    }
}
