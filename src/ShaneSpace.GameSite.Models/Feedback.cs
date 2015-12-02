using System.ComponentModel.DataAnnotations.Schema;

namespace ShaneSpace.GameSite.Models
{
    [Table("feedback")]
    public class Feedback
    {
        public int FeedbackId { get; set; }
        public int UserId { get; set; }
        public int FeedbackType { get; set; }
        public string FeedbackText { get; set; }

        public virtual User User { get; set; }
    }
}
