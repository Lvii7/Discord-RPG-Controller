using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRPGController.models
{
    public class PlayerCharacter
    {
        // the ID of the character itself
        public int Id { get; set; }
        // the ID of the character's user (can repeat, obviously)
        public required string UserId { get; set; }
        public required string Name { get; set; }

        public int Level { get; set; }
        public int Money { get; set; } = 0;

        public int HP { get; set; } = 5;
        public int MaxHP { get; set; } = 5;
        public int SP { get; set; } = 5;
        public int MaxSP { get; set; } = 5;
        public int AP { get; set; } = 3;
        public int MaxAP { get; set; } = 3;
        public int FP { get; set; } = 0;
        public int MaxFP { get; set; } = 0;
        public int Energy { get; set; } = 5;

        public int ATK { get; set; } = 0;
        public int DEF { get; set; } = 0;
        public int Dice { get; set; } = 2;
        public bool IsInBattle { get; set; } = false;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    }
}
