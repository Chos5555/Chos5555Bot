﻿using DAL;
using Chos5555Bot.Services;
using Discord.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Misc;
using Discord.Rest;
using Discord.WebSocket;
using Chos5555Bot.Misc;

namespace Chos5555Bot.Modules.ModerationTools
{
    public class ManualRole : ModuleBase<SocketCommandContext>
    {
        private readonly BotRepository _repo;
        private readonly LogService _log;

        public ManualRole(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setRoleDescription")]
        private async Task setRoleDescriptionCommand(IRole discordRole, [Remainder] string desc)
        {
            var role = await _repo.FindRole(discordRole.Id);

            var oldDesc = role.Description;

            role.Description = desc;
            await _repo.UpdateRole(role);

            // Update text on announce message
            var game = await _repo.FindGameByRole(role);
            var message = await MessageFinder.FindAnnouncedMessage(role, Context.Guild.GetTextChannel(game.ActiveCheckRoom.DiscordId));

            var newMessageContent = message.Content.Replace(oldDesc, role.Description);

            await (message as IUserMessage).ModifyAsync(m => { m.Content = newMessageContent; });
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setRoleEmote")]
        private async Task setRoleEmoteCommand(IRole discordRole, string emote)
        {
            var role = await _repo.FindRole(discordRole.Id);

            var parsedEmote = EmoteParser.ParseEmote(emote);

            var oldEmote = role.ChoiceEmote.Out();

            role.ChoiceEmote = parsedEmote;
            await _repo.UpdateRole(role);

            // Update emote on announce message
            var game = await _repo.FindGameByRole(role);
            var message = await MessageFinder.FindAnnouncedMessage(role, Context.Guild.GetTextChannel(game.ActiveCheckRoom.DiscordId));

            var newMessageContent = message.Content.Replace(oldEmote.ToString(), role.ChoiceEmote.Out().ToString());

            await (message as IUserMessage).ModifyAsync(m => { m.Content = newMessageContent; });
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setRoleResettable")]
        private async Task setRoleResettableCommand(IRole discordRole, bool value)
        {
            var role = await _repo.FindRole(discordRole.Id);

            role.Resettable = value;
            await _repo.UpdateRole(role);
        }
    }
}
