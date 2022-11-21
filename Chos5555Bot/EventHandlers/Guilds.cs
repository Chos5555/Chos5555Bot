using DAL;
using Chos5555Bot.Services;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;

namespace Chos5555Bot.EventHandlers
{
    /// <summary>
    /// Class containing handlers for events that related to guilds
    /// </summary>
    internal class Guilds
    {
        private static BotRepository _repo;
        private static LogService _log;

        public static void InitGuilds(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        public static Task LeftGuild(SocketGuild discordGuild)
        {
            _ = Task.Run(async () =>
            {
                await LeftGuildMain(discordGuild);
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes given guild from DB when bot has left it
        /// </summary>
        /// <param name="discordGuild">Guild the bot has left</param>
        /// <returns>Nothing</returns>
        public async static Task LeftGuildMain(SocketGuild discordGuild)
        {
            var guild = await _repo.FindGuild(discordGuild);

            if (guild is null)
            {
                return;
            }

            await _repo.RemoveGuild(guild);

            await _log.Log($"Guild {discordGuild.Name} was deleted from DB.", LogSeverity.Info);
        }
    }
}
