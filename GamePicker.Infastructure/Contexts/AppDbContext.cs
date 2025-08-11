using GamePicker.Repository.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamePicker.Infastructure.Contexts
{
    public class AppDbContext : DbContext
    {
        public DbSet<GameRecommendationEntity> GameRecommendations { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GameRecommendationEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.GameId).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.RecommendedTimes).HasDefaultValue(1);
            });
        }
    }
}
