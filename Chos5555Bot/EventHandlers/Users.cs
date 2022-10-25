using DAL;
using Chos5555Bot.Services;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;

namespace Chos5555Bot.EventHandlers
{
    internal class Users
    {
        private static BotRepository _repo;
        private static LogService _log;

        public static void InitUsers(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        public static async Task UserLeft(SocketGuild discordGuild, SocketUser user)
        {
            var guild = await _repo.FindGuild(discordGuild);

            if (guild.UserLeaveMessageRoomId != 0)
            {
                await (discordGuild.GetChannel(guild.UserLeaveMessageRoomId) as SocketTextChannel)
                    .SendMessageAsync($"User {user.Username}#{user.Discriminator} left this server.");
                await _log.Log($"User {user.Username}#{user.Discriminator} left {discordGuild.Name}.", LogSeverity.Info);
            }
        }
    }
}
