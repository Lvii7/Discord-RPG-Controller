using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordRPGController.models;

namespace DiscordRPGController.data
{
    public class BotDbContext : DbContext
    {

        public BotDbContext(DbContextOptions<BotDbContext> options) : base(options) { }

        public DbSet<PlayerCharacter> Players => Set<PlayerCharacter>();
        public DbSet<Battle> Battles => Set<Battle>();
        public DbSet<Team> Teams => Set<Team>();
        public DbSet<Combatant> Combatants => Set<Combatant>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Battle>()
                .HasMany(b => b.Teams)
                .WithOne(t => t.Battle)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Team>()
                .HasMany(t => t.Members)
                .WithOne(c => c.Team)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
