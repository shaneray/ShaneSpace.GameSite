using System.Threading.Tasks;
using MediatR;
using ShaneSpace.GameSite.Domain.Data;
using ShaneSpace.GameSite.Models;
using System;
using System.Linq;
using ShaneSpace.GameSite.WebApi.ViewModels;
using System.Data.Entity;
using AutoMapper.QueryableExtensions;

namespace ShaneSpace.GameSite.WebApi.Cqrs.Games.Command
{
    public class LeaveGameCommand : IAsyncRequest<GameActionViewModel>
    {
        public int UserId { get; internal set; }
        public int GameId { get; internal set; }
    }

    public class LeaveGameCommandHandler : IAsyncRequestHandler<LeaveGameCommand, GameActionViewModel>
    {
        private readonly CoreContext _context;
        private readonly IMediator _mediator;

        public LeaveGameCommandHandler(CoreContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<GameActionViewModel> Handle(LeaveGameCommand request)
        {
            var game = _context.Games.Include(x => x.Players).Single(x => x.GameId == request.GameId);
            var gamePlayer = game.Players.Single(x => x.GameId == request.GameId && x.UserId == request.UserId);

            _context.GamePlayers.Remove(gamePlayer);
            var leaveGameAction = new GameAction
            {
                GameId = request.GameId,
                UserId = request.UserId,
                DateTime = DateTime.Now,
                Action = (int)Actions.Exited,
                ActionValue = "has left the game."
            };
            _context.GameActions.Add(leaveGameAction);
            await _context.SaveChangesAsync();
            
            return _context.GameActions.Where(x => x.GameActionId == leaveGameAction.GameActionId).ProjectTo<GameActionViewModel>().Single();
        }
    }
}