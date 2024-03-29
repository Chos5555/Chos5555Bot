﻿using Chos5555Bot.Services;
using Chos5555Bot.TypeReaders;
using Config;
using DAL;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Chos5555Bot.EventHandlers
{
    /// <summary>
    /// Class containing handlers for commands
    /// </summary>
    public class Commands
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly IServiceProvider _services;
        private readonly LogService _log;
        private readonly Configuration _config;
        private readonly BotRepository _repo;

        // Retrieve client and CommandService instance via constructor
        public Commands(DiscordSocketClient client, CommandService commandService, IServiceProvider services, LogService log, Configuration config, BotRepository repo)
        {
            _client = client;
            _commandService = commandService;
            _services = services;
            _log = log;
            _config = config;
            _repo = repo;
        }

        public async Task SetupAsync()
        {
            // Hook the MessageReceived event into our command handler
            _client.MessageReceived += HandleCommandAsync;

            // Add type reader for IEmote
            _commandService.AddTypeReader(typeof(IEmote), new IEmoteTypeReader());

            // Discover all of the command modules in the entry assembly and load them
            await _commandService.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: _services);
        }

        /// <summary>
        /// Handles incoming command and resolves error if there is one
        /// </summary>
        /// <param name="messageParam">Message parameters</param>
        /// <returns>Nothing</returns>
        public async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            // Ignore self when checking commands
            if (message.Author.Id == _client.CurrentUser.Id) return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            if (message is null)
                return;

            // Ignores messages from self
            if (message.Author.Id == _client.CurrentUser.Id)
                return;

            // Ignore message from other bots
            if (message.Author.IsBot)
                return;

            // Get prefix for guild the command was used in, if no guild is found, use '.' as default
            var guild = await _repo.FindGuild(context.Guild);
            var guildPrefix = ".";
            if (guild is not null)
                guildPrefix = guild.Prefix;

            // Determine if the message is a command based on the prefix
            // or if the message mentions the bot
            if (!message.HasMentionPrefix(_client.CurrentUser, ref argPos) &&
                !message.HasStringPrefix(guildPrefix, ref argPos))
                return;

            // Execute the command with the command context we just created
            var res = await _commandService.ExecuteAsync(context: context, argPos: argPos, services: _services);
            if (res.IsSuccess)
            {
                return;
            }

            // Resolve error if there is one
            await ResolveError(res, context, guildPrefix);
        }

        /// <summary>
        /// Resolve all different errors, send and log error messages.
        /// </summary>
        /// <param name="res">Command execute result</param>
        /// <param name="context">Command context</param>
        /// <returns>Nothing</returns>
        private async Task ResolveError(IResult res, SocketCommandContext context, string prefix)
        {
            switch (res.Error.Value)
            {
                case CommandError.Exception:
                    await ResolveCustomException(res.ErrorReason, context, prefix);
                    break;

                case CommandError.Unsuccessful:
                    await context.Channel.SendMessageAsync($"Execution of command was not successful.");
                    await _log.Log("Command execution unsuccessful " + res.ErrorReason, LogSeverity.Error);
                    break;

                case CommandError.UnmetPrecondition:
                    await context.Channel.SendMessageAsync($"You don't have permission to use this command.");
                    await _log.Log("Command unmet precondition " + res.ErrorReason, LogSeverity.Error);
                    break;

                case CommandError.ParseFailed:
                    await context.Channel.SendMessageAsync($"Command couldn't be parsed.");
                    await _log.Log("Command parse failed" + res.ErrorReason, LogSeverity.Error);
                    break;

                case CommandError.ObjectNotFound:
                    await context.Channel.SendMessageAsync($"Couldn't convert one or more objects in your command.");
                    await _log.Log("Object was not found in command " + res.ErrorReason, LogSeverity.Error);
                    break;

                case CommandError.MultipleMatches:
                    await context.Channel.SendMessageAsync($"There were multiple command pattern matches found.");
                    await _log.Log("Multiple matches found for command " + res.ErrorReason, LogSeverity.Error);
                    break;

                case CommandError.BadArgCount:
                    await context.Channel.SendMessageAsync($"Wrong number of arguments for this command.");
                    await _log.Log("Wrong number of args for command " + res.ErrorReason, LogSeverity.Error);
                    break;

                case CommandError.UnknownCommand:
                    await context.Channel.SendMessageAsync($"I couldn't recognize that command, type {prefix}help if you need help.");
                    await _log.Log("Couldn't recognize command " + res.ErrorReason, LogSeverity.Error);
                    break;
            }
        }

        private async Task ResolveCustomException(string exceptionString, SocketCommandContext context, string prefix)
        {
            if (exceptionString.Contains("GuildNotFoundException"))
            {
                await context.Channel.SendMessageAsync($"This guild is not yet registered with me, use {prefix}addGuild to add it first.");
                await _log.Log($"Cannot set selection channel, guild {context.Guild.Name} is not yet in DB.", LogSeverity.Verbose);
            }
            else
            {
                // Default response
                await context.Channel.SendMessageAsync($"There was an error while executing command.");
                await _log.Log("Exception thrown " + exceptionString, LogSeverity.Error);
            }
        }
    }
}
