using System.Threading.Tasks;
using MediatR;
using System.Linq;
using AutoMapper.QueryableExtensions;
using ShaneSpace.GameSite.Domain.Data;
using ShaneSpace.GameSite.Models;
using ShaneSpace.Authentication.WebProxy;
using System;
using System.Configuration;

namespace ShaneSpace.GameSite.WebApi.Cqrs.Users.Command
{
    public class CreateUserCommand : IAsyncRequest<ViewModels.User.UserViewModel>
    {
        public UserConfiguration UserConfig { get; set; }

        public class UserConfiguration
        {
            public string DisplayName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }

    public class CreateUserCommandRequestHandler : IAsyncRequestHandler<CreateUserCommand, ViewModels.User.UserViewModel>
    {
        private readonly CoreContext _context;
        private readonly IUserWebProxy _userWebProxy;

        public CreateUserCommandRequestHandler(CoreContext context, IUserWebProxy userWebProxy)
        {
            _context = context;
            _userWebProxy = userWebProxy;
        }

        public async Task<ViewModels.User.UserViewModel> Handle(CreateUserCommand request)
        {
            // TODO: lame validation, make it better
            var existAlready = _context.Users.Any(x => x.DisplayName == request.UserConfig.DisplayName || x.Email == request.UserConfig.Email);
            if (existAlready)
            {
                throw new ArgumentException("Display name or username already exist or something else bad happened.");
            }

            // create user in shanespace.auth
            var userCreateRequest = await _userWebProxy.RegisterAsync(ConfigurationManager.AppSettings["ShaneSpaceAuthSiteKey"], new RegisterUser
            {
                Email = request.UserConfig.Email,
                Password = request.UserConfig.Password
            });

            // create user in shanespace.gamesite
            var output = new User
            {
                AuthId = userCreateRequest.Id,
                DisplayName = request.UserConfig.DisplayName,
                Email = request.UserConfig.Email
            };
            _context.Users.Add(output);
            await _context.SaveChangesAsync();

            return _context.Users
                .AsNoTracking()
                .Where(x => x.Id == output.Id)
                .ProjectTo<ViewModels.User.UserViewModel>()
                .Single();
        }
    }
}