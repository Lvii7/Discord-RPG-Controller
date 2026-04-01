using DiscordRPGController.config;
using DiscordRPGController.data;
using DiscordRPGController.models;
using DiscordRPGController.ui;
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

namespace DiscordRPGController.commands.basic
{
    public class BattleCommands : BaseCommandModule
    {


        [Command("battlestart")]
        public async Task BattleStart(CommandContext ctx)
        {
            Console.WriteLine($"\n= = = = = = = = = = = = = = = = = = = = = = =\n{ctx.User.Username} called \"battlestart\"");
            // getting started idk
            ulong userID = ctx.User.Id;
            var channelId = ctx.Channel.Id.ToString();
            char separator = '|';
            var interactivity = Program.Client.GetInteractivity();

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

            // now asking for amount of teams. "1" results on a free for all.
            await ctx.Channel.SendMessageAsync("How many teams?");

            var teamCountMessage = await interactivity.WaitForMessageAsync(message => message.Author.Id == userID);
            var teamCountToParse = teamCountMessage.Result.Content;

            char[] charsToTrim = { ' ', '*', '_', '\\', '-', '.' };
            char[] spaceToTrim = { ' ' };
            teamCountToParse = teamCountToParse[0].ToString();

            if (Int32.TryParse((teamCountToParse.Trim(charsToTrim)), out int teams))
            {
                int teamCount = Math.Clamp(teams, 1, 4);
                string[] combatantList = [];

                var newBattle = new Battle
                {
                    ChannelId = ctx.Channel.Id.ToString(),
                    TeamCount = teamCount,
                    Teams = new List<Team>()
                };

                for (var i = 1; i <= teamCount; i++)
                {
                    if (teamCount == 1)
                    {
                        var teamPrompt = await ctx.Channel.SendMessageAsync($"Please specify the members of this battle!\n" +
                                                                        $"Separate each member of the team with a **|**\n" +
                                                                        $"-# Reply \"cancel\" to cancel at any time.");
                    }
                    else
                    {
                        var teamPrompt = await ctx.Channel.SendMessageAsync($"Please specify the members of Team {i}!\n" +
                                                                        $"Separate each member of the team with a **|**\n" +
                                                                        $"-# Reply \"cancel\" to cancel at any time.");
                    }
                    var teamResponse = await interactivity.WaitForMessageAsync(message => message.Author.Id == userID);

                    //
                    if (teamResponse.Result.Content.ToLower() == "cancel")
                    {
                        await ctx.Channel.SendMessageAsync($"Battle canceled!");
                        return;
                    }

                    //
                    var team = new Team
                    {
                        TeamNumber = i,
                        Battle = newBattle,
                        Members = new List<Combatant>()
                    };
                    var teamListSplit = (teamResponse.Result.Content.Split(separator));

                    foreach (var member in teamListSplit)
                    {
                        var memberTrimmed = member.Trim(spaceToTrim).ToLower();

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
                            await ctx.Channel.SendMessageAsync($"Could not find character(s): **{memberTrimmed.ToUpper()}**. They were either misspelled, or they don't exist...!");
                            return;
                        }

                    }
                    newBattle.Teams.Add(team);
                    Console.WriteLine($"throw THis if you write the team {team}");
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
                    m => m.Author.Id == userID
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
            }
            // if team count isn't an integer
            else
            {
                await ctx.Channel.SendMessageAsync("that's probably not a valid number.");
                return;
            }
        }

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
                await ctx.Channel.SendMessageAsync(embeds);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex}");
                await ctx.Channel.SendMessageAsync("something exploded check console");
            }
        }



        //[Command("damage")]
        //public async Task DealDamage(CommandContext ctx)
        //{
        //char[] charsToTrim = { ' ', '*', '_', '\\' };
        //charname = charname.Trim(charsToTrim);
        //var interactivity = Program.Client.GetInteractivity();
        //var sheetService = new GoogleSheetsService();
        //var sheetService = new GoogleSheetsService();
        //var charnameLower = charname.ToLower();
        //var existingPlayers = sheetService.ReadSheet("Players!A3:A");

        // how it's gonna work:
        // 1- checks if there's a battle going on in the current channel, if not, then end command
        // 2- tries to check if mentioned character(s) are in the battle
        // 3- decrease character HP by value mentioned 
        // 4- update google sheet to match
        // 5- show profile of character(s) damaged
        // 6- if KO'd, display extra message
        //}

        /*
        [Command("battleend")]
        public async Task BattleEnd(CommandContext ctx)
        {
            // make sure to later ignore the command if it wasn't sent by the battle host
            ulong userID = ctx.User.Id;
            var interactivity = Program.Client.GetInteractivity();

            var sheetService = new GoogleSheetsService();
            var battleList = await sheetService.ReadSheetAsync("BattleList!A2:B");

            await ctx.Channel.SendMessageAsync("Are you sure you want to end this battle? **It won't save.**\n *Reply 'confirm' to confirm, or 'cancel' to cancel.*");
            var response = await interactivity.WaitForMessageAsync(message => message.Author.Id == userID);

            if (response.TimedOut == false)
            {
                if (response.Result.Content.ToLower().StartsWith("confirm"))
                {
                    await ctx.Channel.SendMessageAsync("ok");
                }
            } else
            {
                await ctx.Channel.SendMessageAsync("that took too long...............");
            }


            

                //yes
                //var battleIndex = await GetRowIndex("BattleList!A2:B", ctx.Channel.Id.ToString(), 1);

            //no
            //await ctx.Channel.SendMessageAsync("ok i wont");
            
        }
        */

        /*
        public async Task<List<string>> GetTeamMembers(CommandContext ctx, List<IList<object>> existingPlayers, string teamName)
        {
            var sheetServices = new GoogleSheetsService();
            ulong userID = ctx.User.Id;
            var interactivity = Program.Client.GetInteractivity();
            char separator = '|';

            // Get the members of the team
            var teamPrompt = await ctx.Channel.SendMessageAsync($"Please specify the members of Team {teamName}!\nSeparate each member of the team with a **|**\n-# Reply \"cancel\" to cancel at any time.");
            var teamResponse = await interactivity.WaitForMessageAsync(message => message.Author.Id == userID);

           
        } */


        /*
        public static DiscordEmbed CharacterBattleInfo(List<IList<object>> existingPlayers, string character)
        {
            BotUtilities botUtils = new BotUtilities();
            var sheetService = new GoogleSheetsService();
    
            var row = existingPlayers.Find(e =>
                string.Equals(e[0]?.ToString(), character, StringComparison.OrdinalIgnoreCase));

            var charInfo = (existingPlayers.Find(e => e.ElementAt(0)?.ToString().ToLower() == character.ToLower())).ToList();

            // indexes 2 and 3 of charInfo hold the HP and HP max variables respectively, indexes 4 and 5 hold SP and SP max
            int statbarWidth = 10;
            double HPblocks = Math.Round((Convert.ToDouble(charInfo[2]) / Convert.ToDouble(charInfo[3])) * statbarWidth * 2, 0);
            double SPblocks = Math.Round((Convert.ToDouble(charInfo[4]) / Convert.ToDouble(charInfo[5])) * statbarWidth * 2, 0);
            Console.WriteLine($"HP%: {HPblocks} | SP%: {SPblocks}");

            var emojiList = new List<DiscordEmoji>();
            Console.WriteLine("New empty emoji list created.");

            int listSize = statbarWidth;
            for (int i = 0; i < statbarWidth; i++)
            {
                int emojiHP = 0;
                int emojiSP = 0;
                if (HPblocks >= 2)
                {
                    emojiHP = 2;
                    HPblocks -= 2;
                }
                else
                {
                    if (HPblocks == 1)
                    {
                        emojiHP = 1;
                        HPblocks = 0;
                    }
                }
                if (SPblocks >= 2)
                {
                    emojiSP = 2;
                    SPblocks -= 2;
                }
                else
                {
                    if (SPblocks == 1)
                    {
                        emojiSP = 1;
                        SPblocks = 0;
                    }
                }

                string emojiName = $"hp{emojiHP}sp{emojiSP}";

                var emojiInfo = botUtils.GetEmojiByName(emojiName);
                emojiName = $":{emojiName}:";

                if (emojiInfo != null)
                {
                    var emoji = DiscordEmoji.FromGuildEmote(Program.Client, ulong.Parse(emojiInfo.ID));
                    emojiList.Add(emoji);
                }
                else
                {
                    Console.WriteLine($"Emoji not found -- replacing with :white_circle:.");
                    var emojiDefault = DiscordEmoji.FromName(Program.Client, ":white_circle:");
                    emojiList.Add(emojiDefault);
                }

            }
            string iconEmojiName = charInfo[0].ToString();
            var iconEmojiInfo = botUtils.GetEmojiByName(iconEmojiName);
            iconEmojiName = $":{iconEmojiName}:";

            var iconEmoji = DiscordEmoji.FromName(Program.Client, ":white_circle:");

            if (iconEmojiInfo != null)
            {
                var emoji = DiscordEmoji.FromGuildEmote(Program.Client, ulong.Parse(iconEmojiInfo.ID));
                iconEmoji = emoji;
            }
            else
            {
                Console.WriteLine($"Emoji not found.");
            }

            Console.WriteLine($"Emoji list finalized successfully.");

            string emojiString = string.Join("", emojiList.Select(e => e.ToString()));

            // Changes color of embed depending on team
            var color = new DiscordColor();
            int teamNumber = int.Parse((string)charInfo[1]);
            switch (teamNumber)
            {
                case 1:
                    color = DiscordColor.Blue;
                    break;

                case 2:
                    color = DiscordColor.Red;
                    break;

                case 3:
                    color = DiscordColor.Yellow;
                    break;

                case 4:
                    color = DiscordColor.Green;
                    break;

                default:
                    color = DiscordColor.White;
                    break;
            }

            var embed = new DiscordEmbedBuilder
            {
                Description = $"## **{iconEmoji} {charInfo[0]}**\n__HP: {charInfo[2]}__/{charInfo[3]} | __SP: {charInfo[4]}__/{charInfo[5]}\nATK: **{charInfo[7]}** | DEF: **{charInfo[8]}** | Energy: **{charInfo[6]}**\n{emojiString}",
                Color = color
            };
            return embed;
        } */
        /*
        public async Task<int> GetEarliestEmptyRow(string range)
        {
            var sheetService = new GoogleSheetsService();
            var existingRows = await sheetService.ReadSheetAsync(range);
            for (int i = 0; i < existingRows.Count(); i++)
            {
                var row = existingRows[i];
                if ( row.All( cell => string.IsNullOrEmpty( cell?.ToString() ) ) )
                {
                    // idk the sheets start at Battle1 so
                    return i + 1;
                }
            }

            return existingRows.Count + 1;
            // if no empty row, next one should be after the last
        }

        public async Task<int> GetRowIndex(string range, string value, int index)
        {
            var sheetService = new GoogleSheetsService();
            var existingRows = await sheetService.ReadSheetAsync(range);

            var foundPlayer = existingRows.Find(e => e.ElementAt(index)?.ToString().ToLower() == value.ToLower());
            string foundPlayerString = string.Join(" | ", foundPlayer);
            Console.WriteLine($"foundPlayerString: {foundPlayerString}");

            for (int i = 0; i < existingRows.Count(); i++)
            {
                var row = existingRows[i];
                if (row[index].ToString().ToLower() == value.ToString().ToLower())
                {
                    return i + 1;
                }
            }
            return -1;            
        } */
    }
}