using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DAL;

namespace Chos5555Bot.EventHandlers
{
    public class Reactions
    {
        public static async Task AddHandler(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cacheChannel, SocketReaction reaction)
        {
            // TODO figure out dependency injection of repo

            BotRepository repo = new BotRepository();

            var channel = cacheChannel
                .GetOrDownloadAsync().Result as SocketGuildChannel;
            var guild = await repo.FindGuildById(channel.Guild.Id);
            var game = await repo.FindGameBySelectionMessage(reaction.MessageId);
            var modRoomGame = await repo.FindGameByModRoom(channel.Id);

            if (channel.Id == guild.RuleRoom.DiscordId)
            {
                await AddedRuleRoomReaction(reaction.User.Value, guild);
            }

            if (modRoomGame is not null)
            {
                await AddedModRoomReaction(cachedMessage.GetOrDownloadAsync().Result, game, channel.Guild, reaction.Emote);
            }

            //TODO: Add selection room handler, which posts to mod room
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

        public static async Task AddedRuleRoomReaction(IUser user, Guild guild)
        {
            await (user as SocketGuildUser).AddRoleAsync(guild.MemberRole.DisordId);
        }

        public static async Task AddedModRoomReaction(IUserMessage message, DAL.Model.Game game, IGuild guild, IEmote emote)
        {
            //TODO: remove, add dependency injection
            BotRepository repo = new BotRepository();

            var userId = message.MentionedUserIds.GetEnumerator().Current;
            var user = await guild.GetUserAsync(userId);
            var role = await repo.FindRoleByGameAndGuild(emote, guild.Id);

            await (user as SocketGuildUser).AddRoleAsync(role.DisordId);
        }

        public static async Task RemoveHandler(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            BotRepository repo = new BotRepository();
            var game = await repo.FindGameBySelectionMessage(reaction.MessageId);

            if (game is null)
            {
                return;
            }

            var roles = await repo.FindAllRoleIdsByGame(game);

            await (reaction.User.Value as IGuildUser).RemoveRolesAsync(roles);
        }
    }
}
