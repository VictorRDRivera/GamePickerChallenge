using GamePicker.Repository.Entities;

namespace GamePicker.Application.Common.Interfaces.Persistence
{
    public interface IGameRecommendationRepository
    {
        Task Add(GameRecommendationEntity gameRecommendation);
        Task<IReadOnlyList<GameRecommendationEntity>> GetGames();
        Task<GameRecommendationEntity?> GetByGameId(int gameId);
        Task Update(GameRecommendationEntity gameRecommendation);
        Task<(IReadOnlyList<GameRecommendationEntity> Data, long TotalCount)> GetPaginated(int pageSize, int pageNumber, string? sortBy, string? sortOrder);
    }
}
