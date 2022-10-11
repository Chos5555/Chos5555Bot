using System;
using System.Threading.Tasks;
using DAL;
using Discord;
using Discord.Commands;

namespace Chos5555Bot.Modules
{
    public static class GameAnnouncer
    {
        private static BotRepository repo;

        public static void InitAnnouncer(BotRepository repo)
        {
            GameAnnouncer.repo ??= repo;
        }

        // TODO: Announce active roles in active selection channel
        public static async Task AnnounceGame(DAL.Model.Game game, Room selectionRoom, SocketCommandContext context)
        {
            IEmote reactEmote = game.ActiveEmote.Out();
            // Send message to user if no selectionRoom is set
            if (selectionRoom is null)
            {
                await context.Channel.SendMessageAsync("Selection room has not yet been set. Couldn't post game selector message.");
                throw new NullReferenceException("Selection room has not yet been set.");
            }

            var discordSelectionRoom = context.Guild.GetTextChannel(selectionRoom.DiscordId);

            var message = await discordSelectionRoom.SendMessageAsync($"{game.Name} {reactEmote}");

            game.SelectionMessageId = message.Id;

            await repo.UpdateGame(game);

            await message.AddReactionAsync(reactEmote);
        }
    }
}
