using DAL;
using Discord.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chos5555Bot.Services;
using Discord.WebSocket;
using Chos5555Bot.Misc;

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
                    Resetable = false,
                    NeedsModApproval = true,
                };
            }

            // Set modRoom viewable for new mod role
            await PermissionSetter.UpdateViewChannel(role, Context.Guild.GetChannel(game.ModAcceptRoom.DiscordId), PermValue.Allow);
            // TODO: If you're gonna be sending messages to activeRoom only after modRole exists, call game announcer here

            game.ModAcceptRoles.Add(modRole);
            await _repo.UpdateGame(game);
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setRuleText")]
        private async Task setRuleTextCommand([Remainder] string text = null)
        {
            if (Context.Message.ReferencedMessage is not null)
                text = Context.Message.ReferencedMessage.Content;

            var guild = await _repo.FindGuild(Context.Guild.Id);
            guild.RuleMessageText = text;

            if (guild.RuleRoom is null)
                return;

            var ruleRoom = Context.Guild.GetChannel(guild.RuleRoom.DiscordId) as SocketTextChannel;

            // Delete old message if one exists
            if (guild.RuleMessageId != 0)
            {
                await (await ruleRoom.GetMessageAsync(guild.RuleMessageId)).DeleteAsync();
            }
            guild.RuleMessageId = (await ruleRoom.SendMessageAsync(guild.RuleMessageText)).Id;

            await _repo.UpdateGuild(guild);
        }

        // TODO Guild.GameCategoryId command
        // TODO Guild.RuleMessageText command
        // TODO Guild.RuleMessageId command
        // TODO AddRoom to Game.Rooms with one of G.ActiveRoles, set perms for the role to view channel
    }
}
