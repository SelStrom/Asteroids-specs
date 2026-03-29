using System;
using System.Collections;
using UnityEngine;

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

        // LEAD-03, LEAD-04: загрузить данные из UGS и отобразить
        // Вызывается из Application.cs через StartCoroutine (D-09)
        public IEnumerator ShowWithDataCoroutine(UgsService ugsService)
        {
            if (_view == null) { yield break; }
            _view.gameObject.SetActive(true);
            _view.Connect(_viewModel);

            // Запустить оба запроса параллельно
            var topTask = ugsService.GetTopScoresAsync();
            var playerTask = ugsService.GetPlayerScoreAsync();

            // Ждать завершения обоих Task (Pitfall 5: не использовать Task.Run в WebGL)
            while (!topTask.IsCompleted || !playerTask.IsCompleted)
            {
                yield return null;
            }

            var vm = new LeaderboardViewModel();
            vm.OnBackClicked = _viewModel.OnBackClicked;

            if (topTask.IsFaulted)
            {
                // LEAD-05: ошибка загрузки
                vm.ErrorMessage = "Ошибка загрузки. Проверьте соединение.";
                Debug.LogWarning("[LeaderboardScreen] Ошибка загрузки Top-10: " + topTask.Exception?.Message);
            }
            else
            {
                var page = topTask.Result;
                if (page?.Results != null)
                {
                    vm.Top10Entries = page.Results.ToArray();
                }
            }

            // PlayerEntry: ошибка обрабатывается внутри GetPlayerScoreAsync (возвращает null)
            vm.PlayerEntry = playerTask.IsFaulted ? null : playerTask.Result;

            _viewModel = vm;
            _view.Bind(vm);
        }

        public void Hide()
        {
            if (_view == null) { return; }
            _view.Dispose();
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
