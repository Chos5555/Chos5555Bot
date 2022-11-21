using DAL;
using Chos5555Bot.Services;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;

namespace Chos5555Bot.EventHandlers
{
    /// <summary>
    /// Class containing handlers for events that related to a roles
    /// </summary>
    internal class Roles
    {
        private static BotRepository _repo;
        private static LogService _log;

        public static void InitRoles(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        public static Task RoleUpdated(SocketRole oldRole, SocketRole newRole)
        {
            _ = Task.Run(async () =>
            {
                await RoleUpdatedMain(oldRole, newRole);
            });

            return Task.CompletedTask;
        }

        public static Task RoleDeleted(SocketRole discordRole)
        {
            _ = Task.Run(async () =>
            {
                await RoleDeletedMain(discordRole);
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Updates roles name when role is changed
        /// </summary>
        /// <param name="oldRole">old discord role</param>
        /// <param name="newRole">new discord role</param>
        /// <returns>Nothing</returns>
        public async static Task RoleUpdatedMain(SocketRole oldRole, SocketRole newRole)
        {
            var role = await _repo.FindRole(oldRole);

            // If role is null, it's not in the DB => nothing to update
            if (role is null)
            {
                return;
            }

            role.Name = newRole.Name;
            await _repo.UpdateRole(role);

            await _log.Log($"Updated role {oldRole.Name} to now be {role.Name}", LogSeverity.Info);
        }

        /// <summary>
        /// Removes given role from DB when its deleted on discord
        /// </summary>
        /// <param name="discordRole">Discord role to be deleted</param>
        /// <returns>Nothing</returns>
        public async static Task RoleDeletedMain(SocketRole discordRole)
        {
            var role = await _repo.FindRole(discordRole);

            if (role is null)
            {
                return;
            }

            await _repo.RemoveRole(role);

            await _log.Log($"Role {discordRole.Name}:{discordRole.Guild.Name} was deleted from DB.", LogSeverity.Info);
        }
    }
}
