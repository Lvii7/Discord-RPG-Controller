using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRPGController.models
{
    public class Battle
    {
        // the ID of the battle itself
        public int Id { get; set; }
        public required string ChannelId { get; set; }
        public required int TeamCount { get; set; }

        public List<Team> Teams { get; set; } = new();
        public int CurrentTurn { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    }
}
