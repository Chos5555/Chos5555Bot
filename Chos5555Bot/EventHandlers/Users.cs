using DAL;
using Chos5555Bot.Services;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;

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

        /// <summary>
        /// Sends a message to a designated channel when user leaves given guild
        /// </summary>
        /// <param name="discordGuild">Guild from which a user left</param>
        /// <returns>Nothing</returns>
        public static async Task UserLeft(SocketGuild discordGuild, SocketUser user)
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
    }
}
