using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DAL;

namespace Chos5555Bot.Modules
{
    public class SetSelectionChannel : ModuleBase<SocketCommandContext>
    {
        private readonly BotRepository repo;

        public SetSelectionChannel(BotRepository repo)
        {
            this.repo = repo;
        }

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [Command("setSelectionChannel")]
        private async Task Command()
        {
            var guild = await CheckGuild();
            Room oldRoom = null;

            // TODO: Fix old room deleting
            if (guild.SelectionRoom is not null)
            {
                oldRoom = guild.SelectionRoom;
            }

            var newRoom = new Room() { DiscordId = Context.Channel.Id };
            await repo.AddRoom(newRoom);

            guild.SelectionRoom = newRoom;
            await repo.UpdateGuild(guild);

            if (oldRoom is not null)
            {
                await repo.RemoveRoom(oldRoom);
            }
            
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
