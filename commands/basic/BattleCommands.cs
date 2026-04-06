using DiscordRPGController.data;
using DiscordRPGController.models;
using DiscordRPGController.ui;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DiscordRPGController.commands.basic
{
    public class BattleCommands : BaseCommandModule
    {

        [Command("battleCancel")]
        public async Task BattleCancel(CommandContext ctx)
        {
            Console.WriteLine($"\n= = = = = = = = = = = = = = = = = = = = = = =\n{ctx.User.Username} called \"battleend\"");
            var channelId = ctx.Channel.Id.ToString();
            var userId = ctx.User.Id;
            var interactivity = Program.Client.GetInteractivity();

            var options = new DbContextOptionsBuilder<BotDbContext>()
                .UseSqlite("Data Source=bot.db")
                .Options;
            using var db = new BotDbContext(options);



            try
            {
                var battle = await db.Battles
                    .Include(b => b.Teams)
                        .ThenInclude(t => t.Members)
                    .FirstOrDefaultAsync(p => p.ChannelId == channelId);

                if (battle == null)
                {
                    Console.WriteLine("No battle found");
                    await ctx.Channel.SendMessageAsync("There isn't a battle happening in this channel!");
                    return;
                }

                Console.WriteLine("Battle found in channel");

                await ctx.Channel.SendMessageAsync("Are you sure you want to end this battle? Progress won't be saved.\n" +
                                                   "Reply \"yes\" to end the battle, anything else will continue it.");
                var response = await interactivity.WaitForMessageAsync(message => message.Author.Id == userId);

                if (response.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync("That took too long...");
                }
                var responseTrimmed = (response.Result.Content).Trim(' ').ToLower();
                if (responseTrimmed == "yes")
                {
                    db.Battles.Remove(battle);

                    await db.SaveChangesAsync();
                    await ctx.Channel.SendMessageAsync("battle deleted!");
                    Console.WriteLine($"Battle {battle} deleted successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex}");
                await ctx.Channel.SendMessageAsync("something exploded check console");
            }
        }


        [Command("battleShow")]
        public async Task BattleShow(CommandContext ctx)
        {
            Console.WriteLine($"\n= = = = = = = = = = = = = = = = = = = = = = =\n{ctx.User.Username} called \"battleend\"");
            var channelId = ctx.Channel.Id.ToString();
            var userId = ctx.User.Id;
            var interactivity = Program.Client.GetInteractivity();

            var options = new DbContextOptionsBuilder<BotDbContext>()
                .UseSqlite("Data Source=bot.db")
                .Options;
            using var db = new BotDbContext(options);

            try
            {
                var battle = await db.Battles
                    .Include(b => b.Teams)
                        .ThenInclude(t => t.Members)
                    .FirstOrDefaultAsync(p => p.ChannelId == channelId);

                if (battle == null)
                {
                    Console.WriteLine("No battle found");
                    await ctx.Channel.SendMessageAsync("There isn't a battle happening in this channel!");
                    return;
                }

                Console.WriteLine("Battle found in channel");

                var combatants = await db.Combatants
                    .Where(c => c.Team.Battle.ChannelId == ctx.Channel.Id.ToString())
                    .OrderBy(c => c.TurnOrder)
                    .ToListAsync();

                var embeds = DisplayBattleMembers.Format(combatants);
                var message = new DiscordMessageBuilder()
                    .AddEmbeds(embeds);
                await ctx.Channel.SendMessageAsync(message);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex}");
                await ctx.Channel.SendMessageAsync("something exploded check console");
            }
        }



    }
}