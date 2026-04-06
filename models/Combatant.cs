using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRPGController.models
{
    // player characters or enemies when they're in battle
    public class Combatant
    {
        // borrowed from the PlayerCharacter class
        public int Id { get; set; }
        public int? PlayerCharacterId { get; set; } // nullable
        public PlayerCharacter? PlayerCharacter { get; set; }
        public int TurnOrder {  get; set; }
        public int TeamId { get; set; }

        public string? ChannelId { get; set; }
        public required Team Team { get; set; }

        public required string Name { get; set; }

        public int HP { get; set; } 
        public int MaxHP { get; set; } 
        public int SP { get; set; } 
        public int MaxSP { get; set; }
        public int Energy { get; set; }

        public int ATK { get; set; } 
        public int DEF { get; set; } 
        public int Dice { get; set; }
        //public string[] statuses { get; set; } = [];
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    }
}
