using ShaneSpace.GameSite.Domain.Mapping;
using ShaneSpace.GameSite.Models;
using System;
using System.Collections.Generic;
using AutoMapper;
using ShaneSpace.GameSite.WebApi.ViewModels.User;

namespace ShaneSpace.GameSite.WebApi.ViewModels
{
    public class GameViewModel : ICustomMapping<Game, GameViewModel>
    {
        public GameViewModel()
        {
            PrivateMessages = new List<PrivateMessageViewModel>();
        }

        public int GameId { get; set; }
        public Guid GameGuid { get; set; }
        public string Name { get; set; }
        public GameStatus Status { get; set; }
        public GameType GameType { get; set; }
        public ProgressionMode ProgressionMode { get; set; }
        public UserSummaryViewModel Host { get; set; }
        public List<GamePlayerViewModel> Players { get; set; }
        public GamePlayerViewModel CurrentPlayer { get; set; }
        public string Rules { get; set; }
        public string Description { get; set; }
        public List<GameActionViewModel> Actions { get; set; }
        public List<MessageViewModel> Messages { get; set; }
        public List<PrivateMessageViewModel> PrivateMessages { get; set; }

        public IMappingExpression<Game, GameViewModel> CreateMappings(IConfiguration configuration)
        {
            return Mapper.CreateMap<Game, GameViewModel>()
                .ForMember(dest => dest.GameType, src => src.MapFrom(x => (GameType)x.GameType))
                .ForMember(dest => dest.ProgressionMode, src => src.MapFrom(x => (ProgressionMode)x.ProgressionMode))
                .ForMember(dest => dest.Status, src => src.MapFrom(x => (GameStatus)x.Status))
                .ForMember(dest => dest.CurrentPlayer, src => src.MapFrom(x => x.CurrentGamePlayer))
                .ForMember(dest => dest.PrivateMessages, src => src.Ignore());
        }
    }
}