using DAL;
using Chos5555Bot.Services;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using Chos5555Bot.Misc;

namespace Chos5555Bot.Modules.ModerationTools
{
    public class ManualGame : ModuleBase<SocketCommandContext>
    {
        private readonly BotRepository _repo;
        private readonly LogService _log;

        public ManualGame(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("deleteGame")]
        private async Task DeleteGameCommand(string gameName, IRole discordRole)
        {
            var game = await _repo.FindGameByNameAndGameRole(gameName, discordRole.Id);
            var guild = await _repo.FindGuild(game.Guild);

            if (game is null)
                await Context.Channel.SendMessageAsync($"Couldn't find game named {gameName} with role {discordRole}");

            foreach (var role in await _repo.FindAllRolesByGame(game))
            {
                await _repo.RemoveRole(await _repo.FindRole(role));
                await Context.Guild.GetRole(role.DisordId).DeleteAsync();
            }

            ulong? categoryId = 0;
            foreach (var room in game.Rooms.ToArray())
            {
                await _repo.RemoveRoom(await _repo.FindRoom(room));
                var discordChannel = Context.Guild.GetChannel(room.DiscordId);
                await discordChannel.DeleteAsync();

                categoryId = (discordChannel as INestedChannel).CategoryId;
                if (categoryId.HasValue)
                    categoryId = categoryId.Value;
            }

            var categoryChannel = Context.Guild.GetChannel(categoryId.Value);
            await categoryChannel.DeleteAsync();

            var channel = Context.Guild.GetChannel(guild.SelectionRoom.DiscordId) as ISocketMessageChannel;
            await (await channel.GetMessageAsync(game.SelectionMessageId)).DeleteAsync();

            await _repo.RemoveGame(game);

            await _log.Log($"Deleted game {game.Name} from server {Context.Guild.Name}", LogSeverity.Info);
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("addModRole")]
        private async Task AddModRoleCommand(IRole role, string gameName)
        {
            var game = await _repo.FindGame(gameName);
            var modRole = await _repo.FindRole(role.Id);

            if (modRole is null)
            {
                modRole = new Role()
                {
                    DisordId = role.Id,
                    Name = role.Name,
                    Resettable = false,
                    NeedsModApproval = true,
                };
            }

            // Set modRoom viewable for new mod role
            await PermissionSetter.UpdateViewChannel(role, Context.Guild.GetChannel(game.ModAcceptRoom.DiscordId), PermValue.Allow);
            // TODO: If you're gonna be sending messages to activeRoom only after modRole exists, call game announcer here

            game.ModAcceptRoles.Add(modRole);
            await _repo.UpdateGame(game);
        }

        // TODO add channel to game, add channel to role, remove channel (with archive) game
        // TODO add role to game
        // TODO set active emote (change emote in select message) game
        // TODO set mod accept room (sets permission only for modaccept roles) game
        // TODO remove mod accept role game
        // TODO reset resettable roles
    }
}
