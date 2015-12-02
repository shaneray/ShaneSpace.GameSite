using System.Threading.Tasks;
using MediatR;
using ShaneSpace.GameSite.Models;
using ShaneSpace.GameSite.WebApi.ViewModels;
using ShaneSpace.GameSite.Domain.Data;
using System.Linq;
using System;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using FluentValidation.Results;

namespace ShaneSpace.GameSite.WebApi.Cqrs.Games.Command
{
    public class CreateGameCommand : IAsyncRequest<GameSummaryViewModel>
    {
        public int HostId { get; set; }
        public GameConfiguration GameConfig { get; set; }

        public class GameConfiguration
        {
            public string GameName { get; set; }
            public ProgressionMode ProgressionMode { get; set; }
            public GameType GameType { get; set; }
            public string Rules { get; set; }
            public string Description { get; set; }
        }
    }

    public class CreateGameCommandRequestHandler : IAsyncRequestHandler<CreateGameCommand, GameSummaryViewModel>
    {

        private readonly CoreContext _context;

        public CreateGameCommandRequestHandler(CoreContext context)
        {
            _context = context;
        }

        public async Task<GameSummaryViewModel> Handle(CreateGameCommand request)
        {
            if (_context.Games.Any(x => x.Name == request.GameConfig.GameName))
            {
                throw new ValidationException(new[] { new ValidationFailure("Name", $"A game named \"{request.GameConfig.GameName}\" already exist.  Please choose a different name.") });
            }
            var output = new Game
            {
                GameGuid = Guid.NewGuid(),
                Name = request.GameConfig.GameName,
                Rules = request.GameConfig.Rules,
                ProgressionMode = (int)request.GameConfig.ProgressionMode,
                Description = request.GameConfig.Description,
                GameType = (int)request.GameConfig.GameType,
                HostId = request.HostId,
                Status = (int)GameStatus.WaitingForPlayers
            };
            output.Actions.Add(new GameAction {
                UserId = request.HostId,
                Action = (int)Actions.Created,
                DateTime = DateTime.Now,
                ActionValue = $"Created game with the name \"{request.GameConfig.GameName}\""
            });
            _context.Games.Add(output);
            await _context.SaveChangesAsync();

            var gameList = _context.Games.AsNoTracking().Where(x => x.GameId == output.GameId);
            return gameList.ProjectTo<GameSummaryViewModel>().Single();
        }
    }
}