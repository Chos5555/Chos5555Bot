using Chos5555Bot.Misc;
using DAL;
using Chos5555Bot.Services;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using DAL.Migrations;
using System.Net;

namespace Chos5555Bot.Modules.ModerationTools
{
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
        private async Task Ping()
        {
            var time = DateTimeOffset.Now - Context.Message.CreatedAt;
            await Context.Channel.SendMessageAsync($"🏓 **Pong!**\n**Latency:** {((int)time.TotalMilliseconds)} ms");
        }

        // TODO: Add .help
        [Command("help")]
        private async Task Help()
        {
            var builder = new EmbedBuilder();
            List<string> commandNames = new();

            foreach (var module in _service.Modules)
            {
                string names = null;
                string description = null;

                foreach (var command in module.Commands)
                {
                    if ((await command.CheckPreconditionsAsync(Context)).IsSuccess)
                    {
                        // Don't add duplicate command
                        if (commandNames.Contains(command.Name))
                            continue;
                        names += $"{_config.Prefix}{command.Name}\n";
                        description += $"{command.Summary ?? "No summary available for this command."}\n";
                        commandNames.Add(command.Name);
                    }
                }

                // TODO: Max 25 fields in one embed, work around this limit if there are more modules (maybe with pages?)
                if (!string.IsNullOrWhiteSpace(names))
                {
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
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

        [Command("help")]
        private async Task Help(string command)
        {
            return;
        }

            // TODO: Add briefs and summaries for commands
    }
}
