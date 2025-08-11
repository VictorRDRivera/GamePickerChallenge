using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamePicker.Repository.Entities
{
    [Table("GameRecommendations")]
    public class GameRecommendationEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        public int GameId { get; set; }
        
        [MaxLength(255)]
        public string? Title { get; set; }
        
        [MaxLength(100)]
        public string? Genre { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public int RecommendedTimes { get; set; } = 1;
    }
}
