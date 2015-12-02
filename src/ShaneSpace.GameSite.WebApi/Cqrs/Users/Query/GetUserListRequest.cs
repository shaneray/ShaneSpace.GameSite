using System.Threading.Tasks;
using MediatR;
using System.Collections.Generic;
using ShaneSpace.GameSite.Domain.Data;
using AutoMapper.QueryableExtensions;
using System.Linq;
using ShaneSpace.GameSite.WebApi.ViewModels.User;

namespace ShaneSpace.GameSite.WebApi.Cqrs.Users.Query
{
    public class GetUserListRequest : IAsyncRequest<List<UserViewModel>>
    {
    }

    public class GetUserListRequestHandler : IAsyncRequestHandler<GetUserListRequest, List<UserViewModel>>
    {
        private readonly CoreContext _context;

        public GetUserListRequestHandler(CoreContext context)
        {
            _context = context;
        }

        public async Task<List<UserViewModel>> Handle(GetUserListRequest request)
        {
            return _context.Users
                .AsNoTracking()
                .ProjectTo<UserViewModel>()
                .ToList();
        }
    }
}