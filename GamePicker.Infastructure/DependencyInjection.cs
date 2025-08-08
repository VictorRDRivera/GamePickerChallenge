using GamePicker.Application.Common.Interfaces.Persistence;
using GamePicker.Repository.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace GamePicker.Infastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services)
        {
            services.AddScoped<IGameRecommendationRepository, GameRecommendationRepository>();
            return services;
        }
    }
}
