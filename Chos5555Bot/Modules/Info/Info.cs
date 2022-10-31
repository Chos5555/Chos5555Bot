using DAL;
using Chos5555Bot.Services;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Config;
using Chos5555Bot.Exceptions;

namespace Chos5555Bot.Modules.Info
{
    /// <summary>
    /// Module class containing informative commands.
    /// </summary>
    [Name("Info")]
    public class Info : ModuleBase<SocketCommandContext>
    {
        private readonly BotRepository _repo;
        private readonly LogService _log;
        private readonly CommandService _service;
        private readonly Configuration _config;

        public Info(BotRepository repo, LogService log, CommandService service, Configuration config)
        {
            _repo = repo;
            _log = log;
            _service = service;
            _config = config;
        }

        [Command("ping")]
        [Summary("Pings the bot, returns latency.")]
        private async Task Ping()
        {
            var time = DateTimeOffset.Now - Context.Message.CreatedAt;
            await Context.Channel.SendMessageAsync($"🏓 **Pong!**\n**Latency:** {(int)time.TotalMilliseconds} ms");
        }

        /// <summary>
        /// Lists all commands available to user that used the command (depending on users permission).
        /// </summary>
        /// <returns>Nothing</returns>
        [Command("help")]
        [Summary("Lists all commands available to you.")]
        private async Task Help()
        {
            var guild = await _repo.FindGuild(Context.Guild);
            if (guild is null)
                throw new GuildNotFoundException();

            // Start building embed
            var builder = new EmbedBuilder()
            {
                Title = $"Command Prefix = \"{guild.Prefix}\""
            };
            List<string> commandNames = new();

            Dictionary<string, List<CommandInfo>> groups = new();

            // Separate commands by name of modules (which match if they should be in the same field)
            foreach (var module in _service.Modules)
            {
                if (module.Name is null)
                    continue;

                if (!groups.Keys.Contains(module.Name))
                    groups.Add(module.Name, new());

                groups[module.Name].AddRange(module.Commands);
            }

            // Add commands into embed
            foreach (var (module, commands) in groups)
            {
                string names = null;
                string description = null;

                foreach (var command in commands)
                    // Only list command, if user has the right permission to use it
                    if ((await command.CheckPreconditionsAsync(Context)).IsSuccess)
                    {
                        // Don't add duplicate command
                        if (commandNames.Contains(command.Name))
                            continue;
                        names += $"{command.Name}\n";
                        var commandDescription = $"{command.Summary ?? "No summary available for this command."}\n";

                        description += commandDescription;

                        // Forces next command onto new line if description of command is longer than 1 complete line
                        // TODO: Find a better way to add newline
                        names += new string('\n', commandDescription.Length / 51);

                        commandNames.Add(command.Name);
                    }

                // TODO: Max 25 fields in one embed, work around this limit if there are more modules (maybe with pages?)
                if (!string.IsNullOrWhiteSpace(names))
                {
                    builder.AddField(x =>
                    {
                        x.Name = module;
                        x.Value = names;
                        x.IsInline = true;
                    });

                    builder.AddField(x =>
                    {
                        x.Name = "\u200b";
                        x.Value = description;
                        x.IsInline = true;
                    });
                    // Add empty field to allign fields in 2 collumns 
                    builder.AddField(x =>
                    {
                        x.Name = "\u200b";
                        x.Value = "\u200b";
                        x.IsInline = true;
                    });
                }
            }

            await Context.Channel.SendMessageAsync("", embed: builder.Build());
        }

        /// <summary>
        /// Sends message with help for a specific command. Lists out all of its variant, their summaries and paramaters.
        /// </summary>
        /// <param name="commandName">Name of the command to be listed.</param>
        /// <returns>Nothing</returns>
        [Command("help")]
        [Alias("helpwith")]
        [Summary("Helps you with a specific command.")]
        private async Task Help(
            [Name("Command")][Summary("Name of command with which you need help.")] string commandName)
        {
            var result = _service.Search(Context, commandName);

            if (!result.IsSuccess)
            {
                await Context.Channel.SendMessageAsync($"I couldn't find the command {commandName}");
                return;
            }

            // Start building embed
            var embedDescription = result.Commands.Count == 1 ? $"There is {result.Commands.Count} result:" : $"There are {result.Commands.Count} results:";

            var builder = new EmbedBuilder()
            {
                Title = $"Help for '{commandName}':",
                Description = embedDescription
            };

            foreach (var command in result.Commands.Select(c => c.Command))
            {
                // Create a code block with summary and parameters
                var description = $"```Summary:\n    {command.Summary ?? "No summary available for this command."}\n";

                // List out all parameters
                if (command.Parameters.Any())
                    description += "Parameters:\n";
                else
                    description += "No parameters\n";

                foreach (var parameter in command.Parameters)
                    description += $"   {parameter.Name} - {parameter.Summary ?? "No description provided."}\n";

                // End code block
                description += "```";

                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", command.Aliases);
                    x.Value = description;
                    x.IsInline = false;
                });
            }

            await Context.Channel.SendMessageAsync("", embed: builder.Build());
        }
    }
}
