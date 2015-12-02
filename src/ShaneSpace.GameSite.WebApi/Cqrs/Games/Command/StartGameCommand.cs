using System.Threading.Tasks;
using MediatR;
using ShaneSpace.GameSite.Domain.Data;
using ShaneSpace.GameSite.Models;
using System;
using ShaneSpace.GameSite.WebApi.ViewModels;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using System.Collections.Generic;
using System.Data.Entity;

namespace ShaneSpace.GameSite.WebApi.Cqrs.Games.Command
{
    public class StartGameCommand : IAsyncRequest<GameActionViewModel[]>
    {
        public int UserId { get; internal set; }
        public int GameId { get; internal set; }
    }

    public class StartGameCommandHandler : IAsyncRequestHandler<StartGameCommand, GameActionViewModel[]>
    {
        private readonly CoreContext _context;

        public StartGameCommandHandler(CoreContext context)
        {
            _context = context;
        }

        public async Task<GameActionViewModel[]> Handle(StartGameCommand request)
        {
            var game = _context.Games
                .Include(x => x.Players.Select(p => p.User))
                .FirstOrDefault(x => x.GameId == request.GameId);

            if (game.HostId != request.UserId)
            {
                throw new ValidationException(new[] { new ValidationFailure("HostId", $"User with Id of \"{request.UserId}\" is not the host.") });
            }

            // Start game
            var gameActionList = new List<GameAction>();
            var gameAction = new GameAction
            {
                GameId = request.GameId,
                UserId = request.UserId,
                DateTime = DateTime.Now,
                Action = (int)Actions.StartedGame,
                ActionValue = "has started the game."
            };
            _context.GameActions.Add(gameAction);
            gameActionList.Add(gameAction);

            // set next player
            GamePlayer nextPlayer;
            GameAction nextPlayerAction;
            GameHelpers.GetNexPlayer(request, game, gameActionList, out nextPlayer, out nextPlayerAction, _context);
            await _context.SaveChangesAsync();

            return GameHelpers.ProcessNextPlayerResults(gameActionList, nextPlayer, nextPlayerAction, _context);
        }
    }
}