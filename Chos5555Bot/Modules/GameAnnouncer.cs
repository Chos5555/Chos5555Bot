using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;
using Discord;
using Discord.Commands;

namespace Chos5555Bot.Modules
{
    public static class GameAnnouncer
    {
        public static readonly IEmote reactEmote = Discord.Emote.Parse("<3");

        public static async Task AnnounceGame(Role role, Room selectionRoom, SocketCommandContext context)
        {
            var discordSelectionRoom = context.Guild.GetTextChannel(selectionRoom.DiscordId);

            var message = await discordSelectionRoom.SendMessageAsync($"{role.Game.Name} {Emote.Parse(role.Game.Emote)}");

            role.Game.MessageId = message.Id;

            BotRepository repo = new BotRepository();
            await repo.UpdateGame(role.Game);

            await message.AddReactionAsync(reactEmote);
        }
    }
}
