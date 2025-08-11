using GamePicker.Application.Common.Interfaces.Persistence;
using GamePicker.Infastructure.Repositories;
using GamePicker.Infastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GamePicker.Infastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? "Server=localhost;Database=GamePicker;Trusted_Connection=true;TrustServerCertificate=true;";
            
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));
            
            services.AddScoped<IGameRecommendationRepository, GameRecommendationRepository>();
            
            return services;
        }
    }
}
