using ShaneSpace.GameSite.Domain;
using ShaneSpace.GameSite.WebApi.Hubs;
using System.Threading.Tasks;
using System.Web.Http;

namespace ShaneSpace.GameSite.WebApi.Controllers
{
    [RoutePrefix("debug")]
    public class DebugController : ApiController
    {
        private readonly IUserMappingService _userMappingService;
        private readonly GameHub _gameHub;

        public DebugController(IUserMappingService userMappingService, GameHub gameHub)
        {
            _userMappingService = userMappingService;
            _gameHub = gameHub;
        }

        [HttpGet]
        [Route("echo")]
        [Authorize]
        public IHttpActionResult Echo(string text)
        {
            var user = _userMappingService.GetUserFromIdentity(User.Identity);
            return Ok(string.Format("{0}:{1} ({2}) - {3}", user.Id, user.Email, user.DisplayName, text));
        }

        [HttpGet]
        [Route("game-hub")]
        [Authorize]
        public IHttpActionResult GameHub()
        {
            return Ok(new { Users = _gameHub.Clients, Groups = _gameHub.Groups });
        }

        [HttpGet]
        [Route("game-hub-client-notification")]
        [Authorize]
        public async Task<IHttpActionResult> GameHubClientNotification(string connectionId, string contents)
        {
            return Ok(await _gameHub.SendHubMessageToClientAsync(connectionId, GameHubClientMessageType.AdminNotification, contents));
        }

        [HttpGet]
        [Route("game-hub-user-notification")]
        [Authorize]
        public async Task<IHttpActionResult> GameHubUserNotification(string userName, string contents)
        {
            var clients = _gameHub.GetConnectionsForUser(userName);
            return Ok(await _gameHub.SendHubMessageToClientsAsync(clients, GameHubClientMessageType.AdminNotification, contents));
        }

        [HttpGet]
        [Route("game-hub-group-notification")]
        [Authorize]
        public async Task<IHttpActionResult> GameHubGroupNotification(string groupName, string contents)
        {
            return Ok(await _gameHub.SendHubMessageToGroupAsync(groupName, GameHubClientMessageType.AdminNotification, contents));
        }
    }
}
