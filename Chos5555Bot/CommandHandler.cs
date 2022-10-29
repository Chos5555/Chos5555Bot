using Chos5555Bot.Services;
using Config;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Chos5555Bot
{
    // TODO: Move into Handlers folder
    /// <summary>
    /// Class containing handlers for commands
    /// </summary>
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly IServiceProvider _services;
        private readonly LogService _log;
        private readonly Configuration _config;

        // Retrieve client and CommandService instance via constructor
        public CommandHandler(DiscordSocketClient client, CommandService commandService, IServiceProvider services, LogService log, Configuration config)
        {
            _client = client;
            _commandService = commandService;
            _services = services;
            _log = log;
            _config = config;
        }

        public async Task SetupAsync()
        {
            // TODO: Add into Program.cs
            // Hook the MessageReceived event into our command handler
            _client.MessageReceived += HandleCommandAsync;

            // Discover all of the command modules in the entry assembly and load them
            await _commandService.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: _services);
        }

        /// <summary>
        /// Handles incoming command and resolves error if there is one
        /// </summary>
        /// <param name="messageParam">Message parameters</param>
        /// <returns>Nothing</returns>
        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            // Ignore self when checking commands
            if (message.Author.Id == _client.CurrentUser.Id) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!message.HasStringPrefix(_config.Prefix.ToString(), ref argPos) ||
                message.Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);

            // Execute the command with the command context we just created
            var res = await _commandService.ExecuteAsync(context: context, argPos: argPos, services: _services);
            if (res.IsSuccess)
            {
                return;
            }

            // Resolve error if there is one
            await ResolveError(res, context);
        }

        /// <summary>
        /// Resolve all different errors, send and log error messages.
        /// </summary>
        /// <param name="res">Command execute result</param>
        /// <param name="context">Command context</param>
        /// <returns>Nothing</returns>
        private async Task ResolveError(IResult res, SocketCommandContext context)
        {
            switch (res.Error.Value)
            {
                case CommandError.Exception:
                    await context.Channel.SendMessageAsync($"There was an error while executing command.");
                    await _log.Log("Exception thrown " + res.ErrorReason, Discord.LogSeverity.Error);
                    break;

                case CommandError.Unsuccessful:
                    await context.Channel.SendMessageAsync($"Execution of command was not successful.");
                    await _log.Log("Command execution unsuccessful " + res.ErrorReason, Discord.LogSeverity.Error);
                    break;

                case CommandError.UnmetPrecondition:
                    await context.Channel.SendMessageAsync($"You don't have permission to use this command.");
                    await _log.Log("Command unmet precondition " + res.ErrorReason, Discord.LogSeverity.Error);
                    break;

                case CommandError.ParseFailed:
                    await context.Channel.SendMessageAsync($"Command couldn't be parsed.");
                    await _log.Log("Command parse failed" + res.ErrorReason, Discord.LogSeverity.Error);
                    break;

                case CommandError.ObjectNotFound:
                    await context.Channel.SendMessageAsync($"Couldn't convert one or more objects in your command.");
                    await _log.Log("Object was not found in command " + res.ErrorReason, Discord.LogSeverity.Error);
                    break;

                case CommandError.MultipleMatches:
                    await context.Channel.SendMessageAsync($"There were multiple command pattern matches found.");
                    await _log.Log("Multiple matches found for command " + res.ErrorReason, Discord.LogSeverity.Error);
                    break;

                case CommandError.BadArgCount:
                    await context.Channel.SendMessageAsync($"Wrong number of arguments for this command.");
                    await _log.Log("Wrong number of args for command " + res.ErrorReason, Discord.LogSeverity.Error);
                    break;

                case CommandError.UnknownCommand:
                    await context.Channel.SendMessageAsync($"I couldn't recognize that command, type {_config.Prefix}help if you need help.");
                    await _log.Log("Couldn't recognize command " + res.ErrorReason, Discord.LogSeverity.Error);
                    break;
            }
        }
    }
}
