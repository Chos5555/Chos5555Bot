using DAL;
using Chos5555Bot.Services;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;

namespace Chos5555Bot.EventHandlers
{
    internal class Channels
    {
        private static BotRepository _repo;
        private static LogService _log;

        public static void InitChannels(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        public static async Task ChannelDestroyed(SocketChannel discordChannel)
        {
            var channel = await _repo.FindRoom(discordChannel);

            if (channel is null)
            {
                return;
            }

            await _repo.RemoveRoom(channel);

            var guildChannel = discordChannel as SocketGuildChannel;
            await _log.Log($"Channel {guildChannel.Name}:{guildChannel.Guild.Name} was deleted from DB.", LogSeverity.Info);
        }
    }
}
}
