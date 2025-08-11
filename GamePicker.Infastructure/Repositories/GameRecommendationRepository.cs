using GamePicker.Application.Common.Interfaces.Persistence;
using GamePicker.Repository.Entities;
using GamePicker.Infastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GamePicker.Infastructure.Repositories
{
    public class GameRecommendationRepository : IGameRecommendationRepository
    {
        private readonly AppDbContext _context;

        public GameRecommendationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task Add(GameRecommendationEntity gameRecommendation)
        {
            await _context.GameRecommendations.AddAsync(gameRecommendation);
            await _context.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<GameRecommendationEntity>> GetGames()
            => await _context.GameRecommendations.ToListAsync();
            
        public async Task<GameRecommendationEntity?> GetByGameId(int gameId)
            => await _context.GameRecommendations.FirstOrDefaultAsync(x => x.GameId == gameId);
            
        public async Task Update(GameRecommendationEntity gameRecommendation)
        {
            _context.GameRecommendations.Update(gameRecommendation);
            await _context.SaveChangesAsync();
        }

        public async Task<(IReadOnlyList<GameRecommendationEntity> Data, long TotalCount)> GetPaginated(int pageSize, int pageNumber, string? sortBy, string? sortOrder)
        {
            var query = _context.GameRecommendations.AsQueryable();
            
            query = ApplySorting(query, sortBy, sortOrder);
            
            var totalCount = await query.CountAsync();
            
            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return (data, totalCount);
        }

        static IQueryable<GameRecommendationEntity> ApplySorting(IQueryable<GameRecommendationEntity> query, string? sortBy, string? sortOrder)
        {
            var isDescending = sortOrder?.ToLower() == "desc";
            
            return sortBy?.ToLower() switch
            {
                "title" => isDescending ? query.OrderByDescending(x => x.Title) : query.OrderBy(x => x.Title),
                "id" => isDescending ? query.OrderByDescending(x => x.GameId) : query.OrderBy(x => x.GameId),
                "createdat" => isDescending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
                "recommendedtimes" => isDescending ? query.OrderByDescending(x => x.RecommendedTimes) : query.OrderBy(x => x.RecommendedTimes),
                _ => query.OrderBy(x => x.Title)
            };
        }
    }
}
