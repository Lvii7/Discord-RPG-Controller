using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRPGController.models
{
    public class Team
    {
        // the ID of the team itself. useful for separating it from,
        // for example, another team 1 in another battle.
        public int Id { get; set; }
        // the number of the team in battle.
        public required int TeamNumber {  get; set; }
        public int BattleId { get; set; }
        public required Battle Battle { get; set; }

        public List<Combatant> Members { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    }
}
