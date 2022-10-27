using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DAL;
using System.Linq;
using DAL.Model;
using DAL.Misc;
using Chos5555Bot.Services;
using System;
using System.Collections.Generic;

namespace Chos5555Bot.EventHandlers
{
    /// <summary>
    /// Class <c>Reactions</c> contains handlers for adding/removing reactions and other helper methods
    /// </summary>
    internal class Reactions
    {
        private static BotRepository _repo;
        private static LogService _log;

        public static void InitReactions(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        /// <summary>
        /// This method is the main handler for added reactions. Checks in which room the reaction was added and calls the appropriate handler.
        /// Removes the reaction if a wrong reaction was added to a selection message.
        /// </summary>
        /// <param name="cachedMessage">Uncached message</param>
        /// <param name="uncachedChannel">Uncached channel</param>
        /// <param name="reaction">Reaction</param>
        /// <returns>Task</returns>
        public static async Task ReactionAdded(Cacheable<IUserMessage, ulong> uncachedMessage, Cacheable<IMessageChannel, ulong> uncachedChannel, SocketReaction reaction)
        {
            var channel = await uncachedChannel.GetOrDownloadAsync() as SocketGuildChannel;

            // Ignore if reactions was added by bot
            if (channel.Guild.GetUser(reaction.UserId).IsBot)
                return;

            var guild = await _repo.FindGuild(channel.Guild.Id);
            var selectionRoomGame = await _repo.FindGameBySelectionMessage(reaction.MessageId);
            var modRoomGame = await _repo.FindGameByModRoom(channel.Id);
            var activeCheckRoomGame = await _repo.FindGameByActiveCheckRoom(channel.Id);
            var message = await uncachedMessage.GetOrDownloadAsync();

            var removeReaction = false;

            if (guild.RuleRoom is not null && channel.Id == guild.RuleRoom.DiscordId)
            {
                removeReaction = await AddedRuleRoomReaction(reaction.User.Value, guild, reaction.Emote);
            }

            if (modRoomGame is not null)
            {
                removeReaction = await AddedModRoomReaction(message, channel.Guild, reaction.Emote, modRoomGame);
            }

            if (selectionRoomGame is not null)
            {
                removeReaction = await AddedSelectionRoomReaction(selectionRoomGame, reaction.User.Value, reaction.Emote);
            }

            if (activeCheckRoomGame is not null)
            {
                removeReaction = await AddedActiveCheckRoomReaction(activeCheckRoomGame, reaction.User.Value, channel.Guild, message, reaction.Emote);
            }

            if (removeReaction)
            {
                await message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
            }
        }

        /// <summary>
        /// Handles reaction added to the rule room message. If the ChoiceEmote reacted with was correct, gives member role to the user.
        /// </summary>
        /// <param name="user">User that reacted to rule room message.</param>
        /// <param name="guild">Guild in which the reaction was added.</param>
        /// <param name="emote">ChoiceEmote with which was reacted.</param>
        /// <returns>
        /// True if the reaction should be removed, false otherwise.
        /// </returns>
        public static async Task<bool> AddedRuleRoomReaction(IUser user, Guild guild, IEmote emote)
        {
            await _log.Log($"User {user.Username} has reacted with {emote.Name} in rule room in server {guild.Id}", LogSeverity.Info);

            // Check if right emote was used
            var checkmark = EmoteParser.ParseEmote("✅");
            if (!CompareEmoteToEmoteEmoji(emote, checkmark))
            {
                await _log.Log($"Wrong emoji was used, deleting react.", LogSeverity.Verbose);
                return true;
            }

            await (user as SocketGuildUser).AddRoleAsync(guild.MemberRole.DisordId);
            await _log.Log($"Giving {user.Username} {guild.Id} member role.", LogSeverity.Verbose);
            return false;
        }

        /// <summary>
        /// Handles reaction added to a message in the mod room. Gives user mentioned in the message the role selected by mod. Administrators are allowed to accept members for all games, since they see all channels
        /// </summary>
        /// <param name="message">Message to which the reaction was added.</param>
        /// <param name="guild">Guild in which the reaction was added.</param>
        /// <param name="emote">ChoiceEmote with which was reacted.</param>
        /// <returns>
        /// True if the reaction should be removed, false otherwise.
        /// </returns>
        public static async Task<bool> AddedModRoomReaction(IUserMessage message, IGuild guild, IEmote emote, DAL.Model.Game game)
        {
            await _log.Log($"ModRoom received reaction {emote.Name} in game {game.Name} on server {guild.Id}", LogSeverity.Info);

            // If there is only 1 of the new emote, it was not given by the bot, thus is not a valid role emote
            if (message.Reactions[emote].ReactionCount == 1)
            {
                await _log.Log($"Wrong emoji was used, deleting react.", LogSeverity.Verbose);
                return true;
            }

            var userId = message.MentionedUserIds.SingleOrDefault();
            var user = await guild.GetUserAsync(userId);
            var roleId = message.MentionedRoleIds.SingleOrDefault();
            var role = guild.GetRole(roleId);

            // Find activeCheckRoom message to which the original reaction was added, remove it, PM user, delete message in modRoom
            if (emote == new Emoji("❎"))
            {
                await _log.Log($"Request denied, removing reaction and DMing user {user.Username}", LogSeverity.Verbose);

                var activeCheckRoom = await guild.GetChannelAsync(game.ActiveCheckRoom.DiscordId) as IMessageChannel;
                var messages = await activeCheckRoom.GetMessagesAsync().FlattenAsync();
                var messageWithReaction = messages.Where(m => m.MentionedRoleIds.Contains(roleId)).SingleOrDefault();
                await messageWithReaction.RemoveReactionAsync(emote, user);

                await user.SendMessageAsync($"Unfortunately your request to get role {role.Name} on server {guild.Name}" +
                    $" has been rejected. Please contact a moderator for reason of the rejection. Thanks.");
                await message.Channel.SendMessageAsync($"You have rejected {user.Mention}'s request for role {role.Mention}.");
                await message.DeleteAsync();
                return false;
            }

            if (emote == new Emoji("✅"))
            {
                await _log.Log($"Request accepted, giving {role.Name} to {user.Username}", LogSeverity.Verbose);

                // Add role designated by reacted emote, PM user, delete message in modRoom
                await (user as SocketGuildUser).AddRoleAsync(role);

                await user.SendMessageAsync($"You have been given role {role.Name} on server {guild.Name}. Enjoy!");
                await message.Channel.SendMessageAsync($"Given user {user.Mention} role {role.Mention}.");
                await message.DeleteAsync();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Handles reaction added to a message in the game selection room. Gives user the appropriate game role.
        /// </summary>
        /// <param name="game">Game to which message was reacted.</param>
        /// <param name="user">User that reacted to the game selection message.</param>
        /// <param name="emote">ChoiceEmote with which was reacted.</param>
        /// <returns>
        /// True if the reaction should be removed, false otherwise.
        /// </returns>
        public static async Task<bool> AddedSelectionRoomReaction(DAL.Model.Game game, IUser user, IEmote emote)
        {
            await _log.Log($"{user.Username} added reaction to selectionRoom, game {game.Name}.", LogSeverity.Info);

            // Check if right emote was used
            if (!CompareEmoteToEmoteEmoji(emote, game.ActiveEmote))
            {
                await _log.Log($"Wrong emoji was used, deleting react.", LogSeverity.Verbose);
                return true;
            }

            await _log.Log($"Giving user {user.Username} gameRole of {game.Name}.", LogSeverity.Verbose);

            await (user as SocketGuildUser).AddRoleAsync(game.GameRole.DisordId);
            return false;
        }

        /// <summary>
        /// Handles reaction added to a message in the active check room. Gives all of the games active roles if there is no designated mod room.
        /// If there is a mod room, posts a message to mod room and adds all possible active role reactions.
        /// </summary>
        /// <param name="game">Game in which active room was reacted.</param>
        /// <param name="user">User that reacted to the game active room message.</param>
        /// <param name="guild">Guild in which the reaction was added.</param>
        /// <param name="emote">ChoiceEmote with which was reacted.</param>
        /// <returns>
        /// True if the reaction should be removed, false otherwise.
        /// </returns>
        public static async Task<bool> AddedActiveCheckRoomReaction(DAL.Model.Game game, IUser user, IGuild guild, IUserMessage message, IEmote emote)
        {
            await _log.Log($"{user.Username} added reaction to {game.Name}'s activeCheckRoom.", LogSeverity.Info);

            // Find mentioned role in message
            var roleId = message.MentionedRoleIds.SingleOrDefault();
            var role = await _repo.FindRole(roleId);
            var discordRole = guild.GetRole(roleId);

            if (!CompareEmoteToEmoteEmoji(emote, role.ChoiceEmote))
            {
                await _log.Log($"Wrong emoji was used, deleting react.", LogSeverity.Verbose);
                return true;
            }

            // If user doesn't have gameRole he shouldn't even see the role choice room, but check anyways
            if (!(user as SocketGuildUser).Roles
                .Contains(guild.GetRole(game.GameRole.DisordId)))
                return true;

            // If user doesn't have games mainActiveRole and the role doesn't need mod approval, remove reaction
            // (a.k.a. it's a secondary role that will only unlock channels if you have mainActiveRole)
            if (!(user as SocketGuildUser).Roles
                .Contains(guild.GetRole(game.MainActiveRole.DisordId))
                && !role.NeedsModApproval)
            {
                await _log.Log($"{user.Username} has no {game.Name} active role and wants a role that doesn't need approval.", LogSeverity.Verbose);
                return true;
            }

            // If there are no mod role, add all active roles
            if (game.ModAcceptRoles.Count == 0)
            {
                await _log.Log($"No modRoles found, giving {user.Username} all activeRoles of {game.Name}.", LogSeverity.Verbose);

                await (user as SocketGuildUser).AddRolesAsync(
                    game.ActiveRoles
                    .Select(r => r.DisordId));
                return false;
            }

            // If there is a mod role, post into mod room and react with yes or no emotes
            if (role.NeedsModApproval)
            {
                await _log.Log($"Sending {user.Username}'s request for {role.Name} into {game.Name}'s modRoom.", LogSeverity.Verbose);

                var messageText = $"{user.Mention} wants the role {discordRole.Mention}, should I give it to them?";

                var modMessage = await (guild as SocketGuild).GetTextChannel(game.ModAcceptRoom.DiscordId).SendMessageAsync(messageText);

                await modMessage.AddReactionAsync(new Emoji("✅"));
                await modMessage.AddReactionAsync(new Emoji("❎"));

                return false;
            }

            await _log.Log($"Giving user {user.Username} gameRole of {game.Name}.", LogSeverity.Verbose);

            // If role doesn't need mod approval, give it to the user
            await (user as SocketGuildUser).AddRoleAsync(roleId);

            return false;
        }

        /// <summary>
        /// This method is the main handler for removing reactions. Checks in which room the reaction was added and calls the appropriate handler.
        /// </summary>
        /// <param name="uncachedMessage">Uncached message</param>
        /// <param name="uncachedChannel">Uncached channel</param>
        /// <param name="reaction">Reaction</param>
        /// <returns>Nothing</returns>
        public static async Task ReactionRemoved(Cacheable<IUserMessage, ulong> uncachedMessage, Cacheable<IMessageChannel, ulong> uncachedChannel, SocketReaction reaction)
        {
            var channel = await uncachedChannel.GetOrDownloadAsync() as SocketGuildChannel;
            var message = await uncachedMessage.GetOrDownloadAsync();

            // Ignore if reactions was added by bot
            if (channel.Guild.GetUser(reaction.UserId).IsBot)
                return;

            var guild = await _repo.FindGuild(channel.Guild.Id);
            var selectionRoomGame = await _repo.FindGameBySelectionMessage(reaction.MessageId);
            var activeCheckRoomGame = await _repo.FindGameByActiveCheckRoom(channel.Id);

            if (guild.RuleRoom is not null && channel.Id == guild.RuleRoom.DiscordId)
            {
                await _log.Log($"Removing {reaction.User.Value.Username} memberRole of {channel.Guild.Name}.", LogSeverity.Info);
                await RemovedRuleRoomReaction(reaction.User.Value, channel.Guild.EveryoneRole.Id);
            }

            if (selectionRoomGame is not null)
            {
                await _log.Log($"Removing {reaction.User.Value.Username} gameRole of {selectionRoomGame.Name} in {channel.Guild.Name} and reaction in activeRoom.", LogSeverity.Info);
                await RemoveSelectionRoomReaction(selectionRoomGame, reaction.User.Value, channel.Guild);
            }

            if (activeCheckRoomGame is not null)
            {
                // If user is not specified, the reaction has been already deleted
                if (!reaction.User.IsSpecified)
                {
                    return;
                }
                await _log.Log($"Removing {reaction.User.Value.Username} " +
                    $"activeRole of {activeCheckRoomGame.Name} in {channel.Guild.Name}.", LogSeverity.Info);
                await RemoveActiveRoomReaction(activeCheckRoomGame, reaction, message);
            }
        }

        /// <summary>
        /// Handles reaction removed for a message in the rule room. Removes all of the servers roles.
        /// </summary>
        /// <param name="user">User that removed the reaction</param>
        /// <returns>Nothing</returns>
        public static async Task RemovedRuleRoomReaction(IUser user, ulong everyoneRoleId)
        {
            await (user as SocketGuildUser).RemoveRolesAsync((user as SocketGuildUser).Roles.Where(r => r.Id != everyoneRoleId));
        }

        /// <summary>
        /// Handles reaction removed for a message in the selection room. Removes all of the game roles.
        /// </summary>
        /// <param name="game">Game whose reaction was removed from the selection message.</param>
        /// <param name="user">User that removed the reaction.</param>
        /// <returns>Nothing</returns>
        public static async Task RemoveSelectionRoomReaction(DAL.Model.Game game, IUser user, IGuild guild)
        {
            var roles = await _repo.FindAllRoleIdsByGame(game);

            await (user as IGuildUser).RemoveRolesAsync(roles);

            // Removes all reactions of user in games activeCheckRoom
            var discordActiveRoom = (ITextChannel)await guild.GetChannelAsync(game.ActiveCheckRoom.DiscordId);

            await RemoveReactionsByUserInChannel(discordActiveRoom, user);
        }

        /// <summary>
        /// Handles reaction removed for a message in the game active room. Removes all of the game active roles.
        /// </summary>
        /// <param name="game">Game whose reaction was removed from the active message.</param>
        /// <param name="user">User that removed the reaction.</param>
        /// <returns>Nothing</returns>
        public static async Task RemoveActiveRoomReaction(DAL.Model.Game game, SocketReaction reaction, IUserMessage message)
        {
            var roleId = message.MentionedRoleIds.SingleOrDefault();
            var user = reaction.User.Value as IGuildUser;

            // If MainActiveRole is removed, also remove all roles that don't need mod approval
            if (game.MainActiveRole.DisordId == roleId)
            {
                var roleIds = game.ActiveRoles.Where(r => !r.NeedsModApproval).Select(r => r.DisordId);
                await user.RemoveRolesAsync(roleIds);

                // Remove reactions to removed roles
                await RemoveReactionsByUserInChannel(reaction.Channel as ITextChannel, user, roleIds);
            }

            await user.RemoveRoleAsync(roleId);
        }

        private static bool CompareEmoteToEmoteEmoji(IEmote emote1, EmoteEmoji emoteEmoji2)
        {
            var emoteEmoji1 = EmoteParser.ParseEmote(emote1.ToString());
            return emoteEmoji1.Equals(emoteEmoji2);
        }

        private static async Task RemoveReactionsByUserInChannel(ITextChannel channel, IUser user, IEnumerable<ulong> roleIds = null)
        {
            var messages = await channel.GetMessagesAsync().FlattenAsync();
            foreach (var message in messages)
            {
                // Check if mentioned role in message is in roleIds
                if (roleIds is not null)
                {
                    if (!roleIds.Where(id => id == message.MentionedRoleIds.SingleOrDefault()).Any())
                        continue;
                }

                var reactedEmotes = message.Reactions.Keys;
                foreach (var emote in reactedEmotes)
                {
                    var users = await message.GetReactionUsersAsync(emote, int.MaxValue).FlattenAsync();
                    if (users.Where(u => u.Id == user.Id).Any())
                        await message.RemoveReactionAsync(emote, user);
                }
            }
        }
    }
}
