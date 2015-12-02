using System.Threading.Tasks;
using MediatR;
using ShaneSpace.GameSite.Domain.Data;
using ShaneSpace.GameSite.Models;
using System;
using ShaneSpace.GameSite.WebApi.ViewModels;
using System.Linq;
using AutoMapper.QueryableExtensions;

namespace ShaneSpace.GameSite.WebApi.Cqrs.Games.Command
{
    public class JoinGameCommand : IAsyncRequest<GameActionViewModel>
    {
        public int UserId { get; internal set; }
        public int GameId { get; internal set; }
    }

    public class JoinGameCommandHandler : IAsyncRequestHandler<JoinGameCommand, GameActionViewModel>
    {
        private readonly CoreContext _context;

        public JoinGameCommandHandler(CoreContext context)
        {
            _context = context;
        }

        public async Task<GameActionViewModel> Handle(JoinGameCommand request)
        {
            _context.GamePlayers.Add(new GamePlayer { UserId = request.UserId, GameId = request.GameId });
            var gameAction = new GameAction
            {
                GameId = request.GameId,
                UserId = request.UserId,
                DateTime = DateTime.Now,
                Action = (int)Actions.Joined,
                ActionValue = "has joined the game."
            };
            _context.GameActions.Add(gameAction);
            await _context.SaveChangesAsync();
            return _context.GameActions.Where(x => x.GameActionId == gameAction.GameActionId).ProjectTo<GameActionViewModel>().Single();
        }
    }
}