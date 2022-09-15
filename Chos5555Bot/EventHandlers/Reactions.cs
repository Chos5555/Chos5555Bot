using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DAL;

namespace Chos5555Bot.EventHandlers
{
    public class Reactions
    {
        //TODO: Add handler for mod room, rule room
        public static async Task AddHandler(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            // TODO check if channel is selecction room
            // TODO figure out dependency injection of repo

            BotRepository repo = new BotRepository();
            var game = await repo.FindGameByMessage(reaction.MessageId);

            if (game is null)
            {
                return;
            }

            var role = await repo.FindGameRoleByGame(game);


            var message = await cachedMessage.GetOrDownloadAsync();
            var discordGuild = (message.Channel as SocketGuildChannel).Guild;
            IGuildUser user = discordGuild.GetUser(reaction.UserId);

            await user.AddRoleAsync(role.DisordId);
        }

        public static async Task RemoveHandler(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            // TODO remove ALL roles of a game

            BotRepository repo = new BotRepository();
            var game = await repo.FindGameByMessage(reaction.MessageId);

            if (game is null)
            {
                return;
            }

            var roles = await repo.FindAllRoleIdsByGame(game);

            await (reaction.User.Value as IGuildUser).RemoveRolesAsync(roles);
        }
    }
}
