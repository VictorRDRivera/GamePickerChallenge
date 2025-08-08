using System.ComponentModel;

namespace GamePicker.Contracts.Responses
{
    /// <summary>
    /// Response model for game recommendation history item
    /// </summary>
    public record GameRecommendationHistoryResponse
    {
        /// <summary>
        /// Title of the game
        /// </summary>
        /// <example>Valorant</example>
        public string? Title { get; init; }
        
        /// <summary>
        /// Genre of the game
        /// </summary>
        /// <example>Shooter</example>
        public string? Genre { get; init; }
        
        /// <summary>
        /// Number of times this game has been recommended
        /// </summary>
        /// <example>5</example>
        public int RecommendedTimes { get; init; }
    }
}
