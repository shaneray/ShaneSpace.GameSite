using ShaneSpace.GameSite.Domain.Mapping;
using ShaneSpace.GameSite.Models;
using ShaneSpace.GameSite.WebApi.ViewModels.User;
using System;

namespace ShaneSpace.GameSite.WebApi.ViewModels
{
    public class MessageViewModel : IMapFrom<Message>
    {
        public DateTime ComposeDate { get; set; }
        public UserSummaryViewModel Composer { get; set; }
        public string MessageContents { get; set; }
    }

    public class PrivateMessageViewModel : IMapFrom<PrivateMessage>
    {
        public DateTime ComposeDate { get; set; }
        public UserSummaryViewModel Composer { get; set; }
        public UserSummaryViewModel Recipient { get; set; }
        public string MessageContents { get; set; }
    }
}