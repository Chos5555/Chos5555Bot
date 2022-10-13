using DAL;
using Discord.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chos5555Bot.Services;

namespace Chos5555Bot.Modules.ModerationTools
{
    public class ModerationTools : ModuleBase<SocketCommandContext>
    {
        private readonly BotRepository _repo;
        private readonly LogService _log;

        public ModerationTools(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("addGuild")]
        private async Task AddGuildCommand()
        {
            var guild = await _repo.FindGuild(Context.Guild);
            if (guild is not null)
                return;
            guild = new Guild() { DiscordId = Context.Guild.Id };
            await _repo.AddGuild(guild);

            await _log.Log($"Added guild {Context.Guild.Name} to the DB.", LogSeverity.Info);
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("deleteGame")]
        private async Task DeleteGameCommand(string gameName, IRole discordRole)
        {
            var game = await _repo.FindGameByNameAndGameRole(gameName, discordRole.Id);

            if (game is null)
                await Context.Channel.SendMessageAsync($"Couldn't find game named {gameName} with role {discordRole}");

            foreach (var role in await _repo.FindAllRolesByGame(game))
            {
                await _repo.RemoveRole(await _repo.FindRole(role));
                await Context.Guild.GetRole(role.DisordId).DeleteAsync();
            }

            foreach (var room in game.Rooms)
            {
                await _repo.RemoveRoom(await _repo.FindRoom(room));
                await Context.Guild.GetChannel(room.DiscordId).DeleteAsync();
            }

            await _repo.RemoveGame(game);
        }

        // TODO Guild.GameCategoryId command
        // TODO Guild.MemberRole command
        // TODO Guild.RuleMessageText command
        // TODO Guild.RuleMessageId command
        // TODO AddRoom to Game.Rooms with one of G.ActiveRoles, set perms for the role to view channel
    }
}
