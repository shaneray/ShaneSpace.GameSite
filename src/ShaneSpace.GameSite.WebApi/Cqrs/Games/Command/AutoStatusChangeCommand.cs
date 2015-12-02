using System.Threading.Tasks;
using MediatR;
using ShaneSpace.GameSite.Domain.Data;
using System;
using ShaneSpace.GameSite.Models;
using System.Linq;
using AutoMapper.QueryableExtensions;
using ShaneSpace.GameSite.WebApi.ViewModels;

namespace ShaneSpace.GameSite.WebApi.Cqrs.Games.Command
{
    public class AutoStatusChangeCommand : IAsyncRequest<GameActionViewModel>
    {
        public int GameId { get; internal set; }
    }

    public class AutoStatusChangeCommandHandler : IAsyncRequestHandler<AutoStatusChangeCommand, GameActionViewModel>
    {
        private readonly CoreContext _context;

        public AutoStatusChangeCommandHandler(CoreContext context)
        {
            _context = context;
        }

        public async Task<GameActionViewModel> Handle(AutoStatusChangeCommand request)
        {
            var game = _context.Games.Single(x => x.GameId == request.GameId);
            game.Status = (int)GameStatus.WaitingForPlayers;
            var leaveGameAction = new GameAction
            {
                UserId = Constants.SystemUser,
                DateTime = DateTime.Now,
                Action = (int)Actions.StatusChanged,
                ActionValue = "has automatically changed the game status to \"Waiting For Players\" because too many players have left the game."
            };
            game.Actions.Add(leaveGameAction);
            await _context.SaveChangesAsync();
            var viewModel = _context.GameActions.Where(x => x.GameActionId == leaveGameAction.GameActionId).ProjectTo<GameActionViewModel>().Single();
            viewModel.AdditionalInfo = new { Status = GameStatus.WaitingForPlayers };
            return viewModel;
        }
    }
}