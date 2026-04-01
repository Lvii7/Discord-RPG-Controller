using DiscordRPGController.data;
using DiscordRPGController.models;
using DiscordRPGController.ui;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace DiscordRPGController.commands.slash
{
    [SlashCommandGroup("battle", "Commands relating to battles i guess idk")]
    public class BattleCommandsContainer : ApplicationCommandModule
    {
        [SlashCommand("startTeam", "Not quite just yet")]
        public async Task SlashCommandParam(InteractionContext ctx,
            [Option("team1", "Who are the members of the first team? Separate each member with |")] string team1,
            [Option("team2", "Who are the members of the second team? Separate each member with |")] string team2,
            [Option("team3", "(OPTIONAL) Who are the members of the third team? Separate each member with |")] string team3 = "",
            [Option("team4", "(OPTIONAL) Who are the members of the fourth team? Separate each member with |")] string team4 = "")
        {
            Console.WriteLine($"\n= = = = = = = = = = = = = = = = = = = = = = =\n{ctx.User.Username} called \"battle -> startTeam (slash)\"");

            /*
            the idea here is to have all 4 teams as separate parameters
            instead of messages the user will fill them out all at once
            team count is no longer necessary
            the code goes through the lists of teams and checks if they're all valid
            if so then the battle is created
            */

            await ctx.DeferAsync();
            var userId = ctx.User.Id;

            // getting started idk
            var channelId = ctx.Channel.Id.ToString();
            char separator = '|';
            var interactivity = Program.Client.GetInteractivity();

            // accesses the database. everything fine by here. live commenting lvii
            var options = new DbContextOptionsBuilder<BotDbContext>()
                .UseSqlite("Data Source=bot.db")
                .Options;
            using var db = new BotDbContext(options);

            var existing = await db.Battles.FirstOrDefaultAsync(p => p.ChannelId == channelId);

            // checks if there's already a battle on the channel
            if (existing != null)
            {
                Console.WriteLine($"{ctx.User.Username} character {channelId} already exist");
                await ctx.Channel.SendMessageAsync("There's already an ongoing battle in this channel!");
                return;
            }

            // checking how many teams there are via checking how many of the team variables are filled in
            int teamCount = 0;
            string[] teamList = { team1, team2, team3, team4 };
            for (int i = 0; i < 4; i++)
            {
                Console.WriteLine($"Team {i+1} -> {teamList[i]}");
                if (teamList[i].Length > 0)
                {
                    teamCount++;
                }
            };
            Console.WriteLine($"Team count: {teamCount}");

            // creating a new battle
            var newBattle = new Battle
            {
                ChannelId = ctx.Channel.Id.ToString(),
                TeamCount = teamCount,
                Teams = new List<Team>()
            };

            try
            {
                for (var i = 0; i < teamCount; i++)
                {
                    var team = new Team
                    {
                        TeamNumber = (i+1),
                        Battle = newBattle,
                        Members = new List<Combatant>()
                    };
                    Console.WriteLine($"New blank Team {i+1}/{teamCount} has been created!");
                    var teamListSplit = (teamList[i].Split(separator));

                    foreach (var member in teamListSplit)
                    {
                        Console.WriteLine($"\"{member}\"");
                        var memberTrimmed = member.Trim(' ').ToLower();
                        Console.WriteLine($"\"{memberTrimmed}\"");
                        var foundPlayer = await db.Players.FirstOrDefaultAsync(p => p.Name.ToLower() == memberTrimmed);

                        if (foundPlayer != null)
                        {
                            Console.WriteLine($"Found character: {foundPlayer.Name}");
                            var combatant = new Combatant
                            {
                                PlayerCharacterId = foundPlayer.Id,
                                PlayerCharacter = foundPlayer,
                                Name = foundPlayer.Name,
                                Team = team,

                                HP = foundPlayer.HP,
                                MaxHP = foundPlayer.MaxHP,
                                SP = foundPlayer.SP,
                                MaxSP = foundPlayer.MaxSP,
                                Energy = foundPlayer.Energy,

                                ATK = foundPlayer.ATK,
                                DEF = foundPlayer.DEF,
                                Dice = foundPlayer.Dice
                            };

                            team.Members.Add(combatant);
                        }
                        else
                        {
                            Console.WriteLine($"something went wrong");
                            await ctx.Channel.SendMessageAsync($"Could not find character **{memberTrimmed.ToUpper()}** from Team {(i+1)}." +
                                                               $"They were either misspelled, or they don't exist...!");
                            return;
                        }

                    }
                    newBattle.Teams.Add(team);
                    Console.WriteLine($"throw THis if you write the team {team}");
                }
            } catch (Exception ex)
            {
                Console.WriteLine($"\n\n=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=\n\n{ex}\n\n=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
                await ctx.Channel.SendMessageAsync("something went wrong!");
            }

            var allCombatants = newBattle.Teams
                .SelectMany(t => t.Members)
                .ToList();

            var names = string.Join(" | ", allCombatants.Select(c => c.Name));

            // TODO: skip this part if there's only one team
            await ctx.Channel.SendMessageAsync(
                $"Define turn order:\n{names}\n\nUse | to separate."
            );

            var orderResponse = await interactivity.WaitForMessageAsync(
                m => m.Author.Id == userId
            );

            var orderList = orderResponse.Result.Content
                .Split(separator)
                .Select(x => x.Trim().ToLower())
                .ToList();

            if (orderList.Count != allCombatants.Count)
            {
                await ctx.Channel.SendMessageAsync("Missing or extra names!");
                return;
            }

            for (int i = 0; i < orderList.Count; i++)
            {
                var name = orderList[i];

                var combatant = allCombatants
                    .FirstOrDefault(c => c.Name.ToLower() == name);

                if (combatant == null)
                {
                    await ctx.Channel.SendMessageAsync($"Invalid name: {name}");
                    return;
                }

                combatant.TurnOrder = i + 1;
            }

            // after the for loop

            Console.WriteLine($"Teams count: {newBattle.Teams.Count}");
            Console.WriteLine($"Combatants total: {newBattle.Teams.SelectMany(t => t.Members).Count()}");

            db.Battles.Add(newBattle);
            await db.SaveChangesAsync();

            // this takes all of the combatants and sorts them via turn order
            var combatants = await db.Combatants
                .Where(c => c.Team.BattleId == newBattle.Id)
                .OrderBy(c => c.TurnOrder)
                .ToListAsync();

            var embeds = DisplayBattleMembers.Format(combatants);
            await ctx.Channel.SendMessageAsync(embeds);

            await ctx.Channel.SendMessageAsync("battle ready baby");



            /*var textMessage = result;

            var embedMessage = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Gold,
                Description = $"# {result}!"
            };

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{textMessage}\n\n*Now, say something!*"));

            for (int i = 0; i < 4; i++)
            {
                var response1 = await ctx.Channel.GetNextMessageAsync(message => message.Author.Id == userId);
                if (response1.TimedOut)
                {
                    await ctx.Channel.SendMessageAsync("youuuu took 2 long!");
                }
                ;
                var responseAdd = $"{(i + 1)} - {response1.Result.Content}";
                textMessage = $"{textMessage}\n{responseAdd}";
                if (i == 3)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{textMessage}"));
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{textMessage}\n\n*{(3 - i)} more time(s)!*"));
                }
                ;
            }*/
        }
    }
}
