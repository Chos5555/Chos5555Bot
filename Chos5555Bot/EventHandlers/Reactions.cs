using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DAL;

namespace Chos5555Bot.EventHandlers
{
    public class Reactions
    {
        public static async Task AddHandler(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            // TODO check if channel is selecction room
            // TODO figure out dependency injection of repo

            BotRepository repo = new BotRepository();
            var game = await repo.FindGameByMessage(reaction.MessageId);

            if (game is null)
            {
                return;
            }

            var role = await repo.FindRoleByGame(game);


            var message = await cachedMessage.GetOrDownloadAsync();
            var discordGuild = (message.Channel as SocketGuildChannel).Guild;
            IGuildUser user = discordGuild.GetUser(reaction.UserId);

            await user.AddRoleAsync(role.DisordId);
        }

        public static async Task RemoveHandler(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            // TODO

            BotRepository repo = new BotRepository();
            var game = await repo.FindGameByMessage(reaction.MessageId);

            if (game is null)
            {
                return;
            }

            var role = await repo.FindRoleByGame(game);

            var message = await cachedMessage.GetOrDownloadAsync();
            var discordGuild = (message.Channel as SocketGuildChannel).Guild;

            await (reaction.User.Value as IGuildUser).RemoveRoleAsync(discordGuild.GetRole(role.DisordId));
        }
    }
}
