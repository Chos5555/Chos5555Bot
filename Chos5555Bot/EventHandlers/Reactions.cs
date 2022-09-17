using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DAL;
using System.Linq;

namespace Chos5555Bot.EventHandlers
{
    public class Reactions
    {
        public static async Task AddHandler(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cacheChannel, SocketReaction reaction)
        {
            // TODO figure out dependency injection of repo
            // TODO: remove reactions that are not correct in rule and selection rooms

            BotRepository repo = new BotRepository();

            var channel = cacheChannel
                .GetOrDownloadAsync().Result as SocketGuildChannel;
            var guild = await repo.FindGuildById(channel.Guild.Id);
            var selectionRoomGame = await repo.FindGameBySelectionMessage(reaction.MessageId);
            var modRoomGame = await repo.FindGameByModRoom(channel.Id);
            var activeCheckRoomGame = await repo.FindGameByActiveCheckRoom(channel.Id);

            if (channel.Id == guild.RuleRoom.DiscordId)
            {
                await AddedRuleRoomReaction(reaction.User.Value, guild);
            }

            if (modRoomGame is not null)
            {
                await AddedModRoomReaction(cachedMessage.GetOrDownloadAsync().Result, channel.Guild, reaction.Emote);
            }

            if (selectionRoomGame is not null)
            {
                await AddedSelectionRoomReaction(selectionRoomGame, reaction.User.Value);
            }

            if (activeCheckRoomGame is not null)
            {
                await AddedActiveCheckRoomReaction(activeCheckRoomGame, reaction.User.Value, channel.Guild);
            }
        }

        public static async Task AddedRuleRoomReaction(IUser user, Guild guild)
        {
            // TODO: add check if emote is checkmark
            await (user as SocketGuildUser).AddRoleAsync(guild.MemberRole.DisordId);
        }

        public static async Task AddedModRoomReaction(IUserMessage message, IGuild guild, IEmote emote)
        {
            //TODO: remove, add dependency injection
            BotRepository repo = new BotRepository();

            var userId = message.MentionedUserIds.FirstOrDefault();
            var user = await guild.GetUserAsync(userId);
            var role = await repo.FindRoleByGameAndGuild(emote, guild.Id);

            await (user as SocketGuildUser).AddRoleAsync(role.DisordId);
        }

        public static async Task AddedSelectionRoomReaction(DAL.Model.Game game, IUser user)
        {
            // TODO: add check if emote is game.Emote
            await (user as SocketGuildUser).AddRoleAsync(game.GameRole.DisordId);
        }

        public static async Task AddedActiveCheckRoomReaction(DAL.Model.Game game, IUser user, IGuild guild)
        {
            if (game.ModAcceptRoles.Count == 0)
            {
                await (user as SocketGuildUser).AddRolesAsync(
                    game.ActiveRoles
                    .Select(r => r.DisordId));
            } else
            {
                var message = $"{user} wants to join you in {game.Name}, select the role you want to give them:\n";
                foreach (var role in game.ActiveRoles)
                {
                    message += $"{role.Emote} for role {guild.GetRole(role.DisordId).Name}\n";
                }

                var sentMessage = await (guild as SocketGuild).GetTextChannel(game.ModAcceptRoom.DiscordId).SendMessageAsync(message);

                await sentMessage.AddReactionsAsync(game.ActiveRoles.Select(r => r.Emote));
            }
        }

        public static async Task RemoveHandler(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            // TODO: add dependency injection
            // TODO: add handlers for removing role when in rule room, selection room or active room
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
