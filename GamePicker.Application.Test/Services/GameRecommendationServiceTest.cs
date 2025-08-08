using FluentAssertions;
using GamePicker.Application.Common.Exceptions;
using GamePicker.Application.Common.External;
using GamePicker.Application.Common.Interfaces;
using GamePicker.Application.Common.Interfaces.Persistence;
using GamePicker.Application.Services.GameRecommendation;
using GamePicker.Contracts.Responses;
using GamePicker.Repository.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using AutoMapper;

namespace GamePicker.Application.Test.Services
{
    public class GameRecommendationServiceTest
    {
        private readonly Mock<IFreeToPlayGamesClient> _mockFreeToPlayGamesClient;
        private readonly Mock<IGameRecommendationRepository> _mockRepository;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ILogger<GameRecommendationService>> _mockLogger;
        private readonly GameRecommendationService _service;

        public GameRecommendationServiceTest()
        {
            _mockFreeToPlayGamesClient = new Mock<IFreeToPlayGamesClient>();
            _mockRepository = new Mock<IGameRecommendationRepository>();
            _mockCacheService = new Mock<ICacheService>();
            _mockLogger = new Mock<ILogger<GameRecommendationService>>();
            var mockMapper = new Mock<IMapper>();
            _service = new GameRecommendationService(
                _mockFreeToPlayGamesClient.Object,
                _mockRepository.Object,
                _mockCacheService.Object,
                mockMapper.Object);
        }

        [Fact]
        public async Task PickGameRecommendation_WhenSuccessful_ShouldReturnGameRecommendation()
        {
            var genres = new List<string> { "Action" };
            var platform = "pc";
            var ramGb = 8;
            var games = new List<FreeToPlayGameResponse>
            {
                new() { Id = 1, Title = "Test Game", Genre = "Action", Platform = "PC", FreetogameProfileUrl = "https://example.com" }
            };

            _mockCacheService.Setup(c => c.GetAsync<GameRecommendationResponse>(It.IsAny<string>()))
                .ReturnsAsync((GameRecommendationResponse)null);

            _mockCacheService.Setup(c => c.GetAsync<IReadOnlyList<FreeToPlayGameResponse>>(It.IsAny<string>()))
                .ReturnsAsync((IReadOnlyList<FreeToPlayGameResponse>)null);

            _mockFreeToPlayGamesClient.Setup(c => c.GetFilteredGames(genres, platform))
                .ReturnsAsync(games);

            _mockFreeToPlayGamesClient.Setup(c => c.GetGame(It.IsAny<int>()))
                .ReturnsAsync(games[0]);

            _mockRepository.Setup(r => r.GetByGameId(It.IsAny<int>()))
                .ReturnsAsync((GameRecommendationEntity)null);

            _mockRepository.Setup(r => r.Add(It.IsAny<GameRecommendationEntity>()))
                .Returns(Task.CompletedTask);

            var result = await _service.PickGameRecommendation(genres, platform, ramGb);

            result.Should().NotBeNull();
            result.Title.Should().Be("Test Game");
            result.Message.Should().Contain("Action");
            result.Message.Should().Contain("Test Game");
        }

        [Fact]
        public async Task PickGameRecommendation_WhenNoGamesFound_ShouldThrowNotFoundException()
        {
            var genres = new List<string> { "Action" };
            var platform = "pc";

            _mockCacheService.Setup(c => c.GetAsync<List<FreeToPlayGameResponse>>(It.IsAny<string>()))
                .ReturnsAsync((List<FreeToPlayGameResponse>)null);

            _mockFreeToPlayGamesClient.Setup(c => c.GetFilteredGames(genres, platform))
                .ThrowsAsync(new NotFoundException("No games found"));

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.PickGameRecommendation(genres, platform, null));

            exception.Message.Should().Be("No games found");
        }

        [Fact]
        public async Task PickGameRecommendation_WhenGameExistsInCache_ShouldUseCachedData()
        {
            var genres = new List<string> { "Action" };
            var platform = "pc";
            var cachedRecommendation = new GameRecommendationResponse
            {
                Title = "Cached Game",
                LinkFromApiSite = "https://example.com",
                Message = "Do you like Action? You definitely should play Cached Game"
            };

            _mockCacheService.Setup(c => c.GetAsync<GameRecommendationResponse>(It.IsAny<string>()))
                .ReturnsAsync(cachedRecommendation);

            // Act
            var result = await _service.PickGameRecommendation(genres, platform, null);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("Cached Game");
            _mockFreeToPlayGamesClient.Verify(c => c.GetFilteredGames(It.IsAny<IReadOnlyList<string>>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetRecommendationsHistory_WhenSuccessful_ShouldReturnPaginatedHistory()
        {
            var entities = new List<GameRecommendationEntity>
            {
                new() { GameId = 1, Title = "Game 1", Genre = "Action", RecommendedTimes = 5 },
                new() { GameId = 2, Title = "Game 2", Genre = "RPG", RecommendedTimes = 3 }
            };

            _mockCacheService.Setup(c => c.GetAsync<PaginatedResponse<GameRecommendationHistoryResponse>>(It.IsAny<string>()))
                .ReturnsAsync((PaginatedResponse<GameRecommendationHistoryResponse>)null);

            _mockRepository.Setup(r => r.GetPaginated(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((entities, 2L));

            // Act
            var result = await _service.GetRecommendationsHistory(10, 1, "title", "asc");

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetRecommendationsHistory_WhenCacheExists_ShouldUseCachedData()
        {
            var cachedResponse = new PaginatedResponse<GameRecommendationHistoryResponse>
            {
                Items = new List<GameRecommendationHistoryResponse>
                {
                    new() { Title = "Cached Game", Genre = "Action", RecommendedTimes = 1 }
                },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 10
            };

            _mockCacheService.Setup(c => c.GetAsync<PaginatedResponse<GameRecommendationHistoryResponse>>(It.IsAny<string>()))
                .ReturnsAsync(cachedResponse);

            var result = await _service.GetRecommendationsHistory(10, 1, "title", "asc");

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items.First().Title.Should().Be("Cached Game");
            _mockRepository.Verify(r => r.GetPaginated(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetRecommendationsHistory_WhenNoDataFound_ShouldReturnEmptyList()
        {
            _mockCacheService.Setup(c => c.GetAsync<PaginatedResponse<GameRecommendationHistoryResponse>>(It.IsAny<string>()))
                .ReturnsAsync((PaginatedResponse<GameRecommendationHistoryResponse>)null);

            _mockRepository.Setup(r => r.GetPaginated(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((new List<GameRecommendationEntity>(), 0L));

            var result = await _service.GetRecommendationsHistory(10, 1, "title", "asc");

            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }
    }
}
