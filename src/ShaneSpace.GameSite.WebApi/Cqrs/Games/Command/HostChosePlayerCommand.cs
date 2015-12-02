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
    public class HostChosePlayerCommand : IAsyncRequest<GameActionViewModel[]>
    {
        public int UserId { get; internal set; }
        public int GameId { get; internal set; }
        public int NextPlayerId { get; internal set; }
    }

    public class HostChosePlayerCommandHandler : IAsyncRequestHandler<HostChosePlayerCommand, GameActionViewModel[]>
    {
        private readonly CoreContext _context;

        public HostChosePlayerCommandHandler(CoreContext context)
        {
            _context = context;
        }

        public async Task<GameActionViewModel[]> Handle(HostChosePlayerCommand request)
        {
            var game = _context.Games
                .Include(x => x.Players.Select(p => p.User))
                .FirstOrDefault(x => x.GameId == request.GameId);

            if (game.HostId != request.UserId)
            {
                throw new ValidationException(new[] { new ValidationFailure("HostId", $"User with Id of \"{request.UserId}\" is not the host.") });
            }

            // chose player game
            var nextPlayer = game.Players.Single(x => x.UserId == request.NextPlayerId);
            var gameActionList = new List<GameAction>();
            game.CurrentGamePlayerId = nextPlayer.GamePlayerId;
            var gameAction = new GameAction
            {
                GameId = request.GameId,
                UserId = request.UserId,
                DateTime = DateTime.Now,
                Action = (int)Actions.StatusChanged,
                ActionValue = $"has selected the next player \"{nextPlayer.User.DisplayName}\"."
            };
            _context.GameActions.Add(gameAction);
            game.Status = (int)GameStatus.WaitingForPlayer;
            gameActionList.Add(gameAction);
            await _context.SaveChangesAsync();
            return GameHelpers.ProcessNextPlayerResults(gameActionList, nextPlayer, gameAction, _context);
        }
    }
}