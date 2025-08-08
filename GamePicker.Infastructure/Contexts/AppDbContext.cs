using GamePicker.Repository.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamePicker.Repository.Contexts
{
    public class AppDbContext : DbContext
    {
        public DbSet<GameRecommendationEntity> GameRecommendations { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

    }
}
