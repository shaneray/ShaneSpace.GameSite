using MediatR;
using ShaneSpace.GameSite.Domain;
using ShaneSpace.GameSite.Models;
using ShaneSpace.GameSite.WebApi.Cqrs.Feedback.Command;
using ShaneSpace.GameSite.WebApi.Cqrs.Feedback.Query;
using ShaneSpace.GameSite.WebApi.ViewModels.Feedback;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace ShaneSpace.GameSite.WebApi.Controllers
{
    [Authorize]
    [RoutePrefix("feedback")]
    public class FeedbackController : ApiController
    {
        private readonly IMediator _mediator;
        private readonly IUserMappingService _userMappingService;
        private readonly User _user;

        public FeedbackController(IMediator mediator, IUserMappingService userMappingService)
        {
            _mediator = mediator;
            _userMappingService = userMappingService;
            _user = _userMappingService.GetUserFromIdentity(User.Identity);
        }

        [HttpPost]
        [Route]
        [ResponseType(typeof(FeedbackViewModel))]
        public async Task<IHttpActionResult> AddFeedback([FromBody]AddFeedbackCommand.AddFeedbackCommandBody body)
        {
            var viewModel = await _mediator.SendAsync(new AddFeedbackCommand
            {
                UserId = _user.Id,
                FeedbackBody = body
            });
            return Created(string.Empty, viewModel);
        }

        [HttpGet]
        [Route]
        [ResponseType(typeof(FeedbackViewModel[]))]
        public async Task<IHttpActionResult> GetFeedback()
        {
            return Ok(await _mediator.SendAsync(new GetFeedbackRequest()));
        }
    }
}
