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
        public static DiscordClient Client { get; set; }
        public static CommandsNextExtension Commands { get; set; }

        static async Task Main(string[] args)
        {
            var jsonReader = new StartupJSONReader();
            await jsonReader.ReadJSON();

            var dbFile = "bot.db";
            var options = new DbContextOptionsBuilder<BotDbContext>()
                .UseSqlite($"Data Source={dbFile}")
                .Options;

            // ---- DEV-FRIENDLY MIGRATION HANDLING ----
            // If you want to wipe the DB in dev mode, uncomment the next line:
            if (File.Exists(dbFile)) File.Delete(dbFile);

            using (var db = new BotDbContext(options))
            {
                try
                {
                    // Automatically apply all pending migrations
                    await db.Database.MigrateAsync();

                    string[] defaultPlayerNames = { "Red", "Blue", "Yellow", "Green" };

                    for (int i = 0; i < defaultPlayerNames.Length; i++)
                    {
                        var newPlayer = new PlayerCharacter
                        {
                            UserId = "0",
                            Name = defaultPlayerNames[i],
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
                    // Optional: for dev, delete the DB and retry
                    // File.Delete(dbFile);
                    // await db.Database.MigrateAsync();
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