using GamePicker.Application.Common.Interfaces;
using GamePicker.Application.Services.GameRecommendation;
using Microsoft.Extensions.DependencyInjection;

namespace GamePicker.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IGameRecommendationService, GameRecommendationService>();
            return services;
        }
    }
}
