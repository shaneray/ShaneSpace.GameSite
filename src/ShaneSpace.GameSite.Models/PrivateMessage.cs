using System;

namespace ShaneSpace.GameSite.Models
{
    public class PrivateMessage
    {
        public int PrivateMessageId { get; set; }
        public int ComposerId { get; set; }
        public int RecipientId { get; set; }
        public int GameId { get; set; }
        public string MessageContents { get; set; }
        public DateTime ComposeDate { get; set; }

        // navigation
        public virtual User Composer { get; set; }
        public virtual User Recipient { get; set; }
    }
}
