using GamePicker.Application.Common.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePicker.Application.Common.Interfaces
{
    public interface IFreeToPlayGamesClient
    {
        Task<FreeToPlayGameResponse?> GetGame(int id);
        Task<List<FreeToPlayGameResponse>> GetFilteredGames(IReadOnlyList<string> tags, string? platform);

    }
}
