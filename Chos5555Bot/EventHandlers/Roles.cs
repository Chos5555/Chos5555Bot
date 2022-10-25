using DAL;
using Chos5555Bot.Services;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;

namespace Chos5555Bot.EventHandlers
{
    internal class Roles
    {
        private static BotRepository _repo;
        private static LogService _log;

        public static void InitRoles(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        public static async Task RoleUpdated(SocketRole oldRole, SocketRole newRole)
        {
            var role = await _repo.FindRole(oldRole);

            // If role is null, it's not in the DB => nothing to update
            if (role is null)
                return;

            role.Name = newRole.Name;
            await _repo.UpdateRole(role);

            await _log.Log($"Updated role {oldRole.Name} to now be {role.Name}", LogSeverity.Info);
        }
    }
}
