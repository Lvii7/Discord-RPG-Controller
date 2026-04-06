using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRPGController.commands.slash
{
    public class CharacterCommandsSlash : ApplicationCommandModule
    {
        [SlashCommand("test", "Slash commands! A new gateway... for saving space!")]
        public async Task testSlashCommand(InteractionContext ctx)
        {
            // this sends an empty message so it can be edited later.
            await ctx.DeferAsync();

            int rollingHP = 100;

            var embedMessage = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Chartreuse,
                Description = $"## `[{rollingHP,3}]`"
            };

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedMessage));

            while (rollingHP > 0 )
            {
                rollingHP--;

                var embedMessage2 = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Chartreuse,
                    Description = $"## `[{rollingHP,3}]`"
                };

                await Task.Delay(100);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embedMessage2));

            }

        }


    }
}
