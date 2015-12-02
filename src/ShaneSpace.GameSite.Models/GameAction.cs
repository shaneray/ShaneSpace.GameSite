using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShaneSpace.GameSite.Models
{
    public class GameAction
    {
        public int GameActionId { get; set; }
        public int GameId { get; set; }
        public DateTime DateTime { get; set; }
        public int UserId { get; set; }
        public int Action { get; set; }
        public string ActionValue { get; set; }

        // Navigation
        [ForeignKey("GameId")]
        public virtual Game Game { get; set; }
        public virtual User User { get; set; }
    }
}