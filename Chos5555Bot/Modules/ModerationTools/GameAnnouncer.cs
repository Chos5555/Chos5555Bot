using System.Threading.Tasks;
using DAL;
using Discord;
using Discord.Commands;

namespace Chos5555Bot.Modules
{
    public static class GameAnnouncer
    {
        private static BotRepository repo = null;

        public static void InitAnnouncer(BotRepository repo)
        {
            GameAnnouncer.repo ??= repo;
        }

        public static async Task AnnounceGame(DAL.Model.Game game, Room selectionRoom, SocketCommandContext context)
        {
            IEmote reactEmote = game.ActiveEmote.Out();
            // TODO: throw error for user if not assigned
            var discordSelectionRoom = context.Guild.GetTextChannel(selectionRoom.DiscordId);

            var message = await discordSelectionRoom.SendMessageAsync($"{game.Name} {reactEmote}");

            game.SelectionMessageId = message.Id;

            await repo.UpdateGame(game);

            await message.AddReactionAsync(reactEmote);
        }
    }
}
