using System;
using Shtl.Mvvm;
using TMPro;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;

namespace SelStrom.Asteroids
{
    // D-06: расширенный ViewModel с реальными данными из UGS
    public class LeaderboardViewModel : AbstractViewModel
    {
        public Action OnBackClicked;
        // LEAD-03: Top-10 записей лидерборда
        public LeaderboardEntry[] Top10Entries = new LeaderboardEntry[0];
        // LEAD-04: позиция текущего игрока (null если не подавал счёт)
        public LeaderboardEntry PlayerEntry;
        // LEAD-05: сообщение об ошибке (null если нет ошибки)
        public string ErrorMessage;
    }

    public class LeaderboardView : AbstractWidgetView<LeaderboardViewModel>
    {
        [SerializeField] private TextMeshProUGUI _titleText;        // "LEADERBOARD" (существующий)
        [SerializeField] private TextMeshProUGUI _placeholderText;  // "Загрузка..." (существующий)
        [SerializeField] private Button _backButton;                // (существующий)

        // D-04: 10 статичных строк Top-10 (новые — Phase7Setup добавит в сцену)
        [SerializeField] private TextMeshProUGUI[] _entryTexts = new TextMeshProUGUI[10];
        // D-05: строка текущего игрока под разделителем
        [SerializeField] private TextMeshProUGUI _playerEntryText;
        // LEAD-05: errorText — скрыт по умолчанию, цвет #FF4444
        [SerializeField] private TextMeshProUGUI _errorText;

        protected override void OnConnected()
        {
            if (_titleText != null) { _titleText.text = "LEADERBOARD"; }

            // Показать "Загрузка..." пока данные не загружены
            if (_placeholderText != null) { _placeholderText.gameObject.SetActive(true); }

            // Скрыть все строки Top-10 до получения данных
            foreach (var entry in _entryTexts)
            {
                if (entry != null) { entry.gameObject.SetActive(false); }
            }

            // Скрыть строку текущего игрока и errorText
            if (_playerEntryText != null) { _playerEntryText.gameObject.SetActive(false); }
            if (_errorText != null) { _errorText.gameObject.SetActive(false); }

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(() => ViewModel?.OnBackClicked?.Invoke());
            }

            // Если данные уже есть в ViewModel — отобразить сразу
            if (ViewModel != null)
            {
                Bind(ViewModel);
            }
        }

        // D-06: метод Bind заполняет UI реальными данными из UgsService
        public void Bind(LeaderboardViewModel vm)
        {
            // Скрыть placeholder
            if (_placeholderText != null) { _placeholderText.gameObject.SetActive(false); }

            // LEAD-03: заполнить Top-10 строки
            for (int i = 0; i < 10; i++)
            {
                if (_entryTexts == null || i >= _entryTexts.Length || _entryTexts[i] == null)
                {
                    continue;
                }
                if (vm.Top10Entries != null && i < vm.Top10Entries.Length)
                {
                    var e = vm.Top10Entries[i];
                    // Формат: "3. ACE#1234  42 000" (Pitfall 2: PlayerName содержит #NNNN)
                    _entryTexts[i].text = $"{e.Rank}. {e.PlayerName}  {(int)e.Score:N0}";
                    _entryTexts[i].gameObject.SetActive(true);
                }
                else
                {
                    _entryTexts[i].gameObject.SetActive(false);
                }
            }

            // Показать пустое состояние если нет записей
            if (_placeholderText != null && (vm.Top10Entries == null || vm.Top10Entries.Length == 0))
            {
                _placeholderText.text = "Нет записей. Сыграйте и отправьте счёт!";
                _placeholderText.gameObject.SetActive(true);
            }

            // LEAD-04: позиция текущего игрока
            if (_playerEntryText != null)
            {
                if (vm.PlayerEntry != null)
                {
                    // Формат: "#42 YOU#5678  1 200"
                    _playerEntryText.text = $"#{vm.PlayerEntry.Rank} {vm.PlayerEntry.PlayerName}  {(int)vm.PlayerEntry.Score:N0}";
                    _playerEntryText.gameObject.SetActive(true);
                }
                else
                {
                    // Pitfall 6: игрок не подавал счёт
                    _playerEntryText.text = "—";
                    _playerEntryText.gameObject.SetActive(true);
                }
            }

            // LEAD-05: отобразить ошибку если есть
            if (_errorText != null)
            {
                bool hasError = !string.IsNullOrEmpty(vm.ErrorMessage);
                _errorText.gameObject.SetActive(hasError);
                if (hasError) { _errorText.text = vm.ErrorMessage; }
            }
        }

        protected override void OnDisposed()
        {
            if (_backButton != null) { _backButton.onClick.RemoveAllListeners(); }
        }
    }
}
