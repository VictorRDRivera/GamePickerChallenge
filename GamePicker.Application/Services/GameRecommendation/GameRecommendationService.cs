using GamePicker.Application.Common.External;
using GamePicker.Application.Common.Interfaces.Persistence;
using GamePicker.Application.Common.utils;
using GamePicker.Application.Common.Exceptions;
using GamePicker.Repository.Entities;
using AutoMapper;
using GamePicker.Contracts.Responses;
using GamePicker.Application.Common.Interfaces;

namespace GamePicker.Application.Services.GameRecommendation
{
    public class GameRecommendationService : IGameRecommendationService
    {
        private readonly IFreeToPlayGamesClient _freeToPlayGamesClient;
        private readonly IGameRecommendationRepository _gameRecommendationRepository;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;

        public GameRecommendationService(
            IFreeToPlayGamesClient freeToPlayGamesClient, 
            IGameRecommendationRepository gameRecommendationRepository, 
            ICacheService cacheService,
            IMapper mapper)
        {
            _freeToPlayGamesClient = freeToPlayGamesClient;
            _gameRecommendationRepository = gameRecommendationRepository;
            _cacheService = cacheService;
            _mapper = mapper;
        }

        public async Task<GameRecommendationResponse> PickGameRecommendation(IReadOnlyList<string> genres, string? platform, int? ramGb)
        {
            var cacheKey = GenerateRecommendationCacheKey(genres, platform, ramGb);
            var cachedRecommendation = await _cacheService.GetAsync<GameRecommendationResponse>(cacheKey);
            
            if (cachedRecommendation != null)
            {
                return cachedRecommendation;
            }

            var filteredGames = await GetFilteredGames(genres, platform);
            var shuffledGameIds = GetShuffledGameIds(filteredGames);

            foreach (var gameId in shuffledGameIds)
            {
                var game = await GetGameDetails(gameId);

                if (game == null) continue;
                if (!IsGameCompatibleWithRam(game, ramGb)) continue;

                await SaveOrUpdateRecommendation(game);
                
                var recommendation = new GameRecommendationResponse
                { 
                    Title = game.Title, 
                    LinkFromApiSite = game.FreetogameProfileUrl,
                    Message = GenerateRecommendationMessage(game)
                };

                await _cacheService.SetAsync(cacheKey, recommendation, TimeSpan.FromMinutes(5));
                
                return recommendation;
            }

            throw new NotFoundException("No games found compatible with the specified RAM memory");
        }

        private string GenerateRecommendationCacheKey(IReadOnlyList<string> genres, string? platform, int? ramGb)
        {
            var genresStr = string.Join("_", genres.OrderBy(g => g));
            var platformStr = platform ?? "all";
            var ramStr = ramGb?.ToString() ?? "any";
            
            return $"recommendation_{genresStr}_{platformStr}_{ramStr}";
        }

        private async Task<IReadOnlyList<FreeToPlayGameResponse>> GetFilteredGames(IReadOnlyList<string> genres, string? platform)
        {
            var cacheKey = GenerateFilteredGamesCacheKey(genres, platform);
            var cachedGames = await _cacheService.GetAsync<IReadOnlyList<FreeToPlayGameResponse>>(cacheKey);
            
            if (cachedGames != null)
            {
                return cachedGames;
            }

            var filteredGames = await _freeToPlayGamesClient.GetFilteredGames(genres, platform);

            if (filteredGames.Count == 0)
            {
                throw new NotFoundException("No games found with the provided filters");
            }

            await _cacheService.SetAsync(cacheKey, filteredGames, TimeSpan.FromHours(1));

            return filteredGames;
        }

        private string GenerateFilteredGamesCacheKey(IReadOnlyList<string> genres, string? platform)
        {
            var genresStr = string.Join("_", genres.OrderBy(g => g));
            var platformStr = platform ?? "all";
            
            return $"filtered_games_{genresStr}_{platformStr}";
        }

        private List<int> GetShuffledGameIds(IReadOnlyList<FreeToPlayGameResponse> games)
        {
            var random = new Random();
            return games.Select(g => g.Id).OrderBy(_ => random.Next()).ToList();
        }

        private async Task<FreeToPlayGameResponse?> GetGameDetails(int gameId)
        {
            var cacheKey = $"game_details_{gameId}";
            var cachedGame = await _cacheService.GetAsync<FreeToPlayGameResponse>(cacheKey);
            
            if (cachedGame != null)
            {
                return cachedGame;
            }

            var game = await _freeToPlayGamesClient.GetGame(gameId);
            
            if (game != null)
            {
                await _cacheService.SetAsync(cacheKey, game, TimeSpan.FromHours(24));
            }

            return game;
        }

        private bool IsGameCompatibleWithRam(FreeToPlayGameResponse game, int? ramGb)
        {
            if (!ramGb.HasValue) return true;

            if (!SystemRequirementParser.TryParseMaxRam(game.MinimumSystemRequirements?.Memory, out var requiredRam))
                return true;

            return requiredRam <= ramGb.Value;
        }

        private async Task SaveOrUpdateRecommendation(FreeToPlayGameResponse game)
        {
            var existingRecommendation = await _gameRecommendationRepository.GetByGameId(game.Id);
            
            if (existingRecommendation != null)
            {
                existingRecommendation.RecommendedTimes++;
                await _gameRecommendationRepository.Update(existingRecommendation);
            }
            else
            {
                var recommendation = _mapper.Map<GameRecommendationEntity>(game);
                await _gameRecommendationRepository.Add(recommendation);
            }

            await _cacheService.RemoveByPatternAsync("*_history_*");
        }

        public async Task<PaginatedResponse<GameRecommendationHistoryResponse>> GetRecommendationsHistory(int pageSize, int pageNumber, string? sortBy, string? sortOrder)
        {
            var cacheKey = GenerateHistoryCacheKey(pageSize, pageNumber, sortBy, sortOrder);
            var cachedHistory = await _cacheService.GetAsync<PaginatedResponse<GameRecommendationHistoryResponse>>(cacheKey);
            
            if (cachedHistory != null)
            {
                return cachedHistory;
            }

            var (pagedRecommendations, totalCount) = await _gameRecommendationRepository.GetPaginated(pageSize, pageNumber, sortBy, sortOrder);

            var historyItems = pagedRecommendations.Select(r => new GameRecommendationHistoryResponse
            {
                Title = r.Title,
                Genre = r.Genre,
                RecommendedTimes = r.RecommendedTimes
            }).ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var response = new PaginatedResponse<GameRecommendationHistoryResponse>
            {
                Data = historyItems,
                TotalCount = (int)totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasNextPage = pageNumber < totalPages,
                HasPreviousPage = pageNumber > 1
            };

            await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

            return response;
        }

        private string GenerateHistoryCacheKey(int pageSize, int pageNumber, string? sortBy, string? sortOrder)
        {
            var sortByStr = sortBy ?? "Title";
            var sortOrderStr = sortOrder ?? "asc";
            
            return $"history_{pageSize}_{pageNumber}_{sortByStr}_{sortOrderStr}";
        }

        private string GenerateRecommendationMessage(FreeToPlayGameResponse game)
        {
            return $"Do you like {game.Genre}? You definitely should play {game.Title}!";
        }
    }
}
