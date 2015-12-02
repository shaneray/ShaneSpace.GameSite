using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShaneSpace.GameSite.Models
{
    public class User
    {
        public int Id { get; set; }
        public int AuthId { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }

        [InverseProperty("Recipient")]
        public virtual List<PrivateMessage> RecievedMessages { get; set; }
        [InverseProperty("Composer")]
        public virtual List<PrivateMessage> ComposedMessages { get; set; }
    }
}