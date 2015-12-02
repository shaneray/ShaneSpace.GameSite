using AutoMapper.QueryableExtensions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using ShaneSpace.GameSite.Domain.Data;
using ShaneSpace.GameSite.WebApi.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace ShaneSpace.GameSite.WebApi.Cqrs.Games.Query
{
    public class GetGameRequest : IAsyncRequest<GameViewModel>
    {
        public int GameId { get; set; }
        public int UserId { get; set; }
    }

    public class GetGameRequestHandler : IAsyncRequestHandler<GetGameRequest, GameViewModel>
    {
        private readonly CoreContext _context;

        public GetGameRequestHandler(CoreContext context)
        {
            _context = context;
        }

        public async Task<GameViewModel> Handle(GetGameRequest request)
        {
            var gameList = _context.Games.AsNoTracking().Where(x => x.GameId == request.GameId);
            if (!gameList.Any())
            {
                throw new ValidationException(new[] { new ValidationFailure("GameId", $"No game found with an Id of \"{request.GameId}\"") });
            }
            var output = gameList.ProjectTo<GameViewModel>().Single();

            // add private messages
            var privateMessages = _context.PrivateMessages
                .Where(x => x.GameId == request.GameId && (x.RecipientId == request.UserId || x.ComposerId == request.UserId))
                .ProjectTo<PrivateMessageViewModel>();
            output.PrivateMessages.AddRange(privateMessages);

            return output;
        }
    }
}