using DAL;
using Chos5555Bot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;

namespace Chos5555Bot.EventHandlers
{
    internal class Guilds
    {
        private static BotRepository _repo;
        private static LogService _log;

        public static void InitGuilds(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        public static async Task LeftGuild(SocketGuild discordGuild)
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
