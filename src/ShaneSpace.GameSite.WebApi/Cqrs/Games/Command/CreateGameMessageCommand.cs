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
    public class CreateGameMessageCommand : IAsyncRequest<MessageViewModel>
    {
        public int ComposerId { get; internal set; }
        public int GameId { get; internal set; }
        public string MessageContents { get; internal set; }
    }

    public class CreateGameMessageCommandHandler : IAsyncRequestHandler<CreateGameMessageCommand, MessageViewModel>
    {
        private readonly CoreContext _context;

        public CreateGameMessageCommandHandler(CoreContext context)
        {
            _context = context;
        }

        public async Task<MessageViewModel> Handle(CreateGameMessageCommand request)
        {
            var game = _context.Games.Single(x => x.GameId == request.GameId);
            var message = new Message
            {
                ComposeDate = DateTime.Now,
                GameId = request.GameId,
                ComposerId = request.ComposerId,
                MessageContents = request.MessageContents
            };
            game.Messages.Add(message);
            await _context.SaveChangesAsync();

            var newMessage = _context.Messages.Where(x => x.MessageId == message.MessageId)
                .ProjectTo<MessageViewModel>().Single();
            
            return newMessage;
        }
    }
}