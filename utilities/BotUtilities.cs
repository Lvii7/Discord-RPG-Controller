using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRPGController.utilities
{
    public class BotUtilities
    {
        

        

        
        public class EmojiData
        {
            public List<EmojiInfo> Emojis { get; set; }
        }

        public class EmojiInfo
        {
            public required string Id { get; set; }
            public required string Name { get; set; }
        }

        public async Task SaveEmojisToJSON(ulong guildID)
        {
            var guild = await Program.Client.GetGuildAsync(guildID);
            if (guild != null)
            {
                var emojis = await guild.GetEmojisAsync();
                var emojiList = new List<EmojiInfo>();

                foreach (var emoji in emojis)
                {
                    emojiList.Add(new EmojiInfo
                    {
                        Id = emoji.Id.ToString(),
                        Name = emoji.Name
                    });
                    Console.WriteLine($"EMOJI | name: {emoji.Name} | ID: {emoji.Id}");
                }

                // Save emojis to a JSON file
                var emojiData = new EmojiData { Emojis = emojiList };
                var json = JsonConvert.SerializeObject(emojiData, Formatting.Indented);
                File.WriteAllText("emojis.json", json);

                Console.WriteLine("Emojis saved to JSON file.");
            }
            else
            {
                Console.WriteLine("Guild not found");
            }
        }

        public List<EmojiInfo> LoadEmojisFromJSON()
        {
            // making sure "emojis.json" exists
            if (!File.Exists("emojis.json"))
            {
                Console.WriteLine("Emoji JSON file does not exist.");
                return new List<EmojiInfo>();
            }

            string json = File.ReadAllText("emojis.json");

            try
            {
                var emojiData = JsonConvert.DeserializeObject<EmojiData>(json);
                if (emojiData == null || emojiData.Emojis == null)
                {
                    Console.WriteLine("Failed to parse JSON: Data is null.");
                    return new List<EmojiInfo>();
                }
                else
                {
                    return emojiData.Emojis;
                }
            }
            catch (Exception ex)
            {
                Console.Write($"JSON parsing error: {ex}");
                return new List<EmojiInfo>();
            }
        }

        public EmojiInfo GetEmojiByName(string emojiName)
        {
            var emojis = LoadEmojisFromJSON();

            if (emojis == null || emojis.Count == 0)
            {
                Console.WriteLine($"Emoji list is empty!");
                return null;
            }

            var emoji = emojis.FirstOrDefault(e => e.Name == emojiName);
            if (emoji == null)
            {
                Console.WriteLine($"Emoji {emojiName} not found!");
            }

            return emoji;
        }

        public async Task InitializeEmojis(ulong guildID)
        {
            await SaveEmojisToJSON(guildID);
        }
    }
}
