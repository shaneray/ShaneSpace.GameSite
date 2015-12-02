using System.Threading.Tasks;
using MediatR;
using ShaneSpace.GameSite.Models;
using ShaneSpace.GameSite.WebApi.ViewModels.Feedback;
using ShaneSpace.GameSite.Domain.Data;
using System.Linq;
using AutoMapper.QueryableExtensions;

namespace ShaneSpace.GameSite.WebApi.Cqrs.Feedback.Command
{
    public class AddFeedbackCommand : IAsyncRequest<FeedbackViewModel>
    {
        public int UserId { get; set; }

        public AddFeedbackCommandBody FeedbackBody { get; set; }

        public class AddFeedbackCommandBody
        {
            public FeedbackTypes FeedbackType { get; set; }
            public string FeedbackText { get; set; }
        }
    }

    public class AddFeedbackCommandHandler : IAsyncRequestHandler<AddFeedbackCommand, FeedbackViewModel>
    {
        private readonly CoreContext _context;

        public AddFeedbackCommandHandler(CoreContext context)
        {
            _context = context;
        }

        public async Task<FeedbackViewModel> Handle(AddFeedbackCommand message)
        {
            var newFeedback = new Models.Feedback
            {
                UserId = message.UserId,
                FeedbackType = (int)message.FeedbackBody.FeedbackType,
                FeedbackText = message.FeedbackBody.FeedbackText
            };
            _context.Feedback.Add(newFeedback);
            await _context.SaveChangesAsync();
            return _context.Feedback.Where(x => x.FeedbackId == newFeedback.FeedbackId).ProjectTo<FeedbackViewModel>().Single();
        }
    }
}