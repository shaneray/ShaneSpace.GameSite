using Autofac;
using MediatR;
using Microsoft.AspNet.SignalR;
using ShaneSpace.GameSite.Domain;
using ShaneSpace.GameSite.Domain.Data;
using ShaneSpace.GameSite.Models;
using ShaneSpace.GameSite.WebApi.Cqrs.Games.Command;
using ShaneSpace.GameSite.WebApi.ViewModels;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq;
using System;

namespace ShaneSpace.GameSite.WebApi.Hubs
{
    [Authorize]
    public class GameHub : Hub
    {
        private User _user;
        private readonly CoreContext _context;
        private readonly ILifetimeScope _hubLifetimeScope;
        private readonly IMediator _mediator;
        private readonly IUserMappingService _userMappingService;

        public GameHub(ILifetimeScope lifetimeScope)
        {
            _hubLifetimeScope = lifetimeScope.BeginLifetimeScope();
            _mediator = _hubLifetimeScope.Resolve<IMediator>();
            _userMappingService = _hubLifetimeScope.Resolve<IUserMappingService>();
            _context = _hubLifetimeScope.Resolve<CoreContext>();
        }

        public override async Task OnConnected()
        {
            GetUser();

            var gameId = $"Game{Context.QueryString["gameId"]}";
            var userId = $"{gameId}User{_user.Id}";

            await Groups.Add(Context.ConnectionId, gameId);
            await Groups.Add(Context.ConnectionId, userId);

            await base.OnConnected();
        }

        public async Task<MessageViewModel> CreateMessage(dynamic request)
        {
            try
            {
                GetUser();

                var message = await _mediator.SendAsync(new CreateGameMessageCommand
                {
                    ComposerId = _user.Id,
                    GameId = int.Parse(Context.QueryString["gameId"]),
                    MessageContents = request.messageContents
                });
                await Clients.OthersInGroup($"Game{Context.QueryString["gameId"]}").gameMessage(message);
                return message;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<PrivateMessageViewModel> CreatePrivateMessage(dynamic request)
        {
            try
            {
                GetUser();
                var gameId = $"Game{Context.QueryString["gameId"]}";
                var userId = $"{gameId}User{_user.Id}";
                var message = await _mediator.SendAsync(new CreatePrivateMessageCommand
                {
                    ComposerId = _user.Id,
                    RecipientId = request.RecipientId,
                    GameId = int.Parse(Context.QueryString["gameId"]),
                    MessageContents = request.messageContents
                });
                await Clients.Group(userId).privateMessage(message);
                return message;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<GameActionViewModel> JoinGame(dynamic request)
        {
            GetUser();
            var gameId = int.Parse(Context.QueryString["gameId"]);
            var game = _context.Games.Include(x => x.CurrentGamePlayer).Single(x => x.GameId == gameId);
            if (game.Players.Any(x => x.UserId == _user.Id))
            {
                throw new ArgumentException("Already joined....");
            }
            var gameAction = await _mediator.SendAsync(new JoinGameCommand
            {
                UserId = _user.Id,
                GameId = int.Parse(Context.QueryString["gameId"])
            });
            await Clients.OthersInGroup($"Game{gameId}").gameAction(gameAction);
            return gameAction;
        }

        public async Task<GameActionViewModel> LeaveGame(dynamic request)
        {
            GetUser();
            var gameId = int.Parse(Context.QueryString["gameId"]);
            var gameAction = await _mediator.SendAsync(new LeaveGameCommand
            {
                UserId = _user.Id,
                GameId = gameId
            });
            await Clients.OthersInGroup($"Game{Context.QueryString["gameId"]}").gameAction(gameAction);
            var game = _context.Games.Include(x => x.Players).Single(x => x.GameId == gameId);

            if (game.Players.Count() < 2 && game.Status != (int)GameStatus.WaitingForPlayers)
            {
                var statusChangeAction = await _mediator.SendAsync(new AutoStatusChangeCommand { GameId = gameId });
                await Clients.Group($"Game{Context.QueryString["gameId"]}").gameAction(statusChangeAction);

            }
            return gameAction;
        }

        public async Task StartGame(dynamic request)
        {
            GetUser();
            var gameId = int.Parse(Context.QueryString["gameId"]);
            var gameActions = await _mediator.SendAsync(new StartGameCommand
            {
                UserId = _user.Id,
                GameId = gameId
            });
            foreach (var gameAction in gameActions)
            {
                await Clients.Group($"Game{gameId}").gameAction(gameAction);
            }
        }

        public async Task CurrentPlayerDieChange(dynamic request)
        {
            GetUser();
            var gameId = int.Parse(Context.QueryString["gameId"]);
            var game = _context.Games.Include(x => x.CurrentGamePlayer).Single(x => x.GameId == gameId);
            if (game.CurrentGamePlayer.UserId == _user.Id)
            {
                await Clients.OthersInGroup($"Game{gameId}").otherPlayerDieChange(request);
            }
        }

        public async Task CurrentPlayerRolledDie(dynamic request)
        {
            GetUser();
            var gameId = int.Parse(Context.QueryString["gameId"]);
            var game = _context.Games.Include(x => x.CurrentGamePlayer).Single(x => x.GameId == gameId);
            if (game.CurrentGamePlayer.UserId == _user.Id)
            {
                await Clients.OthersInGroup($"Game{gameId}").currentPlayerRolling(request);

                var gameActions = await _mediator.SendAsync(new PlayerRolledDieCommand
                {
                    UserId = _user.Id,
                    GameId = gameId,
                    Die = request
                });
                foreach (var gameAction in gameActions)
                {
                    await Clients.Group($"Game{gameId}").gameAction(gameAction);
                }
            }
        }

        public async Task HostChosePlayer(dynamic request)
        {
            GetUser();
            var gameId = int.Parse(Context.QueryString["gameId"]);
            var gameActions = await _mediator.SendAsync(new HostChosePlayerCommand
            {
                UserId = _user.Id,
                GameId = gameId,
                NextPlayerId = request.Id
            });
            foreach (var gameAction in gameActions)
            {
                await Clients.Group($"Game{gameId}").gameAction(gameAction);
            }
        }

        private void GetUser()
        {
            _user = _userMappingService.GetUserFromIdentity(Context.User.Identity);
        }

        public void Heartbeat()
        {
            Clients.All.heartbeat();
        }

        protected override void Dispose(bool disposing)
        {
            // Dipose the hub lifetime scope when the hub is disposed.
            if (disposing && _hubLifetimeScope != null)
            {
                _hubLifetimeScope.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}