using DiscordRPGController.config;
using DiscordRPGController.data;
using DiscordRPGController.models;
using DiscordRPGController.utilities;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Google.Apis.Sheets.v4;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DiscordRPGController.ui
{
    public class DisplayBattleMembers
    {
       public static DiscordMessageBuilder Format(List<Combatant> combatants)
        {
            var message = new DiscordMessageBuilder();

            foreach (var member in combatants)
            {
                var embed = CharacterUI.BattleProfile(member);
                message.AddEmbed(embed);
            }

            return message;
        }

    }
}
