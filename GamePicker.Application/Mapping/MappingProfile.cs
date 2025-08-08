using AutoMapper;
using GamePicker.Application.Common.External;
using GamePicker.Repository.Entities;

namespace GamePicker.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            ExternalGamesMapping();
        }

        private void ExternalGamesMapping()
        {
            CreateMap<FreeToPlayGameResponse, GameRecommendationEntity>()
                .ForMember(dest => dest.GameId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Genre, opt => opt.MapFrom(src => src.Genre))
                .ForMember(dest => dest.RecommendedTimes, opt => opt.MapFrom(src => 1))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}
