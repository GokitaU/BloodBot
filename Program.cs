using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using REGEX = System.Text.RegularExpressions;
using System.Reflection;
using System.Configuration;

namespace BloodBot
{
    class Program
    {
        // TODO switch to D#+
        public static DiscordSocketClient client = new DiscordSocketClient();
        private CommandService commands = new CommandService();
        private IServiceProvider services;

        BloodBot bloodbot = new BloodBot();
        Random random = new Random();
        BetResolver betresolver = new BetResolver();
        
        readonly string LOGIN_TOKEN;

        public Program()
        {
            LOGIN_TOKEN = Config.Data[Config.ConfigKeys.DiscordToken];
        }

        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            client.Log += Log;
            //client.MessageReceived += MessageReceived;

            await client.LoginAsync(TokenType.Bot, LOGIN_TOKEN);
            await client.StartAsync();

            services = new ServiceCollection().BuildServiceProvider();
            await InstallCommands();
 
            client.Ready += () =>
            {
                Task BloodBotTask = Task.Run(() =>
                {
                    bloodbot.Run();
                });
                return Task.CompletedTask;
            };
            await Task.Delay(-1);
        }

        public async Task InstallCommands()
        {
            client.MessageReceived += HandleCommand;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            int argPos = 0;
            if (!(message.HasCharPrefix('!', ref argPos)
                || message.HasMentionPrefix(client.CurrentUser, ref argPos))) return;
            var context = new CommandContext(client, message);
            var result = await commands.ExecuteAsync(context, argPos, services);
            if (!result.IsSuccess)
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
