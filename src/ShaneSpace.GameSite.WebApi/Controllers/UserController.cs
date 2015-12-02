using Birch.Swagger.ProxyGenerator;
using MediatR;
using ShaneSpace.GameSite.Domain;
using ShaneSpace.GameSite.Models;
using ShaneSpace.GameSite.WebApi.Cqrs.Users.Command;
using ShaneSpace.GameSite.WebApi.Cqrs.Users.Query;
using ShaneSpace.GameSite.WebApi.ViewModels.User;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace ShaneSpace.GameSite.WebApi.Controllers
{
    [Authorize]
    [RoutePrefix("users")]
    public class UserController : ApiController
    {
        private readonly IMediator _mediator;
        private User _user;
        private readonly IUserMappingService _userMappingService;

        public UserController(IMediator mediator, IUserMappingService userMappingService)
        {
            _mediator = mediator;
            _userMappingService = userMappingService;
        }

        /// <summary>
        /// Gets the list of users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route()]
        [ResponseType(typeof(List<UserViewModel>))]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> UserList()
        {
            _user = _userMappingService.GetUserFromIdentity(User.Identity);
            return Ok(await _mediator.SendAsync(new GetUserListRequest()));
        }

        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route()]
        [ResponseType(typeof(UserViewModel))]
        [AllowAnonymous]
        public async Task<IHttpActionResult> CreateNewUser(CreateUserCommand.UserConfiguration userConfiguration)
        {
            var user = await _mediator.SendAsync(new CreateUserCommand
            {
                UserConfig = userConfiguration
            });
            var location = string.Format("/users/{0}", user.Id);
            return Created(location, user);
        }

        /// <summary>
        /// Authenticate user
        /// </summary>
        /// <returns>Bearer token</returns>
        [HttpPost]
        [Route("authenticate")]
        [ResponseType(typeof(string))]
        [AllowAnonymous]
        public async Task<IHttpActionResult> AuthenticateUser(string email, string password)
        {
            try
            {
                var token = await _mediator.SendAsync(new AuthenticateUserCommand
                {
                    Email = email,
                    Password = password
                });
                return Ok(token);
            }
            catch (SimpleHttpResponseException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Unauthorized)
                {
                   return Unauthorized();
                }
                throw;
            }
        }

        /// <summary>
        /// Gets the info for the current user
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("current")]
        [ResponseType(typeof(UserViewModel))]
        public async Task<IHttpActionResult> GetCurrentUserInfo()
        {
            _user = _userMappingService.GetUserFromIdentity(User.Identity);
            var viewModel = await _mediator.SendAsync(new GetUserRequest { UserId = _user.Id });
            var roles = new List<string>();
            foreach (var claim in ((ClaimsIdentity)User.Identity).Claims)
            {
                if (claim.Type == ShaneSpaceClaimTypes.Role)
                {
                    roles.Add(claim.Value);
                }
            }
            viewModel.Roles = roles.ToArray();
            return Ok(viewModel);
        }
    }
}
