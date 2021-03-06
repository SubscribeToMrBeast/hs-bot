﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using HSBot;
using HSBot.Core;
using HSBot.Handlers;
using HSBot.Helpers;
using HSBot.Persistent;
using System.Diagnostics;
using System.IO;
using Discord.Addons.Interactive;
using System.Collections.Generic;
using System.Linq;
/* Namespace of discord command tools.
https://www.nuget.org/packages/Discord.Net.Commands/ */
/* Namespace of discord web tools.
https://www.nuget.org/packages/Discord.Net.WebSocket/ */
// Async Task.


namespace HSBot
{
    internal class Program
    {
        private volatile IServiceCollection _services;
        private volatile DiscordSocketClient _client;

        protected internal static bool Online = true;
        private CommandService _commands;
        private readonly string _version = Assembly.GetExecutingAssembly().GetName().Version.ToString();


        private static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        public async Task RunBotAsync()
        {
            Console.SetWindowSize(200, 50);
            await Utilities.Log(MethodBase.GetCurrentMethod(), $"Application started. V{_version}.");
<<<<<<< HEAD
            //Console.Title = Config.BotConfig.ConsoleTitle;
=======
            Console.Title = Config.BotConfig.ConsoleTitle;
            try
            {
                var token = Environment.GetEnvironmentVariable("discord-hsbot-token");
            }
            catch
            {
                await Utilities.Log(MethodBase.GetCurrentMethod(), "Env. token not found.", LogSeverity.Verbose);
            }
>>>>>>> 0679f222e55694edf33ae5760713306c76d912bf

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                MessageCacheSize = Config.BotConfig.MessageCacheSize
            });
            _commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Debug
            });
            _services = new ServiceCollection() // Microsoft.Extensions.DependencyInjection
                            .AddSingleton(_client)
                            .AddSingleton(_commands)
                            .AddSingleton<CommandHandler>();
                            //.AddSingleton<InteractiveService>()
            Global.Client = _client;
            var commandhandler = new CommandHandler();
            await Utilities.Log(MethodBase.GetCurrentMethod(), "Static or singleton objects initialized.", LogSeverity.Verbose);

            await commandhandler.InitializeAsync(_client);

            _client.JoinedGuild += JoinedGuildHandler.Announce;
            _client.Log += ClientHandler.Log;
            _commands.Log += ClientHandler.Log;
            _client.Ready += CoreLoop.StartTimer;

            await Utilities.Log(MethodBase.GetCurrentMethod(), "Event handlers formed. Now logging in...", LogSeverity.Verbose);

            try
            {
                await _client.LoginAsync(TokenType.Bot, Config.BotConfig.Token);
            } catch ( Discord.Net.HttpException ex )
            {
                await Utilities.Log(MethodBase.GetCurrentMethod(), "Failed to login to discord servers. Check your config?", ex);
            }
        
            await _client.StartAsync();
            await Utilities.Log(MethodBase.GetCurrentMethod(), "Program running! :)");
            await ConsoleInput();
            await Task.Delay(-1);
        }

        private async Task ConsoleInput()
        {
            var input = string.Empty;
            while (input.Trim().ToLower() != "block")
            {
                input = Console.ReadLine();
                if (input.Trim().ToLower() == "message")
                    await ConsoleSendMessage();
            }
        }

        private async Task ConsoleSendMessage()
        {
            Console.WriteLine("Select the guild:");
            var guild = GetSelectedGuild(_client.Guilds);
            var textChannel = GetSelectedTextChannel(guild.TextChannels);
            var msg = string.Empty;
            while (msg.Trim() == string.Empty)
            {
                Console.WriteLine("Your message:");
                msg = Console.ReadLine();
            }

            await textChannel.SendMessageAsync(msg);
        }

        private SocketTextChannel GetSelectedTextChannel(IEnumerable<SocketTextChannel> channels)
        {
            var textChannels = channels.ToList();
            var maxIndex = textChannels.Count - 1;
            for (var i = 0; i <= maxIndex; i++)
            {
                Console.WriteLine($"{i} - {textChannels[i].Name}");
            }

            var selectedIndex = -1;
            while (selectedIndex < 0 || selectedIndex > maxIndex)
            {
                var success = int.TryParse(Console.ReadLine().Trim(), out selectedIndex);
                if (!success)
                {
                    Console.WriteLine("That was an invalid index, try again.");
                    selectedIndex = -1;
                }
            }

            return textChannels[selectedIndex];
        }

        private SocketGuild GetSelectedGuild(IEnumerable<SocketGuild> guilds)
        {
            var socketGuilds = guilds.ToList();
            var maxIndex = socketGuilds.Count - 1;
            for (var i = 0; i <= maxIndex; i++)
            {
                Console.WriteLine($"{i} - {socketGuilds[i].Name}");
            }

            var selectedIndex = -1;
            while (selectedIndex < 0 || selectedIndex > maxIndex)
            {
                var success = int.TryParse(Console.ReadLine().Trim(), out selectedIndex);
                if (!success)
                {
                    Console.WriteLine("That was an invalid index, try again.");
                    selectedIndex = -1;
                }
            }

            return socketGuilds[selectedIndex];
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.MessageId == Global.MessageIdToTrack)
            {
                if (reaction.Emote.Name == "👌")
                {
                    await channel.SendMessageAsync(reaction.User.Value.Username + " says OK.");
                }
            }
        }

        protected static void Shutdown(string Caller, string Reason, Exception ex = null)
        {
            System.Environment.Exit(0);
        }
    }
}
