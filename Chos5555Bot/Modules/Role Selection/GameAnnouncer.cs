using System.Threading.Tasks;
using DAL;
using Discord;
using Discord.Commands;

namespace Chos5555Bot.Modules
{
    public static class GameAnnouncer
    {
        private static readonly IEmote reactEmote = new Emoji("\U0001f495");

        private static BotRepository repo = null;

        public static void InitAnnouncer(BotRepository repo)
        {
            GameAnnouncer.repo ??= repo;
        }

        public static async Task AnnounceGame(Role role, Room selectionRoom, SocketCommandContext context)
        {            
            var discordSelectionRoom = context.Guild.GetTextChannel(selectionRoom.DiscordId);

            var message = await discordSelectionRoom.SendMessageAsync($"{role.Game.Name} {Emote.Parse(role.Game.Emote)}");

            role.Game.MessageId = message.Id;

            await repo.UpdateGame(role.Game);

            await message.AddReactionAsync(reactEmote);
        }
    }
}
