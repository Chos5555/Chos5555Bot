using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DAL;

namespace Chos5555Bot.Modules
{
    public class SetSelectionChannel : ModuleBase<SocketCommandContext>
    {
        BotRepository repo = new BotRepository();

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [Command("setSelectionChannel")]
        private async Task Command()
        {
            var guild = await CheckGuild();

            if (guild.SelectionRoom.IsSelectionRoom)
            {
                guild.SelectionRoom.IsSelectionRoom = false;
                await repo.UpdateRoom(guild.SelectionRoom);
            }

            var newRoom = new Room() { DiscordId = Context.Channel.Id, IsSelectionRoom = true };
            await repo.AddRoom(newRoom);

            guild.SelectionRoom = newRoom;
            await repo.UpdateGuild(guild);

            foreach (var role in guild.Roles)
            {
                await GameAnnouncer.AnnounceGame(role, guild.SelectionRoom, Context);
            }
        }

        private async Task<Guild> CheckGuild()
        {
            Guild guild = await repo.FindGuild(Context.Guild);
            if (guild is null)
            {
                guild = new Guild()
                {
                    DiscordId = Context.Guild.Id,
                };
                await repo.AddGuild(guild);
            }
            return guild;
        }
    }
}
