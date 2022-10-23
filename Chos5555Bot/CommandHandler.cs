using Chos5555Bot.Services;
using Config;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Chos5555Bot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient client;
        private readonly CommandService commandService;
        private readonly IServiceProvider services;
        private readonly LogService log;
        private readonly Configuration config;

        // Retrieve client and CommandService instance via constructor
        public CommandHandler(DiscordSocketClient client, CommandService commandService, IServiceProvider services, LogService log, Configuration config)
        {
            this.client = client;
            this.commandService = commandService;
            this.services = services;
            this.log = log;
            this.config = config;
        }

        public async Task SetupAsync()
        {
            // Hook the MessageReceived event into our command handler
            client.MessageReceived += HandleCommandAsync;

            // Discover all of the command modules in the entry assembly and load them
            await commandService.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: services);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            // Ignore self when checking commands
            if (message.Author.Id == client.CurrentUser.Id) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!message.HasStringPrefix(config.Prefix.ToString(), ref argPos) ||
                message.Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(client, message);

            // Execute the command with the command context we just created
            var res = await commandService.ExecuteAsync(context: context, argPos: argPos, services: services);
            // TODO: Send different messages on different results, eg: Error vs command not found
            // TODO: res.Error.Value == CommandError.Exception for errors, otherwise command has been probably typed wrong, handle multiple reasons, add method for it
            if (!res.IsSuccess)
            {
                await context.Channel.SendMessageAsync($"I couldn't recognize that command, type !help if you need help.");
                await log.Log("Couldn't recognize command.", Discord.LogSeverity.Error);
            }
        }
    }
}
