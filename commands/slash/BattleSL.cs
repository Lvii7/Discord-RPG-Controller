using DiscordRPGController.data;
using DiscordRPGController.models;
using DiscordRPGController.ui;
using DiscordRPGController.utilities;
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

            await ctx.DeferAsync();
            var userId = ctx.User.Id;

            // getting started idk
            var channelId = ctx.Channel.Id.ToString();
            char separator = '|';
            var interactivity = Program.Client.GetInteractivity();

            var options = new DbContextOptionsBuilder<BotDbContext>()
                     .UseSqlite("Data Source=bot.db")
                     .Options;
            using var db = new BotDbContext(options);

            if (await BotUtilities.BattleOngoing(db, channelId))
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"There's already a battle in this channel!"
                ));
                return;
            }

            // checking how many teams there are via checking how many of the team variables are filled in
            int teamCount = 0;
            string[] teamList = { team1, team2, team3, team4 };
            for (int i = 0; i < 4; i++)
            {
                Console.WriteLine($"Team {i + 1} -> {teamList[i]}");
                if (teamList[i].Length > 0)
                {
                    teamCount++;
                }
            }
            ;
            Console.WriteLine($"Team count: {teamCount}");

            // creating a new battle
            var newBattle = new Battle
            {
                ChannelId = ctx.Channel.Id.ToString(),
                TeamCount = teamCount,
                Teams = new List<Team>()
            };

            // list of emojis to add next to the teams to make them cute
            string[] teamEmojiNameList = { ":red_square:", ":blue_square:", ":yellow_square:", ":green_square:" };
            string membersSorted = "";

            try
            {
                for (var i = 0; i < teamCount; i++)
                {
                    var team = new Team
                    {
                        TeamNumber = (i + 1),
                        Battle = newBattle,
                        Members = new List<Combatant>()
                    };

                    List<string> teamMembersList = new List<string>();

                    Console.WriteLine($"New blank Team {i + 1}/{teamCount} has been created!");
                    var teamListSplit = (teamList[i].Split(separator));


                    // -0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-
                    foreach (var member in teamListSplit)
                    {
                        Console.WriteLine($"\"{member}\"");
                        var memberTrimmed = member.Trim(' ').ToLower();
                        Console.WriteLine($"\"{memberTrimmed}\"");
                        var foundPlayer = await db.Players.FirstOrDefaultAsync(p => p.Name.ToLower() == memberTrimmed);

                        if (foundPlayer != null)
                        {
                            Console.WriteLine($"Found character: {foundPlayer.Name}");
                            teamMembersList.Add(foundPlayer.Name);
                            var combatant = new Combatant
                            {
                                PlayerCharacterId = foundPlayer.Id,
                                PlayerCharacter = foundPlayer,
                                Name = foundPlayer.Name,
                                Team = team,
                                ChannelId = channelId,

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
                            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Could not find character **{memberTrimmed.ToUpper()}** from Team {(i + 1)}." +
                                                               $"They were either misspelled, or they don't exist...!"));
                            return;
                        }

                    }
                    // -0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-0-
                    string teamMembers = string.Join(", ", teamMembersList.ToArray());
                    membersSorted = $"{membersSorted}{DiscordEmoji.FromName(Program.Client, teamEmojiNameList[i])} - {teamMembers}\n";
                    newBattle.Teams.Add(team);
                    Console.WriteLine($"throw THis if you write the team {team}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n\n=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=\n\n{ex}\n\n=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
                await ctx.Channel.SendMessageAsync("something went wrong!");
                return;
            }

            var allCombatants = newBattle.Teams
                .SelectMany(t => t.Members)
                .ToList();

            var names = string.Join(" | ", allCombatants.Select(c => c.Name));

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                $"{membersSorted}\nDefine turn order! Use | to separate. Characters will act in the order typed in."
            ));

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

            // sending the embeds to the channel
            var embeds = DisplayBattleMembers.Format(combatants);
            var message = new DiscordMessageBuilder()
                .AddEmbeds(embeds);

            // telling the channel whose turn it is
            var combatantFirstTurn = await db.Combatants
                // SQLite starts sorting from 1. so "1" is the first number
                .Where(c => c.TurnOrder == 1)
                .FirstOrDefaultAsync();

            await ctx.Channel.SendMessageAsync(message);

            await ctx.Channel.SendMessageAsync($"***{combatantFirstTurn.Name}**'s turn!*");

        }


        // TODO: bunch up all the reasons one can't attack into one single method to avoid having to rewrite them all the time
        // like if someone's frozen or dead or out of energy, etc.
        [SlashCommandGroup("attack", "attack")]
        public class AttackCommandsContainer : ApplicationCommandModule
        {
            [SlashCommand("basic", "Not quite just yet")]
            public async Task AttackBasic(InteractionContext ctx,
                [Option("attacker", "Who's attacking? Only accepds one entry")] string attacker,
                [Option("dice-roll", "What were the dice rolls? Separate each of them with /")] string diceRoll,
                [Option("targets", "Who's getting targeted by the attack? Separate each member with /")] string targets)
            {
                Console.WriteLine($"\n= = = = = = = = = = = = = = = = = = = = = = =\n{ctx.User.Username} called \"attack -> solo (slash)\"");
                Console.WriteLine($"ATTACKER: {attacker}\nTARGET(s): {targets}");
                // getting started idk
                await ctx.DeferAsync();
                var userId = ctx.User.Id;
                var channelId = ctx.Channel.Id.ToString();
                var interactivity = Program.Client.GetInteractivity();

                var options = new DbContextOptionsBuilder<BotDbContext>()
                    .UseSqlite("Data Source=bot.db")
                    .Options;
                using var db = new BotDbContext(options);

                if (!await BotUtilities.BattleOngoing(db, channelId))
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        $"There's no battle in this channel!"
                    ));
                    return;
                }

                // validates everyone involved to see if it can proceed
                var membersToValidate = ($"{attacker} | {targets}").Split('|');
                var targetsList = targets.Split('|');

                Console.WriteLine($"\n1.\n'{string.Join("', '", membersToValidate)}'\n'{string.Join("', '", targetsList)}'");

                List<Combatant> hurtCombatantList = new List<Combatant>();

                for (int i = 0; i < membersToValidate.Length; i++)
                {
                    if (i == 0)
                    {
                        attacker = attacker.Trim(' ');
                        Console.WriteLine($"\n2.{1 + i}\n{attacker}");
                    }
                    else
                    {
                        targetsList[(i - 1)] = targetsList[(i - 1)].Trim(' ');
                        Console.WriteLine($"\n2.{1 + i}\n{targetsList[i - 1]}");
                    }
                    membersToValidate[i] = membersToValidate[i].Trim(' ');
                }

                if (!await BotUtilities.CombatantsExist(db, channelId, membersToValidate))
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                        $"One or more names are incorrect!"
                    ));
                    return;
                }

                // put everything else here

                int dice = Int32.Parse(diceRoll);

                List<string> occuranceList = new List<string>();

                var attackerCombatant = await db.Combatants
                    .Include(c => c.Team).ThenInclude(t => t.Battle)
                    .Include(c => c.PlayerCharacter)
                    .FirstOrDefaultAsync(b => b.ChannelId == channelId && b.Name.ToLower() == attacker.ToLower());

                // TODO: replace this with a general method for any reason why the attacker wouldn't be able to act
                if (attackerCombatant != null)
                {
                    if (attackerCombatant.Energy < 2)
                    {
                        var embed = new List<Combatant>();
                        embed.Add(attackerCombatant);
                        var attackerEmbed = DisplayBattleMembers.Format(embed);
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                            .WithContent($"{attackerCombatant.Name} doesn't have enough Energy to attack!")
                            .AddEmbeds(attackerEmbed));
                        return;
                    }

                    attackerCombatant.Energy -= 2;

                    foreach (var member in targetsList)
                    {
                        var target = await db.Combatants
                            .Include(c => c.Team).ThenInclude(t => t.Battle)
                            .FirstOrDefaultAsync(b => b.ChannelId == channelId && b.Name.ToLower() == member.ToLower());

                        if (target != null)
                        {
                            // TODO: remember to prevent the attacker from acting if they're downed

                            int damageDealt = Math.Max(0, dice + attackerCombatant.ATK - target.DEF);
                            occuranceList.Add($"***{target.Name}** took {damageDealt} damage!*");
                            target.HP = Math.Max(0, target.HP - damageDealt);
                            if (target.HP <= 0)
                            {
                                occuranceList.Add($"***{target.Name}** is DOWNED!*");
                            }

                            hurtCombatantList.Add(target);
                            Console.WriteLine($"{target.Name} got attacked and now they have {target.HP}/{target.MaxHP} HP");
                            db.Combatants.Update(target);
                        }
                    }

                    db.Combatants.Update(attackerCombatant);

                    await db.SaveChangesAsync();

                    var hurtCombatants = await db.Combatants
                        .Include(c => c.Team).ThenInclude(t => t.Battle)
                        .Where(c => c.Team.Battle.ChannelId == ctx.Channel.Id.ToString() && targetsList.Contains(c.Name.ToLower()))
                        .OrderBy(c => c.TurnOrder)
                        .ToListAsync();

                    var embeds = DisplayBattleMembers.Format(hurtCombatants);

                    string occuranceString = string.Join("\n", occuranceList);

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent($"***{attackerCombatant.Name}** attacked!!*\n{occuranceString}")
                        .AddEmbeds(embeds));

                    if (attackerCombatant.Energy > 0)
                    {
                        await ctx.Channel.SendMessageAsync($"***{attackerCombatant.Name}** has **{attackerCombatant.Energy}/{attackerCombatant.PlayerCharacter.Energy}** Energy left!*");
                    }
                    else
                    {
                        // TODO: invent a general energyless method to throw

                        await ctx.Channel.SendMessageAsync($"***{attackerCombatant.Name}** has no Energy left!*");

                    }

                }

            }
        }


    }


}
