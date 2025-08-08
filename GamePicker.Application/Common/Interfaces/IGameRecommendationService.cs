using GamePicker.Contracts.Responses;

namespace GamePicker.Application.Common.Interfaces
{
    public interface IGameRecommendationService
    {
        Task<GameRecommendationResponse> PickGameRecommendation(IReadOnlyList<string> genres, string? platform, int? ramGb);
        Task<PaginatedResponse<GameRecommendationHistoryResponse>> GetRecommendationsHistory(int pageSize, int pageNumber, string? sortBy, string? sortOrder);
    }
}
