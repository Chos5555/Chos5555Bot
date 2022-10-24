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
            if (res.IsSuccess)
            {
                return;
            }

            switch (res.Error.Value)
            {
                case CommandError.Exception:
                    await context.Channel.SendMessageAsync($"There was an error while executing command.");
                    await log.Log("Exception thrown " + res.ErrorReason, Discord.LogSeverity.Error);
                    break;

                case CommandError.Unsuccessful:
                    await context.Channel.SendMessageAsync($"Execution of command was not successful.");
                    await log.Log("Command execution unsuccessful " + res.ErrorReason, Discord.LogSeverity.Error);
                    break;

                case CommandError.UnmetPrecondition:
                    await context.Channel.SendMessageAsync($"You don't have permission to use this command.");
                    await log.Log("Command unmet precondition " + res.ErrorReason, Discord.LogSeverity.Error);
                    break;

                case CommandError.ParseFailed:
                    await context.Channel.SendMessageAsync($"Command couldn't be parsed.");
                    await log.Log("Command parse failed" + res.ErrorReason, Discord.LogSeverity.Error);
                    break;

                case CommandError.ObjectNotFound:
                    await context.Channel.SendMessageAsync($"Couldn't convert one or more objects in your command.");
                    await log.Log("Object was not found in command " + res.ErrorReason, Discord.LogSeverity.Error);
                    break;

                case CommandError.MultipleMatches:
                    await context.Channel.SendMessageAsync($"There were multiple command pattern matches found.");
                    await log.Log("Multiple matches found for command " + res.ErrorReason, Discord.LogSeverity.Error);
                    break;

                case CommandError.BadArgCount:
                    await context.Channel.SendMessageAsync($"Wrong number of arguments for this command.");
                    await log.Log("Wrong number of args for command " + res.ErrorReason, Discord.LogSeverity.Error);
                    break;

                case CommandError.UnknownCommand:
                    await context.Channel.SendMessageAsync($"I couldn't recognize that command, type {config.Prefix}help if you need help.");
                    await log.Log("Couldn't recognize command " + res.ErrorReason, Discord.LogSeverity.Error);
                    break;
            }
        }
    }
}
