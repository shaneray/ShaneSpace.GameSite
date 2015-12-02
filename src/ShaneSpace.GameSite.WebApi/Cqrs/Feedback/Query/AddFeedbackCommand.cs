using System.Threading.Tasks;
using MediatR;
using ShaneSpace.GameSite.WebApi.ViewModels.Feedback;
using ShaneSpace.GameSite.Domain.Data;
using System.Linq;
using AutoMapper.QueryableExtensions;

namespace ShaneSpace.GameSite.WebApi.Cqrs.Feedback.Query
{
    public class GetFeedbackRequest : IAsyncRequest<FeedbackViewModel[]>
    {
    }

    public class GetFeedbackRequestHandler : IAsyncRequestHandler<GetFeedbackRequest, FeedbackViewModel[]>
    {
        private readonly CoreContext _context;

        public GetFeedbackRequestHandler(CoreContext context)
        {
            _context = context;
        }

        public async Task<FeedbackViewModel[]> Handle(GetFeedbackRequest message)
        {
            return _context.Feedback.ProjectTo<FeedbackViewModel>().ToArray();
        }
    }
}