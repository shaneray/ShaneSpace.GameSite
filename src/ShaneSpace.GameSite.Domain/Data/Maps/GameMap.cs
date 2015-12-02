using ShaneSpace.GameSite.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace ShaneSpace.GameSite.Domain.Data.Maps
{
    public class GameMap : EntityTypeConfiguration<Game>
    {
        public GameMap()
        {
            HasKey(t => t.GameId);

            ToTable("games");
            Property(t => t.GameId).HasColumnName("GameId").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(t => t.GameGuid).HasColumnName("GameGuid");
            Property(t => t.GameType).HasColumnName("GameType");
            Property(t => t.HostId).HasColumnName("HostId");
            Property(t => t.Rules).HasColumnName("Rules");
            Property(t => t.Description).HasColumnName("Description");
        }
    }
}
