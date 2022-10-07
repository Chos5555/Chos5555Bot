﻿using DAL;
using Discord.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chos5555Bot.Modules.ModerationTools
{
    public class ModerationTools : ModuleBase<SocketCommandContext>
    {
        private readonly BotRepository repo;

        public ModerationTools(BotRepository repo)
        {
            this.repo = repo;
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("addGuild")]
        private async Task AddGuildCommand()
        {
            var guild = await repo.FindGuildById(Context.Guild.Id);
            if (guild is not null)
                return;
            guild = new Guild() { DiscordId = Context.Guild.Id };
        }

        // TODO Guild.GameCategoryId command
        // TODO Guild.MemberRole command
        // TODO Guild.RuleMessageText command
        // TODO Guild.RuleMessageId command
    }
}
