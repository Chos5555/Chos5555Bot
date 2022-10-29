using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DAL;
using Chos5555Bot.Services;

namespace Chos5555Bot.Modules
{
    /// <summary>
    /// Module class containing commands for setting selection guilds selection channel
    /// </summary>
    [Name("Manual Guild Management")]
    public class SetSelectionChannel : ModuleBase<SocketCommandContext>
    {
        private readonly BotRepository _repo;
        private readonly LogService _log;

        public SetSelectionChannel(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("setSelectionChannel")]
        [Summary("Sets the channel which this command is used in as selection channel for this guild and posts selection messages.")]
        private async Task Command()
        {
            // TODO: rework or check
            var guild = await CheckGuild();

            // Delete old SelectionRoom from DB
            if (guild.SelectionRoom is not null)
            {
                var oldRoom = guild.SelectionRoom;
                guild.SelectionRoom = null;
                await _repo.RemoveRoom(oldRoom);
            }

            var newRoom = new Room() { DiscordId = Context.Channel.Id };
            await _repo.AddRoom(newRoom);

            guild.SelectionRoom = newRoom;
            await _repo.UpdateGuild(guild);

            await _log.Log($"Set {Context.Guild.GetChannel(newRoom.DiscordId).Name} as selection channel for {Context.Guild.Name}", LogSeverity.Info);

            var games = await _repo.FingGamesByGuild(guild);
            foreach (var game in games)
            {
                await GameAnnouncer.AnnounceGame(game, guild.SelectionRoom, Context);
            }
        }

        private async Task<Guild> CheckGuild()
        {
            var guild = await _repo.FindGuild(Context.Guild);

            if (guild is null)
            {
                guild = new Guild()
                {
                    DiscordId = Context.Guild.Id,
                };
                await _repo.AddGuild(guild);
                await _log.Log($"Added guild {Context.Guild.Name} to the DB.", LogSeverity.Info);
            }
            return guild;
        }
    }
}
