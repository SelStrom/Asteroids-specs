using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using System;

namespace SelStrom.Asteroids
{
    // UGS-сервис: plain C# класс без MonoBehaviour (паттерн AudioManager, D-08)
    // Все async операции через Task — оборачиваются в Coroutine в ApplicationEntry (D-09)
    public class UgsService
    {
        private readonly string _leaderboardId;

        public UgsService(string leaderboardId)
        {
            _leaderboardId = leaderboardId;
        }

        // LEAD-01: Guest sign-in. SignInAnonymouslyAsync автоматически использует кэшированный токен.
        // Pitfall 3: проверяем IsSignedIn перед вызовом.
        public async Task InitializeAsync()
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }

        // LEAD-02: установить имя ПЕРЕД отправкой — оно будет видно в лидерборде (Pitfall 3)
        public async Task SubmitScoreAsync(string playerName, int score)
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
            await LeaderboardsService.Instance.AddPlayerScoreAsync(_leaderboardId, (double)score);
        }

        // LEAD-03: Top-10 (Limit=10 по умолчанию, Offset=0)
        public async Task<LeaderboardScoresPage> GetTopScoresAsync()
        {
            return await LeaderboardsService.Instance.GetScoresAsync(_leaderboardId);
        }

        // LEAD-04: позиция текущего игрока. Pitfall 6: возвращает null если игрок не подавал счёт.
        public async Task<LeaderboardEntry> GetPlayerScoreAsync()
        {
            try
            {
                return await LeaderboardsService.Instance.GetPlayerScoreAsync(_leaderboardId);
            }
            catch (Exception)
            {
                // Игрок не в лидерборде (NotFound) — вернуть null
                return null;
            }
        }
    }
}
