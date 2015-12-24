using Autofac;
using MediatR;
using Microsoft.AspNet.SignalR;
using ShaneSpace.GameSite.Domain.Data;
using ShaneSpace.GameSite.Models;
using ShaneSpace.GameSite.WebApi.Cqrs.Games.Command;
using ShaneSpace.GameSite.WebApi.ViewModels;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq;
using System;

namespace ShaneSpace.GameSite.WebApi.Hubs
{
    [Authorize]
    public class GameHub : BaseHub<GameHub>
    {
        private readonly CoreContext _context;

        public GameHub(ILifetimeScope lifetimeScope) : base(lifetimeScope)
        {
            _context = _hubLifetimeScope.Resolve<CoreContext>();
        }

        public override async Task OnConnected()
        {
            GetUser();

            var gameId = $"Game{Context.QueryString["gameId"]}";
            var userId = $"{gameId}User{_user.Id}";

            JoinGroup(gameId);
            JoinGroup(userId);
            await base.OnConnected();
        }

        public override Task OnReconnected()
        {
            GetUser();

            var gameId = $"Game{Context.QueryString["gameId"]}";
            var userId = $"{gameId}User{_user.Id}";

            JoinGroup(gameId);
            JoinGroup(userId);
            return base.OnReconnected();
        }

        public async Task<MessageViewModel> CreateMessage(dynamic request)
        {
            try
            {
                GetUser();
                var gameId = int.Parse(Context.QueryString["gameId"]);
                var message = await _mediator.SendAsync(new CreateGameMessageCommand
                {
                    ComposerId = _user.Id,
                    GameId = gameId,
                    MessageContents = request.messageContents
                });
                // var clientList = Clients.OthersInGroup($"Game{gameId}");
                // await clientList.gameMessage(message);
                await SendHubMessageToOthersInGroupAsync($"Game{gameId}", GameHubClientMessageType.GameMessage, message);
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
                var recipientId = $"{gameId}User{request.RecipientId}";
                var message = await _mediator.SendAsync(new CreatePrivateMessageCommand
                {
                    ComposerId = _user.Id,
                    RecipientId = request.RecipientId,
                    GameId = int.Parse(Context.QueryString["gameId"]),
                    MessageContents = request.messageContents
                });
                //await Clients.Group(userId).privateMessage(message);
                await SendHubMessageToGroupAsync(recipientId, GameHubClientMessageType.PrivateMessage, message);
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
            //await Clients.OthersInGroup($"Game{gameId}").gameAction(gameAction);
                await SendHubMessageToOthersInGroupAsync($"Game{gameId}", GameHubClientMessageType.GameAction, gameAction);
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
            //await Clients.OthersInGroup($"Game{Context.QueryString["gameId"]}").gameAction(gameAction);
                await SendHubMessageToOthersInGroupAsync($"Game{gameId}", GameHubClientMessageType.GameAction, gameAction);
            var game = _context.Games.Include(x => x.Players).Single(x => x.GameId == gameId);

            if (game.Players.Count() < 2 && game.Status != (int)GameStatus.WaitingForPlayers)
            {
                var statusChangeAction = await _mediator.SendAsync(new AutoStatusChangeCommand { GameId = gameId });
                //await Clients.Group($"Game{Context.QueryString["gameId"]}").gameAction(statusChangeAction);
                await SendHubMessageToGroupAsync($"Game{gameId}", GameHubClientMessageType.GameAction, statusChangeAction);

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
                //await Clients.Group($"Game{gameId}").gameAction(gameAction);
                await SendHubMessageToGroupAsync($"Game{gameId}", GameHubClientMessageType.GameAction, gameAction);
            }
        }

        public async Task CurrentPlayerDieChange(dynamic request)
        {
            GetUser();
            var gameId = int.Parse(Context.QueryString["gameId"]);
            var game = _context.Games.Include(x => x.CurrentGamePlayer).Single(x => x.GameId == gameId);
            if (game.CurrentGamePlayer.UserId == _user.Id)
            {
                //await Clients.OthersInGroup($"Game{gameId}").otherPlayerDieChange(request);
                await SendHubMessageToOthersInGroupAsync($"Game{gameId}", GameHubClientMessageType.OtherPlayerDieChange, request);
            }
        }

        public async Task CurrentPlayerRolledDie(dynamic request)
        {
            GetUser();
            var gameId = int.Parse(Context.QueryString["gameId"]);
            var game = _context.Games.Include(x => x.CurrentGamePlayer).Single(x => x.GameId == gameId);
            if (game.CurrentGamePlayer.UserId == _user.Id)
            {
                //await Clients.OthersInGroup($"Game{gameId}").currentPlayerRolling(request);
                await SendHubMessageToOthersInGroupAsync($"Game{gameId}", GameHubClientMessageType.CurrentPlayerRolling, request);

                var gameActions = await _mediator.SendAsync(new PlayerRolledDieCommand
                {
                    UserId = _user.Id,
                    GameId = gameId,
                    Die = request
                });
                foreach (var gameAction in gameActions)
                {
                    //await Clients.Group($"Game{gameId}").gameAction(gameAction);
                await SendHubMessageToGroupAsync($"Game{gameId}", GameHubClientMessageType.GameAction, gameAction);
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
                //await Clients.Group($"Game{gameId}").gameAction(gameAction);
                await SendHubMessageToGroupAsync($"Game{gameId}", GameHubClientMessageType.GameAction, gameAction);
            }
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