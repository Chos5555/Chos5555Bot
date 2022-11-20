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
using Chos5555Bot.Misc;
using System.Reactive.Linq;

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
        /// <returns>Nothing</returns>
        public async static Task ReactionAdded(Cacheable<IUserMessage, ulong> uncachedMessage, Cacheable<IMessageChannel, ulong> uncachedChannel, SocketReaction reaction)
        {
            var channel = await uncachedChannel.GetOrDownloadAsync() as SocketGuildChannel;

            // Ignore if reactions was added by bot
            if (channel.Guild.GetUser(reaction.UserId).IsBot)
                return;

            var guild = await _repo.FindGuild(channel.Guild.Id);
            var selectionRoomGame = await _repo.FindGameBySelectionMessage(reaction.MessageId);
            var modRoomGame = await _repo.FindGameByModRoom(channel.Id);
            var activeCheckRoomGame = await _repo.FindGameByActiveCheckRoom(channel.Id);
            var isStageChannel = await _repo.FindGuildByStageChannel(channel.Id) is not null;
            var questMessage = await _repo.FindQuestByQuestMessage(reaction.MessageId);
            var modQuestMessage = await _repo.FindQuestByModMessage(reaction.MessageId);

            var message = await uncachedMessage.GetOrDownloadAsync();
            IUser user;
            if (!reaction.User.IsSpecified)
            {
                user = channel.Guild.GetUser(reaction.UserId);
            }
            else
            {
                user = reaction.User.Value;
            }

            var removeReaction = false;

            // Call appropriate handler
            if (guild.RuleRoom is not null && channel.Id == guild.RuleRoom.DiscordId)
            {
                removeReaction = await AddedRuleRoomReaction(user, guild, reaction.Emote);
            }

            if (modRoomGame is not null)
            {
                removeReaction = await AddedModRoomReaction(message, channel.Guild, reaction, modRoomGame);
            }

            if (selectionRoomGame is not null)
            {
                removeReaction = await AddedSelectionRoomReaction(selectionRoomGame, user, reaction.Emote);
            }

            if (activeCheckRoomGame is not null)
            {
                removeReaction = await AddedActiveCheckRoomReaction(activeCheckRoomGame, user, channel.Guild, message, reaction.Emote);
            }

            if (isStageChannel)
            {
                removeReaction = await AddedStageChannelReaction(message, reaction, user as IGuildUser, channel);
            }

            if (questMessage is not null)
            {
                removeReaction = await AddedQuestMessageReaction(message, reaction, questMessage, channel.Guild);
            }

            if (modQuestMessage is not null)
            {
                removeReaction = await AddedModQuestMessageReaction(message, reaction, modQuestMessage, channel.Guild);
            }

            // Remove reaction if handler return true
            if (removeReaction)
            {
                await message.RemoveReactionAsync(reaction.Emote, user);
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
        private async static Task<bool> AddedRuleRoomReaction(IUser user, Guild guild, IEmote emote)
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
        private async static Task<bool> AddedModRoomReaction(IUserMessage message, IGuild guild, SocketReaction reaction, DAL.Model.Game game)
        {
            var emote = reaction.Emote;

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

            var checkmark = EmoteParser.ParseEmote("✅");
            var cross = EmoteParser.ParseEmote("❎");

            // Find activeCheckRoom message to which the original reaction was added, remove it, PM user, delete message in modRoom
            if (CompareEmoteToEmoteEmoji(emote, cross))
            {
                await _log.Log($"Request denied, removing reaction and DMing user {user.Username}", LogSeverity.Verbose);

                var activeCheckRoom = await guild.GetChannelAsync(game.ActiveCheckRoom.DiscordId) as IMessageChannel;
                var messages = await activeCheckRoom.GetMessagesAsync().FlattenAsync();
                var messageWithReaction = messages.Where(m => m.MentionedRoleIds.Contains(roleId)).SingleOrDefault();
                var roleReaction = messageWithReaction.Reactions.Keys.First();
                await messageWithReaction.RemoveReactionAsync(roleReaction, user);

                await user.SendMessageAsync($"Unfortunately your request to get role {role.Name} on server {guild.Name}" +
                    $" has been rejected. Please contact a moderator for reason of the rejection. Thanks.");
                await message.Channel.SendMessageAsync($"{(reaction.User.IsSpecified ? reaction.User.Value.Username : $"User with Id {reaction.UserId}")} " +
                    $"has rejected {user.Mention}'s request for role {role.Mention}.");
                await message.DeleteAsync();
                return false;
            }

            if (CompareEmoteToEmoteEmoji(emote, checkmark))
            {
                await _log.Log($"Request accepted, giving {role.Name} to {user.Username}", LogSeverity.Verbose);

                // Add role designated by reacted emote, PM user, delete message in modRoom
                await (user as SocketGuildUser).AddRoleAsync(role);

                await user.SendMessageAsync($"You have been given role {role.Name} on server {guild.Name}. Enjoy!");
                await message.Channel.SendMessageAsync($"{(reaction.User.IsSpecified ? reaction.User.Value.Username : $"User with Id {reaction.UserId}")} " +
                    $"has given user {user.Mention} role {role.Mention}.");
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
        private async static Task<bool> AddedSelectionRoomReaction(DAL.Model.Game game, IUser user, IEmote emote)
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
        private async static Task<bool> AddedActiveCheckRoomReaction(DAL.Model.Game game, IUser user, IGuild guild, IUserMessage message, IEmote emote)
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
        /// Handles reaction added to a message in a stage channel. If the reaction was added to a speak command message,
        /// was the sound icon and the user adding it has the speaker role, allow user to speak
        /// </summary>
        /// <param name="message">Message to which reaction was added</param>
        /// <param name="reaction">Reaction that was added</param>
        /// <param name="user">User that added the reaction</param>
        /// <param name="channel">Channel in which the reaction was added</param>
        /// <returns>Bool</returns>
        private async static Task<bool> AddedStageChannelReaction(IMessage message, SocketReaction reaction, IGuildUser user, IGuildChannel channel)
        {
            // Return if message is not a speak command
            if (!message.Content.ToLower().Contains("speak"))
            {
                return false;
            }

            // Return if added reaction is not sound icon
            var sound = EmoteParser.ParseEmote("🔊");
            if (!CompareEmoteToEmoteEmoji(reaction.Emote, sound))
            {
                return true;
            }

            var stageChannel = await _repo.FindRoomByTextOfStage(channel.Id);

            // Return if user doesn't have the speaker role
            if (!user.RoleIds.Contains(stageChannel.SpeakerRoleId))
            {
                return true;
            }

            await UserVoicePropertiesSetter.UpdateMute(message.Author as SocketGuildUser, false);

            return false;
        }

        private async static Task<bool> AddedQuestMessageReaction(IMessage message, SocketReaction reaction, Quest quest, IGuild guild)
        {
            var game = await _repo.FindGame(quest.GameName);

            // Setup emotes to compare to
            var take = EmoteParser.ParseEmote("✋");
            var checkmark = EmoteParser.ParseEmote("✅");
            var cross = EmoteParser.ParseEmote("❎");
            var delete = EmoteParser.ParseEmote("🗑");

            // Handle accordingly to what reaction has been used
            if (CompareEmoteToEmoteEmoji(reaction.Emote, take))
            {
                // Handle quest being taken
                // Send a message in the mod quest channel
                var modChannel = await guild.GetTextChannelAsync(game.ModQuestRoom.DiscordId);
                var modMessage = await modChannel.SendMessageAsync($"User {reaction.User.Value.Mention} has taken quest:\n{quest.Text}");

                // Update quest in DB
                quest.ModMessage = modMessage.Id;
                quest.TakerId = reaction.UserId;

                await _repo.UpdateQuest(quest);

                // Modify the content of the message to show it's been taken
                var content = $"{(await guild.GetUserAsync(quest.TakerId)).Mention} has taken this quest:\n{quest.Text}\n" +
                    $"To complete this quest click the ✅, then wait for a moderator to confirm.\n" +
                    $"If you want to cancel this quest, click ❎";
                await (message as IUserMessage).ModifyAsync(p => { p.Content = content; });

                // Remove all reactions
                await message.RemoveAllReactionsAsync();

                // Add delay because of rate limitting
                await Task.Delay(2000);

                // React with checkmark and cross
                await message.AddReactionAsync(new Emoji("✅"));
                // Add delay because of rate limitting
                await Task.Delay(2000);
                await message.AddReactionAsync(new Emoji("❎"));

                return false;
            }
            else if (CompareEmoteToEmoteEmoji(reaction.Emote, checkmark))
            {
                // Handle quest being completed
                // Remove reaction if a different user than the taker reacted
                if (reaction.UserId != quest.TakerId)
                    return true;

                var modChannel = await guild.GetTextChannelAsync(game.ModQuestRoom.DiscordId);
                var modMessage = await modChannel.GetMessageAsync(quest.ModMessage);

                if (modMessage is not null)
                    // Delete previous mod message saying quest was taken
                    await modMessage.DeleteAsync();

                // Create new message and react to it
                modMessage = await modChannel.SendMessageAsync($"User {reaction.User.Value.Mention} claims he completed quest:\n{quest.Text}\n" +
                    $"Is it really completed?");

                await modMessage.AddReactionAsync(new Emoji("✅"));
                // Add delay because of rate limitting
                await Task.Delay(2000);
                await modMessage.AddReactionAsync(new Emoji("❎"));
                
                // Update quest in DB
                quest.ModMessage = modMessage.Id;
                await _repo.UpdateQuest(quest);

                // Modify quest message
                var content = $"{reaction.User.Value.Mention} has completed quest:\n{quest.Text}\nWaiting for moderator confirmation.";
                await (message as IUserMessage).ModifyAsync(p => { p.Content = content; });

                // Remove all reactions
                await message.RemoveAllReactionsAsync();

                return false;
            }
            else if (CompareEmoteToEmoteEmoji(reaction.Emote, cross))
            {
                // Handle quest being cancelled
                // Remove reaction if a different user than the taker reacted
                if (reaction.UserId != quest.TakerId)
                    return true;

                var modChannel = await guild.GetTextChannelAsync(game.ModQuestRoom.DiscordId);
                var modMessage = await modChannel.GetMessageAsync(quest.ModMessage);

                if (modMessage is not null)
                    // Delete previous mod message saying quest was taken
                    await modMessage.DeleteAsync();

                // Create new message and react to it
                await modChannel.SendMessageAsync($"User {reaction.User.Value.Mention} cancelled quest:\n{quest.Text}\n" +
                    $"Making it available again.");

                // Update quest in DB
                quest.ModMessage = 0;
                quest.TakerId = 0;
                await _repo.UpdateQuest(quest);

                // Modify quest message back to before it was taken
                var content = $"{(await guild.GetUserAsync(quest.AuthorId)).Mention} has added a new quest:\n{quest.Text}\n" +
                    $"Press ✋ down below to claim this quest.";
                await (message as IUserMessage).ModifyAsync(p => { p.Content = content; });
                await message.RemoveAllReactionsAsync();

                // Add delay because of rate limitting
                await Task.Delay(2000);

                await message.AddReactionAsync(new Emoji("✋"));

                return false;
            }
            else if (CompareEmoteToEmoteEmoji(reaction.Emote, delete))
            {
                // Handle quest being deleted
                // Check it's the author who is deleting
                if (reaction.UserId != quest.AuthorId)
                    return true;

                // Delete quest from DB and quest message
                await message.DeleteAsync();
                await _repo.RemoveQuest(quest);

                return false;
            }

            // Remove reaction if it doesn't match any of the used emotes
            return true;
        }

        private async static Task<bool> AddedModQuestMessageReaction(IMessage message, SocketReaction reaction, Quest quest, IGuild guild)
        {
            // Setup emotes to compare to
            var checkmark = EmoteParser.ParseEmote("✅");
            var cross = EmoteParser.ParseEmote("❎");

            // Handle accordingly to what reaction has been used
            if (CompareEmoteToEmoteEmoji(reaction.Emote, checkmark))
            {
                // Handle qeust completion accepted
                // Find quest message and delete it
                var questChannel = await guild.GetTextChannelAsync(quest.QuestMessageChannelId);
                var questMessage = await questChannel.GetMessageAsync(quest.QuestMessage);
                await questMessage.DeleteAsync();

                // Modify mod message content and remove reactions
                var content = $"{reaction.User.Value.Mention} accepted completion by {(await guild.GetUserAsync(quest.TakerId)).Mention} " +
                    $"of quest: \n{quest.Text}\nQuest completed!";
                await (message as IUserMessage).ModifyAsync(p => { p.Content = content; });

                // Add delay because of rate limitting
                await Task.Delay(2000);

                await message.RemoveAllReactionsAsync();

                // Find user in DB or create and add it into DB
                var userId = message.MentionedUserIds.SingleOrDefault();
                var user = await _repo.FindUser(userId);
                if (user is null)
                {
                    user = new User()
                    {
                        DiscordId = userId
                    };
                    await _repo.AddUser(user);
                }

                // Increase users completed quest count for this game
                // Create new entry in completed quests it's this game is not there yet
                if (!user.CompletedQuests.Where(c => c.GameName == quest.GameName).Any())
                    user.CompletedQuests.Add(new CompletedQuests()
                    {
                        GameName = quest.GameName,
                        QuestCount = 0
                    });
                user.CompletedQuests.Where(c => c.GameName == quest.GameName).Single().QuestCount++;
                await _repo.UpdateUser(user);

                // Remove quest from DB
                await _repo.RemoveQuest(quest);

                return false;
            }
            else if (CompareEmoteToEmoteEmoji(reaction.Emote, cross))
            {
                // Handle quest completion denied
                // Modify quest message back to the non completed state
                var questChannel = await guild.GetTextChannelAsync(quest.QuestMessageChannelId);
                var questMessage = await questChannel.GetMessageAsync(quest.QuestMessage);

                // Modify the content of the message to show it's been taken
                var content = $"{await guild.GetUserAsync(quest.TakerId)} has taken this quest: \n{quest.Text}\n" +
                    $"To complete this quest click the ✅, then wait for a moderator to confirm.\n" +
                    $"If you want to cancel this quest, click ❎";
                await (questMessage as IUserMessage).ModifyAsync(p => { p.Content = content; });

                // Add complete and cancel reactions again
                await questMessage.AddReactionAsync(new Emoji("✅"));
                // Add delay because of rate limitting
                await Task.Delay(1000);
                await questMessage.AddReactionAsync(new Emoji("❎"));

                // Send DM to user telling him the completion was rejected
                var taker = await guild.GetUserAsync(message.MentionedUserIds.SingleOrDefault());
                await taker.SendMessageAsync($"Completion of quest:\n{quest.Text}\n was **rejected**, complete it and click the checkmark again.");

                // Modify mod message content and remove reactions
                content = $"{reaction.User.Value.Mention} rejected completion by {(await guild.GetUserAsync(quest.TakerId)).Mention} " +
                    $"of quest:\n{quest.Text}\nRemoving completed status.";
                await (message as IUserMessage).ModifyAsync(p => { p.Content = content; });

                // Add delay because of rate limitting
                await Task.Delay(2000);

                await message.RemoveAllReactionsAsync();

                return false;
            }

            // Remove reaction if it doesn't match any of the used emotes
            return true;
        }

        /// <summary>
        /// This method is the main handler for removing reactions. Checks in which room the reaction was added and calls the appropriate handler.
        /// </summary>
        /// <param name="uncachedMessage">Uncached message</param>
        /// <param name="uncachedChannel">Uncached channel</param>
        /// <param name="reaction">Reaction</param>
        /// <returns>Nothing</returns>
        public async static Task ReactionRemoved(Cacheable<IUserMessage, ulong> uncachedMessage, Cacheable<IMessageChannel, ulong> uncachedChannel, SocketReaction reaction)
        {
            var channel = await uncachedChannel.GetOrDownloadAsync() as SocketGuildChannel;
            var message = await uncachedMessage.GetOrDownloadAsync();

            // Ignore if reactions was added by bot
            if (channel.Guild.GetUser(reaction.UserId).IsBot)
                return;

            var guild = await _repo.FindGuild(channel.Guild.Id);
            var selectionRoomGame = await _repo.FindGameBySelectionMessage(reaction.MessageId);
            var activeCheckRoomGame = await _repo.FindGameByActiveCheckRoom(channel.Id);
            var isStageChannel = await _repo.FindGuildByStageChannel(channel.Id) is not null;
            IUser user;
            if (!reaction.User.IsSpecified)
            {
                user = channel.Guild.GetUser(reaction.UserId);
            }
            else
            {
                user = reaction.User.Value;
            }

            // Call appropriate handler
            if (guild.RuleRoom is not null && channel.Id == guild.RuleRoom.DiscordId)
            {
                await _log.Log($"Removing {reaction.User.Value.Username} memberRole of {channel.Guild.Name}.", LogSeverity.Info);
                await RemovedRuleRoomReaction(reaction.User.Value, channel.Guild.EveryoneRole.Id);
            }

            // Return if emoji added isn't sound icon
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
                await RemoveActiveRoomReaction(activeCheckRoomGame, reaction, message, channel.Guild);
            }

            if (isStageChannel)
            {
                await RemovedStageChannelReaction(message, reaction, user as IGuildUser, channel);
            }
        }

        /// <summary>
        /// Handles reaction removed for a message in the rule room. Removes all of the servers roles.
        /// </summary>
        /// <param name="user">User that removed the reaction</param>
        /// <returns>Nothing</returns>
        private async static Task RemovedRuleRoomReaction(IUser user, ulong everyoneRoleId)
        {
            await (user as SocketGuildUser).RemoveRolesAsync((user as SocketGuildUser).Roles.Where(r => r.Id != everyoneRoleId));
        }

        /// <summary>
        /// Handles reaction removed for a message in the selection room. Removes all of the game roles.
        /// </summary>
        /// <param name="game">Game whose reaction was removed from the selection message.</param>
        /// <param name="user">User that removed the reaction.</param>
        /// <returns>Nothing</returns>
        private async static Task RemoveSelectionRoomReaction(DAL.Model.Game game, IUser user, IGuild guild)
        {
            var roles = await _repo.FindAllRoleIdsByGame(game);

            // TODO: Investigate Discord.Net.HttpException: The server responded with error 50013: Missing Permissions
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
        private async static Task RemoveActiveRoomReaction(DAL.Model.Game game, SocketReaction reaction, IUserMessage message, IGuild guild)
        {
            var roleId = message.MentionedRoleIds.SingleOrDefault();
            var role = await _repo.FindRole(roleId);
            var user = reaction.User.Value as IGuildUser;

            // If user removed reaction and doesn't have the role yet (hadn't yet been approved by mod), remove the message
            if (role is not null && role.NeedsModApproval && !user.RoleIds.Contains(roleId))
            {
                var modAcceptChannel = await guild.GetChannelAsync(game.ModAcceptRoom.DiscordId) as IMessageChannel;
                var messages = await modAcceptChannel.GetMessagesAsync().FlattenAsync();
                await messages
                    .Where(m => m.MentionedUserIds.Contains(user.Id) && m.MentionedRoleIds.Contains(roleId) &&
                    m.Reactions.Keys.Count() == 2)
                    .SingleOrDefault()
                    .DeleteAsync();
                return;
            }

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

        /// <summary>
        /// Compares EmoteEmoji to IEmote
        /// </summary>
        /// <param name="emote1">Emote</param>
        /// <param name="emoteEmoji2">EmoteEmoji</param>
        /// <returns>bool</returns>
        private static bool CompareEmoteToEmoteEmoji(IEmote emote1, EmoteEmoji emoteEmoji2)
            => emoteEmoji2.Equals(emote1);

        /// <summary>
        /// Removes all reactions created by given user for all messages (or only messages containing mentions of role contained in roleIds if roleIds is passed) in given channel
        /// </summary>
        /// <param name="channel">Given channel</param>
        /// <param name="user">Given user</param>
        /// <param name="roleIds">Role Ids of selection messages from which reactions should be removed</param>
        /// <returns>Nothing</returns>
        private async static Task RemoveReactionsByUserInChannel(ITextChannel channel, IUser user, IEnumerable<ulong> roleIds = null)
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

                // Remove reaction of user, if user reacted to this message
                var reactedEmotes = message.Reactions.Keys;
                foreach (var emote in reactedEmotes)
                {
                    // Find all users that have reacted with current emote
                    var users = await message.GetReactionUsersAsync(emote, int.MaxValue).FlattenAsync();
                    if (users.Where(u => u.Id == user.Id).Any())
                        await message.RemoveReactionAsync(emote, user);
                }
            }
        }

        /// <summary>
        /// Handles reaction remove from a message in a stage channel. If the reaction was added to a speak command message,
        /// was the sound icon and the user adding it has the speaker role, deny user to speaking
        /// </summary>
        /// <param name="message">Message to which reaction was added</param>
        /// <param name="reaction">Reaction that was added</param>
        /// <param name="user">User that added the reaction</param>
        /// <param name="channel">Channel in which the reaction was added</param>
        /// <returns>Nothing</returns>
        private async static Task RemovedStageChannelReaction(IMessage message, SocketReaction reaction, IGuildUser user, IGuildChannel channel)
        {
            // Return if message is not a speak command
            if (!message.Content.ToLower().Contains("speak"))
            {
                return;
            }

            // Return if emoji added isn't sound icon
            var sound = EmoteParser.ParseEmote("🔊");
            if (!CompareEmoteToEmoteEmoji(reaction.Emote, sound))
            {
                return;
            }

            var stageChannel = await _repo.FindRoomByTextOfStage(channel.Id);

            // Return if user doesn't have the speaker role
            if (!user.RoleIds.Contains(stageChannel.SpeakerRoleId))
            {
                return;
            }

            // Mute user when speaker removed sound emote reaction
            await UserVoicePropertiesSetter.UpdateMute(message.Author as SocketGuildUser, true);

            // Delete message afterwards
            await message.DeleteAsync();
        }
    }
}
