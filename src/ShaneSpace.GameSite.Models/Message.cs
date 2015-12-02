using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShaneSpace.GameSite.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public int ComposerId { get; set; }
        public int GameId { get; set; }
        public string MessageContents { get; set; }
        public DateTime ComposeDate { get; set; }

        // navigation
        [ForeignKey("GameId")]
        public virtual Game Game { get; set; }
        [ForeignKey("ComposerId")]
        public virtual User Composer { get; set; }
    }
}
