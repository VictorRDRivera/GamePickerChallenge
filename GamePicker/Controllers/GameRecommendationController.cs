using GamePicker.Application.Common.Exceptions;
using GamePicker.Contracts;
using Microsoft.AspNetCore.Mvc;
using GamePicker.Contracts.Responses;
using GamePicker.Application.Common.Interfaces;

namespace GamePicker.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class GameRecommendationController : ControllerBase
    {
        private readonly ILogger<GameRecommendationController> _logger;
        private readonly IGameRecommendationService _gameRecommendationService;

        public GameRecommendationController(ILogger<GameRecommendationController> logger, IGameRecommendationService gameRecommendationService)
        {
            _logger = logger;
            _gameRecommendationService = gameRecommendationService;
        }

        /// <summary>
        /// Get a personalized game recommendation based on specified filters
        /// </summary>
        /// <param name="request">Game filter criteria including genres, platform, and RAM requirements</param>
        /// <returns>A personalized game recommendation with title, link, and custom message</returns>
        /// <response code="200">Returns the recommended game</response>
        /// <response code="400">Invalid request parameters</response>
        /// <response code="404">No games found matching the criteria</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("/recommendation")]
        [ProducesResponseType(typeof(GameRecommendationResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> PickGameRecommendation(GameFilterRequest request)
        {
            try
            {
                var gameRecommendationResponse = await _gameRecommendationService.PickGameRecommendation(request.Genres, request.Platform, request.RamGb);
                
                if (gameRecommendationResponse == null)
                {
                    throw new NotFoundException("No games found matching the specified criteria");
                }
                
                return Ok(gameRecommendationResponse);
            }
            catch (Exception ex) when (ex is not GamePickerException)
            {
                _logger.LogError(ex, "Error occurred while picking game recommendation");
                throw new InternalServerException("Failed to process game recommendation request");
            }
        }

        /// <summary>
        /// Get paginated history of game recommendations with sorting options
        /// </summary>
        /// <param name="request">Pagination and sorting parameters</param>
        /// <returns>Paginated list of game recommendations with title, genre, and recommendation count</returns>
        /// <response code="200">Returns the paginated game recommendations history</response>
        /// <response code="400">Invalid pagination parameters</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("/recommendations/history")]
        [ProducesResponseType(typeof(PaginatedResponse<GameRecommendationResponse>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetGameRecommendationsHistory([FromQuery] PaginationRequest request)
        {
            try
            {
                var gameRecommendationsHistory = await _gameRecommendationService.GetRecommendationsHistory(
                    request.PageSize, 
                    request.PageNumber, 
                    request.SortBy, 
                    request.SortOrder);
                return Ok(gameRecommendationsHistory);
            }
            catch (Exception ex) when (ex is not GamePickerException)
            {
                _logger.LogError(ex, "Error occurred while retrieving game recommendations history");
                throw new InternalServerException("Failed to retrieve game recommendations history");
            }
        }
    }
}
