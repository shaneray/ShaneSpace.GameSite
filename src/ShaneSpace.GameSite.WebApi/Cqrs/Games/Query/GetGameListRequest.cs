using System.Threading.Tasks;
using MediatR;
using ShaneSpace.GameSite.WebApi.ViewModels;
using System.Collections.Generic;
using ShaneSpace.GameSite.Domain.Data;
using AutoMapper.QueryableExtensions;
using System.Linq;
using ShaneSpace.GameSite.Models;

namespace ShaneSpace.GameSite.WebApi.Cqrs.Games.Query
{
    public class GetGameListRequest : IAsyncRequest<List<GameSummaryViewModel>>
    {
        public bool ActiveOnly { get; set; }
    }

    public class GetGameListRequestHandler : IAsyncRequestHandler<GetGameListRequest, List<GameSummaryViewModel>>
    {
        private readonly CoreContext _context;

        public GetGameListRequestHandler(CoreContext context)
        {
            _context = context;
        }

        public async Task<List<GameSummaryViewModel>> Handle(GetGameListRequest request)
        {
            var gameList = _context.Games.AsNoTracking();
            if (request.ActiveOnly)
            {
                gameList.Where(x => x.Status == (int)GameStatus.Active);
            }
            return gameList.ProjectTo<GameSummaryViewModel>().ToList();
        }
    }
}