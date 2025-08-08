using FluentAssertions;
using GamePicker.Application.Common.Interfaces;
using GamePicker.Application.Common.Exceptions;
using GamePicker.Contracts;
using GamePicker.Contracts.Responses;
using GamePicker.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GamerPicker.Api.Test.Controllers
{
    public class GameRecommendationControllerTest
    {
        private readonly Mock<IGameRecommendationService> _mockService;
        private readonly Mock<ILogger<GameRecommendationController>> _mockLogger;
        private readonly GameRecommendationController _controller;

        public GameRecommendationControllerTest()
        {
            _mockService = new Mock<IGameRecommendationService>();
            _mockLogger = new Mock<ILogger<GameRecommendationController>>();
            _controller = new GameRecommendationController(_mockLogger.Object, _mockService.Object);
        }

        [Fact]
        public async Task PickGameRecommendation_WhenSuccessful_ShouldReturnOkResult()
        {
            var filter = new GameFilterRequest { Genres = new List<string> { "Action" }, Platform = "pc" };
            var expectedResult = new GameRecommendationResponse
            {
                Title = "Test Game",
                LinkFromApiSite = "https://example.com",
                Message = "Do you like Action? You definitely should play Test Game"
            };

            _mockService.Setup(s => s.PickGameRecommendation(filter.Genres, filter.Platform, filter.RamGb))
                .ReturnsAsync(expectedResult);

            var result = await _controller.PickGameRecommendation(filter);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeOfType<GameRecommendationResponse>();
            
            var response = okResult.Value as GameRecommendationResponse;
            response!.Title.Should().Be("Test Game");
            response.Message.Should().Contain("Action");
        }

        [Fact]
        public async Task PickGameRecommendation_WhenServiceThrowsException_ShouldThrowInternalServerException()
        {
            var filter = new GameFilterRequest { Genres = new List<string> { "Action" }, Platform = "pc" };
            _mockService.Setup(s => s.PickGameRecommendation(filter.Genres, filter.Platform, filter.RamGb))
                .ThrowsAsync(new Exception("Service error"));

            var exception = await Assert.ThrowsAsync<InternalServerException>(
                () => _controller.PickGameRecommendation(filter));

            exception.Message.Should().Contain("Failed to process game recommendation request");
        }

        [Fact]
        public async Task GetGameRecommendationsHistory_WhenSuccessful_ShouldReturnOkResult()
        {
            var request = new PaginationRequest { PageSize = 10, PageNumber = 1, SortBy = "title", SortOrder = "asc" };
            var expectedResult = new PaginatedResponse<GameRecommendationHistoryResponse>
            {
                Items = new List<GameRecommendationHistoryResponse>
                {
                    new() { Title = "Game 1", Genre = "Action", RecommendedTimes = 5 },
                    new() { Title = "Game 2", Genre = "RPG", RecommendedTimes = 3 }
                },
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 10
            };

            _mockService.Setup(s => s.GetRecommendationsHistory(request.PageSize, request.PageNumber, request.SortBy, request.SortOrder))
                .ReturnsAsync(expectedResult);

            var result = await _controller.GetGameRecommendationsHistory(request);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeOfType<PaginatedResponse<GameRecommendationHistoryResponse>>();
            
            var response = okResult.Value as PaginatedResponse<GameRecommendationHistoryResponse>;
            response!.Items.Should().HaveCount(2);
            response.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task GetGameRecommendationsHistory_WhenNoData_ShouldReturnEmptyList()
        {
            var request = new PaginationRequest { PageSize = 10, PageNumber = 1, SortBy = "title", SortOrder = "asc" };
            var expectedResult = new PaginatedResponse<GameRecommendationHistoryResponse>
            {
                Items = new List<GameRecommendationHistoryResponse>(),
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10
            };

            _mockService.Setup(s => s.GetRecommendationsHistory(request.PageSize, request.PageNumber, request.SortBy, request.SortOrder))
                .ReturnsAsync(expectedResult);

            var result = await _controller.GetGameRecommendationsHistory(request);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as PaginatedResponse<GameRecommendationHistoryResponse>;
            response!.Items.Should().BeEmpty();
            response.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetGameRecommendationsHistory_WhenServiceThrowsException_ShouldThrowInternalServerException()
        {
            var request = new PaginationRequest { PageSize = 10, PageNumber = 1, SortBy = "title", SortOrder = "asc" };
            _mockService.Setup(s => s.GetRecommendationsHistory(request.PageSize, request.PageNumber, request.SortBy, request.SortOrder))
                .ThrowsAsync(new Exception("Service error"));

            var exception = await Assert.ThrowsAsync<InternalServerException>(
                () => _controller.GetGameRecommendationsHistory(request));

            exception.Message.Should().Contain("Failed to retrieve game recommendations history");
        }

        [Theory]
        [InlineData(10, 1, "title", "asc")]
        [InlineData(50, 2, "genre", "desc")]
        [InlineData(100, 3, "recommendedtimes", "asc")]
        public async Task GetGameRecommendationsHistory_WithDifferentParameters_ShouldCallServiceCorrectly(
            int pageSize, int pageNumber, string sortBy, string sortOrder)
        {
            var request = new PaginationRequest { PageSize = pageSize, PageNumber = pageNumber, SortBy = sortBy, SortOrder = sortOrder };
            var expectedResult = new PaginatedResponse<GameRecommendationHistoryResponse>
            {
                Items = new List<GameRecommendationHistoryResponse>(),
                TotalCount = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _mockService.Setup(s => s.GetRecommendationsHistory(pageSize, pageNumber, sortBy, sortOrder))
                .ReturnsAsync(expectedResult);

            var result = await _controller.GetGameRecommendationsHistory(request);

            result.Should().BeOfType<OkObjectResult>();
            _mockService.Verify(s => s.GetRecommendationsHistory(pageSize, pageNumber, sortBy, sortOrder), Times.Once);
        }
    }
}
