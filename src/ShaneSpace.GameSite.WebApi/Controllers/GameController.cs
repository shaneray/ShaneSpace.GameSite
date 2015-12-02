using FluentValidation;
using MediatR;
using ShaneSpace.GameSite.Domain;
using ShaneSpace.GameSite.Models;
using ShaneSpace.GameSite.WebApi.Cqrs.Games.Command;
using ShaneSpace.GameSite.WebApi.Cqrs.Games.Query;
using ShaneSpace.GameSite.WebApi.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace ShaneSpace.GameSite.WebApi.Controllers
{
    [Authorize]
    [RoutePrefix("games")]
    public class GameController : ApiController
    {
        private readonly IMediator _mediator;
        private readonly User _user;
        private readonly IUserMappingService _userMappingService;

        public GameController(IMediator mediator, IUserMappingService userMappingService)
        {
            _mediator = mediator;
            _userMappingService = userMappingService;
            _user = _userMappingService.GetUserFromIdentity(User.Identity);
        }

        /// <summary>
        /// Gets the list of games
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route()]
        [ResponseType(typeof(List<GameSummaryViewModel>))]
        public async Task<IHttpActionResult> GameList(bool activeOnly = true)
        {
            return Ok(await _mediator.SendAsync(new GetGameListRequest()));
        }

        /// <summary>
        /// Creates a new game
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route()]
        [ResponseType(typeof(GameViewModel))]
        public async Task<IHttpActionResult> CreateNewGame(CreateGameCommand.GameConfiguration gameConfiguration)
        {
            try
            {
                var game = await _mediator.SendAsync(new CreateGameCommand
                {
                    HostId = _user.Id,
                    GameConfig = gameConfiguration
                });
                var location = string.Format("/games/{0}", game.GameId);
                return Created(location, game);
            }
            catch (Exception ex)
            {
                if (ex is EntityCommandExecutionException)
                {
                    throw ex.InnerException;
                }
                if (ex is ValidationException)
                {
                    return BadRequest(ex.Message);
                }
                throw;
            }
        }

        /// <summary>
        /// Gets the info for a game
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{gameId}")]
        [ResponseType(typeof(GameViewModel))]
        public async Task<IHttpActionResult> GetGameInfo(int gameId)
        {
            try
            {
                return Ok(await _mediator.SendAsync(new GetGameRequest { GameId = gameId, UserId = _user.Id }));
            }
            catch(Exception ex)
            {
                if (ex is EntityCommandExecutionException)
                {
                    throw ex.InnerException;
                }
                if (ex is ValidationException)
                {
                    return BadRequest(ex.Message);
                }
                throw;
            }
        }

        /// <summary>
        /// Adds a message to the game
        /// </summary>
        /// <returns></returns>
        /// [HttpPost]
        [HttpPost]
        [Route("{gameId}/messages")]
        [ResponseType(typeof(MessageViewModel))]
        public async Task<IHttpActionResult> CreateMessage(int gameId, string messageContents)
        {
            var message = await _mediator.SendAsync(new CreateGameMessageCommand
            {
                ComposerId = _user.Id,
                GameId = gameId,
                MessageContents = messageContents
            });

            return Created("", message);
        }
    }
}
