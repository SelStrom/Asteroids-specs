using System;

namespace SelStrom.Asteroids
{
    public class TitleScreen : AbstractScreen
    {
        private TitleScreenView _view;

        public void Connect(TitleScreenView view, Action onStart)
        {
            _view = view;
            var vm = new TitleScreenViewModel();
            vm.OnPlayClicked = onStart;
            _view.Connect(vm);
        }
    }
}
