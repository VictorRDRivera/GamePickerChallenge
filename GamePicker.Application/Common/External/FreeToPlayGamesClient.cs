using RestSharp;
using GamePicker.Application.Common.Exceptions;
using System.Text.Json;
using GamePicker.Application.Common.Interfaces;

namespace GamePicker.Application.Common.External
{
    public class FreeToGameClient : IFreeToPlayGamesClient
    {
        private readonly RestClient _restClient;

        public FreeToGameClient(RestClient restClient)
        {
            _restClient = restClient;
        }

        public async Task<FreeToPlayGameResponse?> GetGame(int id)
        {
            var request = new RestRequest("api/game", Method.Get);
            request.AddQueryParameter("id", id.ToString());

            var response = await _restClient.ExecuteAsync<FreeToPlayGameResponse>(request);

            if (!response.IsSuccessful)
            {
                throw new ExternalApiException($"Error on External API: {response.StatusCode} - {response.Content}");
            }

            return response.Data;
        }

        public async Task<List<FreeToPlayGameResponse>> GetFilteredGames(IReadOnlyList<string> tags, string? platform)
        {
            var request = new RestRequest("api/filter", Method.Get);

            if (tags is { Count: > 0 })
                request.AddQueryParameter("tag", string.Join('.', tags.Select(t => t.ToLowerInvariant())));

            if (!string.IsNullOrWhiteSpace(platform))
                request.AddQueryParameter("platform", platform.ToLowerInvariant());

            var response = await _restClient.ExecuteAsync<List<FreeToPlayGameResponse>>(request);

            if (response.Content?.Contains("No active giveaways available at the moment, please try again later") == true)
                throw new NotFoundException("No games found with the provided filters");

            if (!response.IsSuccessful)
                throw new ExternalApiException($"Error on External API (filter): {response.StatusCode} - {response.Content}");

            return response.Data ?? new List<FreeToPlayGameResponse>();
        }
    }
}
