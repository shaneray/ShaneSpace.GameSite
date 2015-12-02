using System.Threading.Tasks;
using MediatR;
using ShaneSpace.Authentication.WebProxy;
using System.Configuration;

namespace ShaneSpace.GameSite.WebApi.Cqrs.Users.Command
{
    public class AuthenticateUserCommand : IAsyncRequest<string>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class AuthenticateUserCommandRequestHandler : IAsyncRequestHandler<AuthenticateUserCommand, string>
    {
        private readonly IOAuthWebProxy _oAuthWebProxy;

        public AuthenticateUserCommandRequestHandler(IOAuthWebProxy userWebProxy)
        {
            _oAuthWebProxy = userWebProxy;
        }

        public async Task<string> Handle(AuthenticateUserCommand request)
        {
            var authenticateUserResponse = await _oAuthWebProxy.TokenAsync(request.Email, request.Password, "password", ConfigurationManager.AppSettings["ShaneSpaceAuthSiteKey"]);
            return authenticateUserResponse.access_token;
        }
    }
}