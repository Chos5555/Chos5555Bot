using DAL;
using Chos5555Bot.Services;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using Chos5555Bot.Misc;
using DAL.Model;
using Game = DAL.Model.Game;
using System;

namespace Chos5555Bot.EventHandlers
{
    /// <summary>
    /// Class containing handlers for events that related to a user
    /// </summary>
    internal class Users
    {
        private static BotRepository _repo;
        private static LogService _log;

        public static void InitUsers(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        public static Task UserLeft(SocketGuild discordGuild, SocketUser user)
        {
            _ = Task.Run(async () =>
            {
                await UserLeftMain(discordGuild, user);
            });

            return Task.CompletedTask;
        }

        public static Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            _ = Task.Run(async () =>
            {
                await UserVoiceStateUpdatedMain(user, oldState, newState);
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sends a message to a designated channel when user leaves given guild
        /// </summary>
        /// <param name="discordGuild">Guild from which a user left</param>
        /// <returns>Nothing</returns>
        public async static Task UserLeftMain(SocketGuild discordGuild, SocketUser user)
        {
            var guild = await _repo.FindGuild(discordGuild);

            // Only send message if UserLeaveMessageRoomId is set
            if (guild.UserLeaveMessageRoomId != 0)
            {
                await (discordGuild.GetChannel(guild.UserLeaveMessageRoomId) as SocketTextChannel)
                    .SendMessageAsync($"User {user.Username}#{user.Discriminator} left this server.");
                await _log.Log($"User {user.Username}#{user.Discriminator} left {discordGuild.Name}.", LogSeverity.Info);
            }
        }

        /// <summary>
        /// Handle when users voice state is updated.
        /// When user left a stage channel and is muted, because he got permission to speak during the stage,
        /// unmute him.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="oldState"></param>
        /// <param name="newState"></param>
        /// <returns></returns>
        public async static Task UserVoiceStateUpdatedMain(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            await TrackActivity(user, oldState, newState);
            await UnmuteAfterStage(user, oldState, newState);
        }

        public async static Task UnmuteAfterStage(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            // If voice channel is null on both states, return
            var voiceChannel = oldState.VoiceChannel ?? newState.VoiceChannel;
            if (voiceChannel is null)
            {
                return;
            }

            // If guild is null, return
            var discordGuild = voiceChannel.Guild;
            if (discordGuild is null)
            {
                return;
            }

            // If guild is not in DB, return
            var guild = await _repo.FindGuild(discordGuild);
            if (guild is null)
            {
                return;
            }

            var guildUser = discordGuild.GetUser(user.Id);

            // If user has switched channel (or joined or left voice)
            if (oldState.VoiceChannel != newState.VoiceChannel)
            {
                // If user left voice
                if (newState.VoiceChannel is null)
                {
                    // You can't unmute user if he's not in voice
                    return;
                }

                // Unmute user if he's muted and changed channel (either going from a stage chanel
                // or joined a voice channel first time after being in a stage channel and left voice)
                if (guildUser.IsMuted)
                {
                    // Find channel in DB
                    var channel = await _repo.FindRoom(newState.VoiceChannel.Id);

                    // Only unmute if channel in not a stage channel
                    if (channel is null || channel.SpeakerRoleId == 0)
                    {
                        await UserVoicePropertiesSetter.UpdateMute(guildUser, false);
                    }
                }
            }
        }

        public async static Task TrackActivity(SocketUser discordUser, SocketVoiceState oldState, SocketVoiceState newState)
        {
            // Only update LastAppearance if user left voice channel
            if (newState.VoiceChannel is not null || oldState.VoiceChannel == newState.VoiceChannel)
                return;

            // Return if channel isn't in a category
            var categoryId = (oldState.VoiceChannel as INestedChannel).CategoryId;
            if (!categoryId.HasValue)
                return;

            // Return if channel isn't a game channel
            var game = await _repo.FindGameByCategoryId(categoryId.Value);
            if (game is null)
                return;

            // If game doesn't have tracking enabled, return
            if (!game.TrackActivity)
                return;

            // Remove active roles of inactive users
            await RemoveInactiveUsers(game, oldState.VoiceChannel.Guild);

            var user = await _repo.FindUser(discordUser.Id);

            // Create a new user if none is found
            if (user is null)
            {
                user = new User()
                {
                    DiscordId = discordUser.Id,
                };
                await _repo.AddUser(user);
            }

            var gameActivity = await _repo.FindUsersGameActivity(user, game.Name);
            
            // Create new game activity if none is found
            gameActivity ??= new GameActivity()
                {
                    GameName = game.Name,
                };

            // Set LastAppearance
            gameActivity.LastAppearance = DateTime.UtcNow;

            await _repo.UpdateUser(user);
        }

        public async static Task RemoveInactiveUsers(Game game, IGuild guild)
        {
            // Only check once every day to not slow down bot
            if ((DateTime.UtcNow - game.LastActivityCheck).TotalDays > 1)
                return;

            // Get all users with activity for game
            var users = _repo.FindAllUsersActivityForGame(game);

            foreach (var (user, activity) in users)
            {
                // If LastAppearance is less than RemoveAfter from now, return
                if (activity.LastAppearance - DateTime.UtcNow > game.RemoveAfter)
                    continue;

                // Find activerole announce message
                var activeCheckChannel = await guild.GetTextChannelAsync(game.ActiveCheckRoom.DiscordId);
                var message = await MessageFinder.FindAnnouncedMessage(game.MainActiveRole, activeCheckChannel);

                // Remove users reaction on MainActiveRole announce message, this will remove all other active roles in reactions handler
                await message.RemoveReactionAsync(game.MainActiveRole.ChoiceEmote.Out(), user.DiscordId);

                // Remove GameActivity from user when MainActiveRoleIsRemoved
                user.GameActivities.Remove(activity);
                await _repo.UpdateUser(user);
                await _repo.RemoveGameActivity(activity);
            }
        }
    }
}
