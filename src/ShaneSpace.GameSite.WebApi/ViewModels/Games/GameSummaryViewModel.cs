using ShaneSpace.GameSite.Domain.Mapping;
using ShaneSpace.GameSite.Models;
using AutoMapper;

namespace ShaneSpace.GameSite.WebApi.ViewModels
{
    public class GameSummaryViewModel : ICustomMapping<Game, GameSummaryViewModel>
    {
        public int GameId { get; set; }
        public string Name { get; set; }
        public GameStatus Status { get; set; }
        public GameType GameType { get; set; }
        public ProgressionMode ProgressionMode { get; set; }
        public User.UserSummaryViewModel Host { get; set; }
        public int PlayerCount { get; set; }

        public IMappingExpression<Game, GameSummaryViewModel> CreateMappings(IConfiguration configuration)
        {
            return Mapper.CreateMap<Game, GameSummaryViewModel>()
                .ForMember(dest => dest.GameType, src => src.MapFrom(x => (GameType)x.GameType))
                .ForMember(dest => dest.Status, src => src.MapFrom(x => (GameStatus)x.Status))
                .ForMember(dest => dest.ProgressionMode, src => src.MapFrom(x => (ProgressionMode)x.ProgressionMode))
                .ForMember(dest => dest.PlayerCount, src => src.MapFrom(x => x.Players.Count));
        }
    }
}