﻿using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DAL;
using System.Linq;
using DAL.Model;
using System.Runtime.Serialization;

namespace Chos5555Bot.EventHandlers
{
    public class Reactions
    {
        // TODO: Add comments
        public static async Task AddHandler(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
        {
            // TODO figure out dependency injection of repo

            BotRepository repo = new BotRepository();

            var channel = cachedChannel
                .GetOrDownloadAsync().Result as SocketGuildChannel;
            var guild = await repo.FindGuildById(channel.Guild.Id);
            var selectionRoomGame = await repo.FindGameBySelectionMessage(reaction.MessageId);
            var modRoomGame = await repo.FindGameByModRoom(channel.Id);
            var activeCheckRoomGame = await repo.FindGameByActiveCheckRoom(channel.Id);

            var removeReaction = false;

            if (channel.Id == guild.RuleRoom.DiscordId)
            {
                removeReaction = await AddedRuleRoomReaction(reaction.User.Value, guild, reaction.Emote);
            }

            if (modRoomGame is not null)
            {
                removeReaction = await AddedModRoomReaction(cachedMessage.GetOrDownloadAsync().Result, channel.Guild, reaction.Emote);
            }

            if (selectionRoomGame is not null)
            {
                removeReaction = await AddedSelectionRoomReaction(selectionRoomGame, reaction.User.Value, reaction.Emote);
            }

            if (activeCheckRoomGame is not null)
            {
                removeReaction = await AddedActiveCheckRoomReaction(activeCheckRoomGame, reaction.User.Value, channel.Guild, reaction.Emote);
            }

            if (removeReaction)
            {
                await cachedMessage.Value.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
            }
        }

        public static async Task<bool> AddedRuleRoomReaction(IUser user, Guild guild, IEmote emote)
        {
            if (emote.Name != ":white_check_mark:")
            {
                return true;
            }

            await (user as SocketGuildUser).AddRoleAsync(guild.MemberRole.DisordId);
            return false;
        }

        public static async Task<bool> AddedModRoomReaction(IUserMessage message, IGuild guild, IEmote emote)
        {
            if (message.Reactions[emote].ReactionCount == 1)
            {
                return true;
            }

            //TODO: add dependency injection
            BotRepository repo = new BotRepository();

            var userId = message.MentionedUserIds.FirstOrDefault();
            var user = await guild.GetUserAsync(userId);
            var role = await repo.FindRoleByGameAndGuild(emote, guild.Id);

            await (user as SocketGuildUser).AddRoleAsync(role.DisordId);
            return false;
        }

        public static async Task<bool> AddedSelectionRoomReaction(DAL.Model.Game game, IUser user, IEmote emote)
        {
            if (emote != game.ActiveEmote)
            {
                return true;
            }

            await (user as SocketGuildUser).AddRoleAsync(game.GameRole.DisordId);
            return false;
        }

        public static async Task<bool> AddedActiveCheckRoomReaction(DAL.Model.Game game, IUser user, IGuild guild, IEmote emote)
        {
            if (emote != game.ActiveEmote)
            {
                return true;
            }

            if (game.ModAcceptRoles.Count == 0)
            {
                await (user as SocketGuildUser).AddRolesAsync(
                    game.ActiveRoles
                    .Select(r => r.DisordId));
                return false;
            } else
            {
                var message = $"{user} wants to join you in {game.Name}, select the role you want to give them:\n";
                foreach (var role in game.ActiveRoles)
                {
                    message += $"{role.Emote} for role {guild.GetRole(role.DisordId).Name}\n";
                }

                var sentMessage = await (guild as SocketGuild).GetTextChannel(game.ModAcceptRoom.DiscordId).SendMessageAsync(message);

                await sentMessage.AddReactionsAsync(game.ActiveRoles.Select(r => r.Emote));

                return false;
            }
        }

        public static async Task RemoveHandler(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
        {
            // TODO: add dependency injection
            BotRepository repo = new BotRepository();

            var channel = cachedChannel
                .GetOrDownloadAsync().Result as SocketGuildChannel;
            var guild = await repo.FindGuildById(channel.Guild.Id);
            var selectionRoomGame = await repo.FindGameBySelectionMessage(reaction.MessageId);
            var activeCheckRoomGame = await repo.FindGameByActiveCheckRoom(channel.Id);

            if (channel.Id == guild.RuleRoom.DiscordId)
            {
                await RemovedRuleRoomReaction(reaction.User.Value);
            }

            if (selectionRoomGame is not null)
            {
                await RemoveSelectionRoomReaction(selectionRoomGame, reaction.User.Value);
            }

            if (activeCheckRoomGame is not null)
            {
                await RemoveActiveRoomReaction(activeCheckRoomGame, reaction.User.Value);
            }
        }

        public static async Task RemovedRuleRoomReaction(IUser user)
        {
            await (user as SocketGuildUser).RemoveRolesAsync((user as SocketGuildUser).Roles);
        }

        public static async Task RemoveSelectionRoomReaction(DAL.Model.Game game, IUser user)
        {
            //TODO: add dependency injection
            BotRepository repo = new BotRepository();

            var roles = await repo.FindAllRoleIdsByGame(game);

            await (user as IGuildUser).RemoveRolesAsync(roles);
        }

        public static async Task RemoveActiveRoomReaction(DAL.Model.Game game, IUser user)
        {
            await (user as IGuildUser).RemoveRolesAsync(game.ActiveRoles.Select(r => r.DisordId));
        }
    }
}
