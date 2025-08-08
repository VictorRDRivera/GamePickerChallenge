using System.ComponentModel;

namespace GamePicker.Contracts.Responses
{
    /// <summary>
    /// Generic paginated response model
    /// </summary>
    /// <typeparam name="T">Type of items in the paginated response</typeparam>
    public record PaginatedResponse<T>
    {
        /// <summary>
        /// Collection of items for the current page
        /// </summary>
        public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
        
        /// <summary>
        /// Total number of items across all pages
        /// </summary>
        /// <example>150</example>
        public int TotalCount { get; init; }
        
        /// <summary>
        /// Current page number
        /// </summary>
        /// <example>1</example>
        public int PageNumber { get; init; }
        
        /// <summary>
        /// Number of items per page
        /// </summary>
        /// <example>10</example>
        public int PageSize { get; init; }
        
        /// <summary>
        /// Total number of pages
        /// </summary>
        /// <example>15</example>
        public int TotalPages { get; init; }
        
        /// <summary>
        /// Indicates if there is a next page available
        /// </summary>
        /// <example>true</example>
        public bool HasNextPage { get; init; }
        
        /// <summary>
        /// Indicates if there is a previous page available
        /// </summary>
        /// <example>false</example>
        public bool HasPreviousPage { get; init; }
    }
}
