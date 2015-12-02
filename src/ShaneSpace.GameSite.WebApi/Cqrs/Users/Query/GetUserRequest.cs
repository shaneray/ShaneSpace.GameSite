using AutoMapper.QueryableExtensions;
using MediatR;
using ShaneSpace.GameSite.Domain.Data;
using ShaneSpace.GameSite.WebApi.ViewModels.User;
using System.Linq;
using System.Threading.Tasks;

namespace ShaneSpace.GameSite.WebApi.Cqrs.Users.Query
{
    public class GetUserRequest : IAsyncRequest<UserViewModel>
    {
        public int UserId { get; set; }
    }

    public class GetUserRequestHandler : IAsyncRequestHandler<GetUserRequest, UserViewModel>
    {
        private readonly CoreContext _context;

        public GetUserRequestHandler(CoreContext context)
        {
            _context = context;
        }

        public async Task<UserViewModel> Handle(GetUserRequest request)
        {
            var userList = _context.Users.AsNoTracking().Where(x => x.Id == request.UserId);
            return userList.ProjectTo<UserViewModel>().Single();
        }
    }
}