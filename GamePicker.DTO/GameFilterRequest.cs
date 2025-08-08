using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace GamePicker.Contracts
{
    /// <summary>
    /// Request model for filtering game recommendations
    /// </summary>
    public record GameFilterRequest
    {
        /// <summary>
        /// List of game genres to filter by. At least one genre is required.
        /// </summary>
        /// <example>["Shooter", "RPG", "Strategy"]</example>
        [Required(ErrorMessage = "At least 1 genre is required")]
        [MinLength(1)]
        public required IReadOnlyList<string> Genres { get; set; }

        /// <summary>
        /// Platform to filter games by. Options: 'pc', 'browser', or 'all'
        /// </summary>
        /// <example>pc</example>
        [RegularExpression("^(?i)(pc|browser|all)$", ErrorMessage = "Platform should be 'pc', 'browser' or 'all'")]
        public string? Platform { get; set; }

        /// <summary>
        /// Maximum RAM in GB to filter games by system requirements
        /// </summary>
        /// <example>8</example>
        [Range(1, int.MaxValue, ErrorMessage = "Ram number must be greater than 0")]
        public int? RamGb { get; set; }
    }
}
