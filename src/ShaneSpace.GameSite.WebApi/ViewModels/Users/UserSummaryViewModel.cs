using AutoMapper;
using ShaneSpace.GameSite.Domain.Mapping;
using ShaneSpace.GameSite.Models;

namespace ShaneSpace.GameSite.WebApi.ViewModels.User
{
    public class UserSummaryViewModel : IMapFrom<Models.User>
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
    }

    public class GamePlayerViewModel : ICustomMapping<GamePlayer, GamePlayerViewModel>
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }

        public IMappingExpression<GamePlayer, GamePlayerViewModel> CreateMappings(IConfiguration configuration)
        {
            return Mapper.CreateMap<GamePlayer, GamePlayerViewModel>()
                .ForMember(dest => dest.Id, src => src.MapFrom(x => x.User.Id))
                .ForMember(dest => dest.DisplayName, src => src.MapFrom(x => x.User.DisplayName));
        }
    }
}