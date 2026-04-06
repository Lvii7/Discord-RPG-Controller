using DSharpPlus.Entities;
using DiscordRPGController;
using DiscordRPGController.utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordRPGController.models;

namespace DiscordRPGController.ui
{
    public static class CharacterUI
    {
        public static DiscordEmbed CharacterProfile(PlayerCharacter player)
        {
            
            int statbarWidth = 10;
            double HPblocks = Math.Round((Convert.ToDouble(player.HP) / Convert.ToDouble(player.MaxHP)) * statbarWidth * 2, 0);
            double SPblocks = Math.Round((Convert.ToDouble(player.SP) / Convert.ToDouble(player.MaxSP)) * statbarWidth * 2, 0);
            Console.WriteLine($"HP%: {HPblocks} | SP%: {SPblocks}");

            var emojiList = new List<DiscordEmoji>();
            Console.WriteLine("New empty emoji list created.");

            int listSize = statbarWidth;
            for (int i = 0; i < statbarWidth; i++)
            {
                int emojiHP = 0;
                int emojiSP = 0;

                switch (HPblocks)
                {
                    case >= 2:
                        emojiHP = 2;
                        HPblocks -= 2;
                        break;
                    case 1:
                        emojiHP = 1;
                        HPblocks = 1;
                        break;
                    default:
                        break;
                }

                switch (SPblocks)
                {
                    case >= 2:
                        emojiSP = 2;
                        SPblocks -= 2;
                        break;
                    case 1:
                        emojiSP = 1;
                        SPblocks = 1;
                        break;
                    default:
                        break;
                }

                string emojiName = $"hp{emojiHP}sp{emojiSP}";

                BotUtilities botUtils = new BotUtilities();
                var emojiInfo = botUtils.GetEmojiByName(emojiName);
                emojiName = $":{emojiName}:";

                if (emojiInfo != null)
                {
                    var emoji = DiscordEmoji.FromGuildEmote(Program.Client, ulong.Parse(emojiInfo.Id));
                    emojiList.Add(emoji);
                }
                else
                {
                    Console.WriteLine($"Emoji not found -- replacing with :white_circle:.");
                    var emojiDefault = DiscordEmoji.FromName(Program.Client, ":white_circle:");
                    emojiList.Add(emojiDefault);
                    Console.WriteLine($"Emoji appended to Emoji list.");
                }

            }

            Console.WriteLine($"Emoji list finalized successfully.");

            string emojiString = string.Join("", emojiList.Select(e => e.ToString()));
   
            var embed = new DiscordEmbedBuilder
            {
                Description = $"**`{player.Name}`**\n" +
                              $"**`Level {player.Level}\n`**" +
                              $"`HP: {player.HP, 3}/{player.MaxHP, 3} | SP: {player.SP, 3}/{player.MaxSP, 3}`\n"+
                              $"{emojiString}\n"+ 
                              $"`ATK: {player.ATK} | DEF: {player.DEF} | Dice: {player.Dice,2} | Energy: {player.Energy}`\n\n" +
                              $"`AP: {player.AP,3}/{player.MaxAP,3} | FP: {player.FP,3}/{player.MaxFP,3}`\n" +
                              $"`Cash: {player.Money}`",
                Color = DiscordColor.White
            };
            return embed;
        }

        public static DiscordEmbed BattleProfile(Combatant combatant)
        {

            int statbarWidth = 10;
            double HPblocks = Math.Round((Convert.ToDouble(combatant.HP) / Convert.ToDouble(combatant.MaxHP)) * statbarWidth * 2, 0);
            double SPblocks = Math.Round((Convert.ToDouble(combatant.SP) / Convert.ToDouble(combatant.MaxSP)) * statbarWidth * 2, 0);
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

                BotUtilities botUtils = new BotUtilities();
                var emojiInfo = botUtils.GetEmojiByName(emojiName);
                emojiName = $":{emojiName}:";

                if (emojiInfo != null)
                {
                    var emoji = DiscordEmoji.FromGuildEmote(Program.Client, ulong.Parse(emojiInfo.Id));
                    emojiList.Add(emoji);
                }
                else
                {
                    Console.WriteLine($"Emoji not found -- replacing with :white_circle:.");
                    var emojiDefault = DiscordEmoji.FromName(Program.Client, ":white_circle:");
                    emojiList.Add(emojiDefault);
                    Console.WriteLine($"Emoji appended to Emoji list.");
                }

            }

            Console.WriteLine($"Emoji list finalized successfully.");

            string emojiString = string.Join("", emojiList.Select(e => e.ToString()));

            var setEmbedColor = DiscordColor.White;
            if (combatant.Team.Battle.TeamCount > 1)
            {
                switch (combatant.Team.TeamNumber)
                {
                    case 1:
                        setEmbedColor = DiscordColor.Red;
                        break;
                    case 2:
                        setEmbedColor = DiscordColor.Blue;
                        break;
                    case 3:
                        setEmbedColor = DiscordColor.Yellow;
                        break;
                    case 4:
                        setEmbedColor = DiscordColor.Green;
                        break;
                }
            }
            var embed = new DiscordEmbedBuilder
            {
                Description = $"**`{combatant.Name}`**\n" +
                              $"`HP: {combatant.HP,3}/{combatant.MaxHP,3} | SP: {combatant.SP,3}/{combatant.MaxSP,3}`\n" +
                              $"{emojiString}\n" +
                              $"`ATK: {combatant.ATK} | DEF: {combatant.DEF} | Dice: {combatant.Dice,2} | Energy: {combatant.Energy}`",

                Color = setEmbedColor
            };
            return embed;
        }

    }
}
