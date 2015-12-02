using AutoMapper.QueryableExtensions;
using ShaneSpace.GameSite.Domain.Data;
using ShaneSpace.GameSite.Models;
using ShaneSpace.GameSite.WebApi.ViewModels;
using ShaneSpace.GameSite.WebApi.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShaneSpace.GameSite.WebApi
{
    public static class GameHelpers
    {
        public static GameActionViewModel[] ProcessNextPlayerResults(List<GameAction> gameActionList, GamePlayer nextPlayer, GameAction nextPlayerAction, CoreContext context)
        {
            var gameActionIdList = gameActionList.Select(x => x.GameActionId);
            var output = context.GameActions.Where(x => gameActionIdList.Contains(x.GameActionId)).ProjectTo<GameActionViewModel>().ToArray();

            // attach what user we are waiting.
            if (nextPlayer != null)
            {
                var action = output.First(x => x.GameActionId == nextPlayerAction.GameActionId);
                var userViewModel = context.Users.Where(x => x.Id == nextPlayer.UserId).ProjectTo<UserSummaryViewModel>().Single();
                action.AdditionalInfo = new { User = userViewModel, Status = GameStatus.WaitingForPlayer };
            }
            else
            {
                output.Last().AdditionalInfo = new { Status = GameStatus.WaitingForHost };
            }
            return output;
        }

        public static void GetNexPlayer(dynamic request, Game game, List<GameAction> gameActionList, out GamePlayer nextPlayer, out GameAction nextPlayerAction, CoreContext context)
        {
            nextPlayer = null;
            nextPlayerAction = new GameAction();
            if (game.ProgressionMode == (int)ProgressionMode.RoundRobin)
            {
                if (game.CurrentGamePlayerId != null)
                {
                    nextPlayer = game.Players.FirstOrDefault(x => x.GamePlayerId > game.CurrentGamePlayerId);
                }
                if (nextPlayer == null)
                {
                    nextPlayer = game.Players.OrderBy(x => x.GamePlayerId).First();
                }

                game.CurrentGamePlayerId = nextPlayer.GamePlayerId;
                game.Status = (int)GameStatus.WaitingForPlayer;

                nextPlayerAction = new GameAction
                {
                    GameId = request.GameId,
                    UserId = Constants.SystemUser,
                    DateTime = DateTime.Now,
                    Action = (int)Actions.StatusChanged,
                    ActionValue = $"has selected the next player \"{nextPlayer.User.DisplayName}\"."
                };
                gameActionList.Add(nextPlayerAction);
                context.GameActions.Add(nextPlayerAction);
            }

            if (game.ProgressionMode == (int)ProgressionMode.HostChoice)
            {
                game.CurrentGamePlayerId = null;
                game.Status = (int)GameStatus.WaitingForHost;
                var waitingForHostAction = new GameAction
                {
                    GameId = request.GameId,
                    UserId = Constants.SystemUser,
                    DateTime = DateTime.Now,
                    Action = (int)Actions.StatusChanged,
                    ActionValue = $"is waiting for the host to slect the next player."
                };
                gameActionList.Add(waitingForHostAction);
                context.GameActions.Add(waitingForHostAction);
            }
        }
    }
}
