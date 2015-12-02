using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShaneSpace.GameSite.Models
{
    public class Game
    {
        public Game()
        {
            Players = new List<GamePlayer>();
            Actions = new List<GameAction>();
            Messages = new List<Message>();
        }

        public int GameId { get; set; }
        public Guid GameGuid { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public int ProgressionMode { get; set; }
        public int? CurrentGamePlayerId { get; set; }
        public int GameType { get; set; }
        public int HostId { get; set; }
        public string Rules { get; set; }
        public string Description { get; set; }

        // navigation
        public virtual User Host { get; set; }
        [InverseProperty("Game")]
        public virtual ICollection<GamePlayer> Players { get; set; }
        public virtual List<GameAction> Actions { get; set; }
        public virtual List<Message> Messages { get; set; }
        [ForeignKey("CurrentGamePlayerId")]
        public virtual GamePlayer CurrentGamePlayer { get; set; }
    }
}
