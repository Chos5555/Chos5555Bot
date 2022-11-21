using DAL;
using Chos5555Bot.Services;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;

namespace Chos5555Bot.EventHandlers
{
    /// <summary>
    /// Class containing handlers for events that related to a channel
    /// </summary>
    internal class Channels
    {
        private static BotRepository _repo;
        private static LogService _log;

        public static void InitChannels(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        public static Task ChannelDestroyed(SocketChannel discordChannel)
        {
            _ = Task.Run(async () =>
            {
                await ChannelDestroyedMain(discordChannel);
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes given channel from DB when its deleted on discord
        /// </summary>
        /// <param name="discordChannel">Given discord channel</param>
        /// <returns>Nothing</returns>
        public async static Task ChannelDestroyedMain(SocketChannel discordChannel)
        {
            // If the channel deleted was one of the stage channels, remove delete the other one
            // and remove from DB
            // TODO: Remove when TiV works on Discord.net
            var stage = await _repo.FindStageChannel(discordChannel.Id);
            var stageByText = await _repo.FindRoomByTextOfStage(discordChannel.Id);
            if (stage is not null || stageByText is not null)
            {
                var guild = (discordChannel as SocketGuildChannel).Guild;
                if (stageByText is null)
                {
                    await guild.GetChannel(stage.TextForStageId).DeleteAsync();
                }
                else
                {
                    await _repo.RemoveRoom(await _repo.FindRoom(stageByText.DiscordId));
                    await guild.GetChannel(stageByText.DiscordId).DeleteAsync();
                    return;
                }
            }

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
