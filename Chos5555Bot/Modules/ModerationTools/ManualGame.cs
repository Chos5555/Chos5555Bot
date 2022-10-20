﻿using DAL;
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
using DAL.Misc;

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
            var modRole = await _repo.FindRole(role);

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

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("removeModRole")]
        private async Task RemoveModRoleCommand(IRole role, string gameName)
        {
            var game = await _repo.FindGame(gameName);
            var modRole = await _repo.FindRole(role);

            game.ModAcceptRoles.Remove(modRole);
            await _repo.UpdateGame(game);

            await PermissionSetter.UpdateViewChannel(role, Context.Guild.GetChannel(game.ModAcceptRoom.DiscordId), PermValue.Deny);
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setModRoom")]
        private async Task setMemberRoleCommand(IChannel discordChannel, string gameName)
        {
            var game = await _repo.FindGame(gameName);
            var channel = await _repo.FindRoom(discordChannel);

            if (channel is null)
            {
                channel = new Room()
                {
                    DiscordId = discordChannel.Id,
                };
                await _repo.AddRoom(channel);
            }

            game.ModAcceptRoom = channel;
            await _repo.UpdateGame(game);

            var modDiscordRoles = new List<IRole>();
            foreach (var role in game.ModAcceptRoles)
            {
                modDiscordRoles.Add(Context.Guild.GetRole(role.DisordId));
            }

            await PermissionSetter.SetShownForRoles(modDiscordRoles, Context.Guild.GetRole(game.MainActiveRole.DisordId), discordChannel as IGuildChannel);
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setGameEmote")]
        private async Task setGameEmoteCommand(string gameName, string emote)
        {
            var game = await _repo.FindGame(gameName);

            var parsedEmote = EmoteParser.ParseEmote(emote);

            var oldEmote = game.ActiveEmote.Out();

            game.ActiveEmote = parsedEmote;
            await _repo.UpdateGame(game);

            // Update emote on announce message
            var guildSelectionChannelId = (await _repo.FindGuild(Context.Guild)).SelectionRoom.DiscordId;
            var message = await MessageFinder.FindAnnouncedMessage(game.GameRole, Context.Guild.GetTextChannel(guildSelectionChannelId));

            var newMessageContent = message.Content.Replace(oldEmote.ToString(), game.ActiveEmote.Out().ToString());

            await (message as IUserMessage).ModifyAsync(m => { m.Content = newMessageContent; });
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("addChannelToGame")]
        private async Task AddChannelToGameCommand([Remainder] string gameName)
        {
            var game = await _repo.FindGame(gameName);

            var room = await _repo.FindRoom(Context.Channel);

            if (room is null)
            {
                room = new Room()
                {
                    DiscordId = Context.Channel.Id
                };
                await _repo.AddRoom(room);
            }

            game.Rooms.Add(room);
            await _repo.UpdateGame(game);
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("addRoleToGame")]
        private async Task AddRoleToGameCommand(IRole discordRole, bool resettable, bool needModApproval, IEmote emote, [Remainder] string gameName)
        {
            var game = await _repo.FindGame(gameName);

            var role = await _repo.FindRole(discordRole);

            if (role is null)
            {
                role = new Role()
                {
                    DisordId = discordRole.Id,
                    Name = discordRole.Name,
                    Resettable = resettable,
                    NeedsModApproval = needModApproval,
                    ChoiceEmote = EmoteParser.ParseEmote(emote.ToString())
                };
                await _repo.AddRole(role);
            }

            game.ActiveRoles.Add(role);

            // Announce new active role
            await GameAnnouncer.AnnounceActiveRole(role, game, Context.Guild.GetChannel(game.ActiveCheckRoom.DiscordId) as ITextChannel, Context);

            await _repo.UpdateGame(game);
        }

        // TODO reset resettable roles
    }
}
