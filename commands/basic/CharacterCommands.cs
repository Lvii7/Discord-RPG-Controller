using DiscordRPGController.config;
using DiscordRPGController.data;
using DiscordRPGController.models;
using DiscordRPGController.ui;
using DiscordRPGController.utilities;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
//using System.Data.Entity.DbContext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace DiscordRPGController.commands.basic
{
    public class CharacterCommands : BaseCommandModule
    {

        [Command("createCharacter")]
        public async Task CreateCharacter(CommandContext ctx, [RemainingText] string charname)
        {

            char[] charsToTrim = { ' ', '*', '_', '\\' };
            charname = charname.Trim(charsToTrim);
            Console.WriteLine($"{ctx.User.Username} create character {charname}");
            if (charname.Length < 2)
            {
                await ctx.Channel.SendMessageAsync("But that's too short a name!\n-# Try something at least two characters long!");
                Console.WriteLine($"{ctx.User.Username} character {charname} name too short");
                return;
            }
            Console.WriteLine($"{ctx.User.Username} character {charname} name LONG ENOUGH!");

            // Checks if the character already exists
            try
            {
                var options = new DbContextOptionsBuilder<BotDbContext>()
                    .UseSqlite("Data Source=bot.db")
                    .Options;
                using var db = new BotDbContext(options);

                var existing = await db.Players.FirstOrDefaultAsync(p => p.Name == charname);

                if (existing != null)
                {
                    Console.WriteLine($"{ctx.User.Username} character {charname} already exist");
                    await ctx.Channel.SendMessageAsync("There's already a character with that name!");
                    return;
                }

                Console.WriteLine($"{ctx.User.Username} character {charname} name UNIQUE!");

                var newPlayer = new PlayerCharacter
                {
                    UserId = ctx.User.Id.ToString(),
                    Name = charname,
                    Level = 1,
                    IsInBattle = false,
                };

                db.Players.Add(newPlayer);
                await db.SaveChangesAsync();

                Console.WriteLine($"{ctx.User.Username} character {charname} created");
                await ctx.Channel.SendMessageAsync($"Character **{charname}** created!");
                var profile = CharacterUI.CharacterProfile(newPlayer);
                await ctx.Channel.SendMessageAsync(embed: profile);
                Console.WriteLine($"Profile for {charname} posted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                await ctx.Channel.SendMessageAsync("Something went wrong creating the character.");
            }
        }

        [Command("player")]
        public async Task PlayerProfile(CommandContext ctx, [RemainingText] string charname)
        {
            char[] charsToTrim = { ' ' };
            charname = charname.Trim(charsToTrim);

            var options = new DbContextOptionsBuilder<BotDbContext>()
                   .UseSqlite("Data Source=bot.db")
                   .Options;
            using var db = new BotDbContext(options);

            var player = await db.Players.FirstOrDefaultAsync(p => p.Name.ToLower() == charname.ToLower());

            if (player == null)
            {
                Console.WriteLine($"character {charname} NOT REAL");
                await ctx.Channel.SendMessageAsync($"Character {charname} not found.");
                return;
            }

            var profile = CharacterUI.CharacterProfile(player);
            await ctx.Channel.SendMessageAsync(embed: profile);
            Console.WriteLine($"Profile for {charname} posted.");

        }

    }
    
}
