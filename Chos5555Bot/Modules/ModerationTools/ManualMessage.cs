using DAL;
using Chos5555Bot.Services;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Chos5555Bot.Modules.ModerationTools
{
    /// <summary>
    /// Module class containing commands for managing messages
    /// </summary>
    [Name("Manual Message Management")]
    public class ManualMessage : ModuleBase<SocketCommandContext>
    {
        private readonly BotRepository _repo;
        private readonly LogService _log;

        public ManualMessage(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("ModifyMessage")]
        [Summary("Modifies content of a message with given Id (Careful as some messages can be in DB and will not be updated)")]
        private async Task ModifyMessage(
            [Name("Message Id")][Summary("Id of the message to be modified.")] ulong messageId,
            [Name("Text")][Summary("New text for the message")][Remainder] string text)
        {
            var message = await (Context.Channel as ITextChannel).GetMessageAsync(messageId);
            await (message as IUserMessage).ModifyAsync(m => { m.Content = text; });
            await _log.Log($"Modified content on a message in {Context.Guild.Name}.", LogSeverity.Info);
        }
    }
}
