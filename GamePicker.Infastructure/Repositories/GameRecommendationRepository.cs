using GamePicker.Application.Common.Interfaces.Persistence;
using GamePicker.Repository.Entities;
using MongoDB.Driver;

namespace GamePicker.Repository.Repositories
{
    public class GameRecommendationRepository : IGameRecommendationRepository
    {
        private readonly IMongoCollection<GameRecommendationEntity> _collection;

        public GameRecommendationRepository(IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase("gamepicker");
            _collection = database.GetCollection<GameRecommendationEntity>("GameRecommendations");
        }

        public async Task Add(GameRecommendationEntity gameRecommendation)
            => await _collection.InsertOneAsync(gameRecommendation);

        public async Task<IReadOnlyList<GameRecommendationEntity>> GetGames()
            => await _collection.Find(FilterDefinition<GameRecommendationEntity>.Empty).ToListAsync();
            
        public async Task<GameRecommendationEntity?> GetByGameId(int gameId)
            => await _collection.Find(x => x.GameId == gameId).FirstOrDefaultAsync();
            
        public async Task Update(GameRecommendationEntity gameRecommendation)
            => await _collection.ReplaceOneAsync(x => x.GameId == gameRecommendation.GameId, gameRecommendation);

        public async Task<(IReadOnlyList<GameRecommendationEntity> Data, long TotalCount)> GetPaginated(int pageSize, int pageNumber, string? sortBy, string? sortOrder)
        {
            var totalCount = await _collection.CountDocumentsAsync(FilterDefinition<GameRecommendationEntity>.Empty);
            var filter = FilterDefinition<GameRecommendationEntity>.Empty;
            var sort = CreateSortDefinition(sortBy, sortOrder);
            
            var data = await _collection
                .Find(filter)
                .Sort(sort)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
            
            return (data, totalCount);
        }

        private SortDefinition<GameRecommendationEntity> CreateSortDefinition(string? sortBy, string? sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";
            
            return sortBy?.ToLower() switch
            {
                "title" => isDescending ? Builders<GameRecommendationEntity>.Sort.Descending(x => x.Title) : Builders<GameRecommendationEntity>.Sort.Ascending(x => x.Title),
                "id" => isDescending ? Builders<GameRecommendationEntity>.Sort.Descending(x => x.GameId) : Builders<GameRecommendationEntity>.Sort.Ascending(x => x.GameId),
                "createdat" => isDescending ? Builders<GameRecommendationEntity>.Sort.Descending(x => x.CreatedAt) : Builders<GameRecommendationEntity>.Sort.Ascending(x => x.CreatedAt),
                "recommendedtimes" => isDescending ? Builders<GameRecommendationEntity>.Sort.Descending(x => x.RecommendedTimes) : Builders<GameRecommendationEntity>.Sort.Ascending(x => x.RecommendedTimes),
                _ => Builders<GameRecommendationEntity>.Sort.Ascending(x => x.Title)
            };
        }
    }
}
