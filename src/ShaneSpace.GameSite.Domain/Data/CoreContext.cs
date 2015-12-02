using ShaneSpace.GameSite.Models;
using System.Data.Entity;
using System.Diagnostics;

namespace ShaneSpace.GameSite.Domain.Data
{
    public class CoreContext : DbContext
    {
        public CoreContext() : base(nameOrConnectionString: "ShaneSpaceGameSite") { }

        public DbSet<Game> Games { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<GamePlayer> GamePlayers { get; set; }
        public DbSet<GameAction> GameActions { get; set; }
        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<PrivateMessage> PrivateMessages { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (Debugger.IsAttached)
            {
                Database.Log = s => Debug.Write(s);
            }
            base.OnModelCreating(modelBuilder);
        }
    }
}