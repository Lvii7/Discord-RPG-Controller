using DiscordRPGController.commands.basic;
using DiscordRPGController.commands.slash;
using DiscordRPGController.config;
using DiscordRPGController.data;
using DiscordRPGController.models;
using DiscordRPGController.utilities;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DiscordRPGController
{
    public sealed class Program
    {
        public static DiscordClient? Client { get; set; }
        public static CommandsNextExtension? Commands { get; set; }

        static async Task Main(string[] args)
        {
            var jsonReader = new StartupJSONReader();
            await jsonReader.ReadJSON();

            var dbFile = "bot.db";
            var options = new DbContextOptionsBuilder<BotDbContext>()
                .UseSqlite($"Data Source={dbFile}")
                .Options;

            // ---- DEV-FRIENDLY MIGRATION HANDLING ----
            // currenly rebuilding the database from scratch as i set everything up
            if (File.Exists(dbFile)) File.Delete(dbFile);

            using (var db = new BotDbContext(options))
            {
                try
                {
                    // automatically apply all pending migrations (hopefully...!)
                    await db.Database.MigrateAsync();

                    string[] defaultPlayerNames = { "Red", "Blue", "Yellow", "Green" };

                    for (int i = 0; i < defaultPlayerNames.Length; i++)
                    {
                        var newPlayer = new PlayerCharacter
                        {
                            UserId = "0",
                            Name = defaultPlayerNames[i],
                            // 0, 2, 4, 6
                            // 6, 3, 0, -3
                            ATK = i * 2,
                            DEF = 6 - (i * 3),
                            Level = 1
                        };

                        Console.WriteLine($"arise new default character {defaultPlayerNames[i]}");
                        db.Players.Add(newPlayer);
                    }

                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Migration failed: " + ex.Message);
                   
                    await db.Database.MigrateAsync();
                    throw;
                }
            }
            // ----------------------------------------

            var discordConfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = jsonReader.token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            Client = new DiscordClient(discordConfig);

            Client.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(2)
            });

            Client.Ready += OnClientReady;

            ulong guildID = 1351337536290816060;
            BotUtilities botUtils = new BotUtilities();
            await botUtils.InitializeEmojis(guildID);

            // =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
            //  THIS IS WHERE THE COMMANDS GO!!!!!
            // =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { jsonReader.prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false,
            };


            Commands = Client.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands<CharacterCommands>();
            Commands.RegisterCommands<BattleCommands>();
            var slashCommandsConfig = Client.UseSlashCommands();
            slashCommandsConfig.RegisterCommands<CharacterCommandsSlash>();
            slashCommandsConfig.RegisterCommands<BattleCommandsContainer>();
            slashCommandsConfig.RegisterCommands<AttackCommandsContainer>();

            // =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

            // don't put anything below this. it won't work.
            // this is the part that starts the bot!

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private static Task OnClientReady(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}