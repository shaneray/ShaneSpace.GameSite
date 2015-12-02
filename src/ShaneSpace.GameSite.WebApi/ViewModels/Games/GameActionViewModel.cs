using ShaneSpace.GameSite.Domain.Mapping;
using ShaneSpace.GameSite.Models;
using ShaneSpace.GameSite.WebApi.ViewModels.User;
using System;
using AutoMapper;

namespace ShaneSpace.GameSite.WebApi.ViewModels
{
    public class GameActionViewModel : ICustomMapping<GameAction, GameActionViewModel>
    {
        public int GameActionId { get; set; }
        public DateTime DateTime { get; set; }
        public Actions Action { get; set; }
        public string ActionValue { get; set; }
        public virtual UserSummaryViewModel User { get; set; }
        public dynamic AdditionalInfo { get; set; }

        public IMappingExpression<GameAction, GameActionViewModel> CreateMappings(IConfiguration configuration)
        {
            return Mapper.CreateMap<GameAction, GameActionViewModel>()
                .ForMember(dest => dest.Action, src => src.MapFrom(x => (Actions)x.Action));
        }
    }
}