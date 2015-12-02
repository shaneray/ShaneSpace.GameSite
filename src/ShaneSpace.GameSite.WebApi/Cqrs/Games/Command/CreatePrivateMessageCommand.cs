using System.Threading.Tasks;
using MediatR;
using ShaneSpace.GameSite.Models;
using ShaneSpace.GameSite.WebApi.ViewModels;
using ShaneSpace.GameSite.Domain.Data;
using System.Linq;
using System;
using AutoMapper.QueryableExtensions;

namespace ShaneSpace.GameSite.WebApi.Cqrs.Games.Command
{
    public class CreatePrivateMessageCommand : IAsyncRequest<PrivateMessageViewModel>
    {
        public int ComposerId { get; internal set; }
        public int GameId { get; internal set; }
        public string MessageContents { get; internal set; }
        public dynamic RecipientId { get; internal set; }
    }

    public class CreatePrivateMessageCommandHandler : IAsyncRequestHandler<CreatePrivateMessageCommand, PrivateMessageViewModel>
    {
        private readonly CoreContext _context;

        public CreatePrivateMessageCommandHandler(CoreContext context)
        {
            _context = context;
        }

        public async Task<PrivateMessageViewModel> Handle(CreatePrivateMessageCommand request)
        {
            var message = new PrivateMessage
            {
                ComposeDate = DateTime.Now,
                GameId = request.GameId,
                ComposerId = request.ComposerId,
                RecipientId = request.RecipientId,
                MessageContents = request.MessageContents
            };
            _context.PrivateMessages.Add(message);
            await _context.SaveChangesAsync();

            var newMessage = _context.PrivateMessages.Where(x => x.PrivateMessageId == message.PrivateMessageId)
                .ProjectTo<PrivateMessageViewModel>().Single();
            
            return newMessage;
        }
    }
}