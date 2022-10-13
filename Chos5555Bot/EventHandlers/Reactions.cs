using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DAL;
using System.Linq;
using DAL.Model;
using DAL.Misc;

namespace Chos5555Bot.EventHandlers
{
    /// <summary>
    /// Class <c>Reactions</c> contains handlers for adding/removing reactions and other helper methods
    /// </summary>
    public class Reactions
    {
        // TODO: Add logging

        /// <summary>
        /// This method is the main handler for added reactions. Checks in which room the reaction was added and calls the appropriate handler.
        /// Removes the reaction if a wrong reaction was added to a selection message.
        /// </summary>
        /// <param name="cachedMessage">Uncached message</param>
        /// <param name="uncachedChannel">Uncached channel</param>
        /// <param name="reaction">Reaction</param>
        /// <returns>Task</returns>
        public static async Task AddHandler(Cacheable<IUserMessage, ulong> uncachedMessage, Cacheable<IMessageChannel, ulong> uncachedChannel, SocketReaction reaction)
        {
            // TODO figure out dependency injection of repo

            BotRepository repo = new BotRepository();

            var channel = await uncachedChannel.GetOrDownloadAsync() as SocketGuildChannel;

            // Ignore if reactions was added by bot
            if (channel.Guild.GetUser(reaction.UserId).IsBot)
                return;

            var guild = await repo.FindGuild(channel.Guild.Id);
            var selectionRoomGame = await repo.FindGameBySelectionMessage(reaction.MessageId);
            var modRoomGame = await repo.FindGameByModRoom(channel.Id);
            var activeCheckRoomGame = await repo.FindGameByActiveCheckRoom(channel.Id);
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
            // Check if right emote was used
            var checkmark = EmoteParser.ParseEmote("✅");
            if (CompareEmoteToEmoteEmoji(emote, checkmark))
            {
                return true;
            }

            await (user as SocketGuildUser).AddRoleAsync(guild.MemberRole.DisordId);
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
            // If there is only 1 of the new emote, it was not given by the bot, thus is not a valid role emote
            if (message.Reactions[emote].ReactionCount == 1)
            {
                return true;
            }

            //TODO: add dependency injection
            BotRepository repo = new BotRepository();

            var userId = message.MentionedUserIds.SingleOrDefault();
            var user = await guild.GetUserAsync(userId);
            var roleId = message.MentionedRoleIds.SingleOrDefault();
            var role = guild.GetRole(roleId);

            // Find activeCheckRoom message to which the original reaction was added, remove it, PM user, delete message in modRoom
            if (emote == new Emoji("❎"))
            {
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
            // Check if right emote was used
            if (CompareEmoteToEmoteEmoji(emote, game.ActiveEmote))
            {
                return true;
            }

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
            //TODO: add dependency injection
            BotRepository repo = new BotRepository();

            // Find mentioned role in message
            var roleId = message.MentionedRoleIds.SingleOrDefault();
            var role = await repo.FindRole(roleId);
            var discordRole = guild.GetRole(roleId);

            if (!CompareEmoteToEmoteEmoji(emote, role.ChoiceEmote))
                return true;

            // If user doesn't have games mainActiveRole and the role doesn't need mod approval, remove reaction
            // (a.k.a. it's a secondary role that will only unlock channels if you have mainActiveRole)
            if (!(user as SocketGuildUser).Roles
                .Contains(guild.GetRole(game.MainActiveRole.DisordId))
                && !role.NeedsModApproval)
                return true;

            // TODO: Check if there's any way to hide other role messages if no modAcceptRoles are set
            // (Or send optional role messages only after modRole has been set?)

            // If there are no mod role, add all active roles
            if (game.ModAcceptRoles.Count == 0)
            {
                await (user as SocketGuildUser).AddRolesAsync(
                    game.ActiveRoles
                    .Select(r => r.DisordId));
                return false;
            }

            // If there is a mod role, post into mod room and react with yes or no emotes
            if (role.NeedsModApproval)
            {
                var messageText = $"{user.Mention} wants the role {discordRole.Mention}, should I give it to them?";

                var modMessage = await (guild as SocketGuild).GetTextChannel(game.ModAcceptRoom.DiscordId).SendMessageAsync(messageText);

                await modMessage.AddReactionAsync(new Emoji("✅"));
                await modMessage.AddReactionAsync(new Emoji("❎"));

                return false;
            }

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
        public static async Task RemoveHandler(Cacheable<IUserMessage, ulong> uncachedMessage, Cacheable<IMessageChannel, ulong> uncachedChannel, SocketReaction reaction)
        {
            // TODO: add dependency injection
            BotRepository repo = new BotRepository();

            var channel = await uncachedChannel.GetOrDownloadAsync() as SocketGuildChannel;

            // Ignore if reactions was added by bot
            if (channel.Guild.GetUser(reaction.UserId).IsBot)
                return;

            var guild = await repo.FindGuild(channel.Guild.Id);
            var selectionRoomGame = await repo.FindGameBySelectionMessage(reaction.MessageId);
            var activeCheckRoomGame = await repo.FindGameByActiveCheckRoom(channel.Id);

            if (guild.RuleRoom is not null && channel.Id == guild.RuleRoom.DiscordId)
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

        /// <summary>
        /// Handles reaction removed for a message in the rule room. Removes all of the servers roles.
        /// </summary>
        /// <param name="user">User that removed the reaction</param>
        /// <returns>Nothing</returns>
        public static async Task RemovedRuleRoomReaction(IUser user)
        {
            await (user as SocketGuildUser).RemoveRolesAsync((user as SocketGuildUser).Roles);
        }

        /// <summary>
        /// Handles reaction removed for a message in the selection room. Removes all of the game roles.
        /// </summary>
        /// <param name="game">Game whose reaction was removed from the selection message.</param>
        /// <param name="user">User that removed the reaction.</param>
        /// <returns>Nothing</returns>
        public static async Task RemoveSelectionRoomReaction(DAL.Model.Game game, IUser user)
        {
            // TODO: remove reactions of user in game.activeCheckRoom
            // TODO: add dependency injection
            BotRepository repo = new BotRepository();

            var roles = await repo.FindAllRoleIdsByGame(game);

            await (user as IGuildUser).RemoveRolesAsync(roles);
        }

        /// <summary>
        /// Handles reaction removed for a message in the game active room. Removes all of the game active roles.
        /// </summary>
        /// <param name="game">Game whose reaction was removed from the active message.</param>
        /// <param name="user">User that removed the reaction.</param>
        /// <returns>Nothing</returns>
        public static async Task RemoveActiveRoomReaction(DAL.Model.Game game, IUser user)
        {
            // TODO: Remove only the role which was unselected
            await (user as IGuildUser).RemoveRolesAsync(game.ActiveRoles.Select(r => r.DisordId));
        }

        private static bool CompareEmoteToEmoteEmoji(IEmote emote1, EmoteEmoji emoteEmoji2)
        {
            var emoteEmoji1 = EmoteParser.ParseEmote(emote1.ToString());
            return emoteEmoji1 == emoteEmoji2;
        }
    }
}
