using ShaneSpace.GameSite.Domain;
using System.Web.Http;

namespace ShaneSpace.GameSite.WebApi.Controllers
{
    [RoutePrefix("debug")]
    public class DebugController : ApiController
    {
        private readonly IUserMappingService _userMappingService;

        public DebugController(IUserMappingService userMappingService)
        {
            _userMappingService = userMappingService;
        }

        [HttpGet]
        [Route("echo")]
        [Authorize]
        public IHttpActionResult Echo(string text)
        {
            var user = _userMappingService.GetUserFromIdentity(User.Identity);
            return Ok(string.Format("{0}:{1} ({2}) - {3}", user.Id, user.Email, user.DisplayName, text));
        }
    }
}
