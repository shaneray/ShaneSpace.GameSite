using System.ComponentModel.DataAnnotations.Schema;

namespace ShaneSpace.GameSite.Models
{
    public class GamePlayer
    {
        public int GamePlayerId { get; set; }
        public int GameId { get; set; }
        public int UserId { get; set; }

        // navigation
        public virtual User User { get; set; }
        [ForeignKey("GameId")]
        public virtual Game Game { get; set; }
    }
}