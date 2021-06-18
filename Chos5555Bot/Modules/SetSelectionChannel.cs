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
            Console.WriteLine("inside command\n");

            var guild = await CheckGuild();

            guild.SelectionRoom = new Room() { DiscordId = Context.Channel.Id };
            await repo.UpdateGuild(guild);

            Console.WriteLine("created guild\n");

            foreach (var role in guild.Roles)
            {
                await GameAnnouncer.AnnounceGame(role, guild.SelectionRoom);
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
