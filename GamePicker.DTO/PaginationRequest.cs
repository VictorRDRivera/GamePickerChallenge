using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace GamePicker.Contracts
{
    /// <summary>
    /// Request model for pagination and sorting parameters
    /// </summary>
    public record PaginationRequest
    {
        /// <summary>
        /// Number of items per page. Must be between 1 and 100.
        /// </summary>
        /// <example>10</example>
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; init; } = 10;

        /// <summary>
        /// Page number to retrieve. Must be greater than 0.
        /// </summary>
        /// <example>1</example>
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; init; } = 1;

        /// <summary>
        /// Field to sort by. Options: Title, Genre, RecommendedTimes
        /// </summary>
        /// <example>title</example>
        public string? SortBy { get; init; } = "Title";

        /// <summary>
        /// Sort order. Options: 'asc' (ascending) or 'desc' (descending)
        /// </summary>
        /// <example>asc</example>
        public string? SortOrder { get; init; } = "asc"; // asc, desc
    }
}
