﻿using System.Threading.Tasks;
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
                removeReaction = await AddedActiveCheckRoomReaction(activeCheckRoomGame, reaction.User.Value, channel.Guild, reaction.Emote);
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

            var userId = message.MentionedUserIds.FirstOrDefault();
            var user = await guild.GetUserAsync(userId);
            var role = await repo.FindRoleByEmoteAndGame(emote, game);

            // Add role designated by reacted emote
            await (user as SocketGuildUser).AddRoleAsync(role.DisordId);
            return false;
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
        public static async Task<bool> AddedActiveCheckRoomReaction(DAL.Model.Game game, IUser user, IGuild guild, IEmote emote)
        {
            // Check if right emote was used
            // TODO: Rework, so user selects what messages he wants, mods only confirm
            if (CompareEmoteToEmoteEmoji(emote, game.ActiveEmote))
            {
                return true;
            }

            // If there are no mod role, add all active roles
            if (game.ModAcceptRoles.Count == 0)
            {
                await (user as SocketGuildUser).AddRolesAsync(
                    game.ActiveRoles
                    .Select(r => r.DisordId));
                return false;
            } else
            // If there is a mod role, post into mod room and react with all active role emotes for that game
            {
                var message = $"{user} wants to join you in {game.Name}, select the role you want to give them:\n";
                foreach (var role in game.ActiveRoles)
                {
                    message += $"{role.ChoiceEmote} for role {guild.GetRole(role.DisordId).Name}\n";
                }

                var sentMessage = await (guild as SocketGuild).GetTextChannel(game.ModAcceptRoom.DiscordId).SendMessageAsync(message);

                await sentMessage.AddReactionsAsync(game.ActiveRoles.Select(r => r.ChoiceEmote.Out()));

                return false;
            }
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
            //TODO: add dependency injection
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
            await (user as IGuildUser).RemoveRolesAsync(game.ActiveRoles.Select(r => r.DisordId));
        }

        private static bool CompareEmoteToEmoteEmoji(IEmote emote1, EmoteEmoji emoteEmoji2)
        {
            var emoteEmoji1 = EmoteParser.ParseEmote(emote1.ToString());
            return emoteEmoji1 == emoteEmoji2;
        }
    }
}
