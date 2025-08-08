using System.ComponentModel;

namespace GamePicker.Contracts.Responses
{
    /// <summary>
    /// Response model for a game recommendation
    /// </summary>
    public record GameRecommendationResponse
    {
        /// <summary>
        /// Title of the recommended game
        /// </summary>
        /// <example>Valorant</example>
        public string? Title { get; init; }
        
        /// <summary>
        /// Direct link to the game's profile on the external API site
        /// </summary>
        /// <example>https://www.freetogame.com/games/valorant</example>
        public string? LinkFromApiSite { get; init; }
        
        /// <summary>
        /// Personalized recommendation message
        /// </summary>
        /// <example>Do you like shooter? You definitely should play Valorant!</example>
        public string? Message { get; init; }
    }
}
