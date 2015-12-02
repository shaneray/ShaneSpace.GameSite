using System.Threading.Tasks;
using MediatR;
using ShaneSpace.GameSite.Domain.Data;
using ShaneSpace.GameSite.Models;
using System;
using ShaneSpace.GameSite.WebApi.ViewModels;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;

namespace ShaneSpace.GameSite.WebApi.Cqrs.Games.Command
{
    public class PlayerRolledDieCommand : IAsyncRequest<GameActionViewModel[]>
    {
        public int UserId { get; internal set; }
        public int GameId { get; internal set; }
        public dynamic Die { get; internal set; }
    }

    public class PlayerRolledDieCommandHandler : IAsyncRequestHandler<PlayerRolledDieCommand, GameActionViewModel[]>
    {
        private readonly CoreContext _context;
        private readonly Random _random;
        public PlayerRolledDieCommandHandler(CoreContext context)
        {
            _context = context;
            _random = new Random();
        }

        public async Task<GameActionViewModel[]> Handle(PlayerRolledDieCommand request)
        {
            var game = _context.Games
                .Include(x => x.Players.Select(p => p.User))
                .FirstOrDefault(x => x.GameId == request.GameId);

            // wait 5 seconds so you can see a nice spinning animation in UI...
            await Task.Delay(5000);

            // calculate roll
            List<Die> dieList = request.Die.ToObject<List<Die>>();
            var dieCount = dieList.Count;
            
            foreach (var die in dieList)
            {
                die.DieValue = _random.Next(1, die.DieSideCount);
            }
            // build game action
            var gameActionList = new List<GameAction>();
            var gameAction = new GameAction
            {
                GameId = request.GameId,
                UserId = request.UserId,
                DateTime = DateTime.Now,
                Action = (int)Actions.Moved,
                ActionValue = $"has rolled {dieCount} die and got the following values: {string.Join(", ", dieList.Select(x => $"D{x.DieSideCount}:{x.DieValue}"))}"
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