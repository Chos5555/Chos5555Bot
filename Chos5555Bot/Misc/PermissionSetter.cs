using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chos5555Bot.Misc
{
    /// <summary>
    /// Class containing methods that alter permission of a discord channel
    /// </summary>
    internal class PermissionSetter
    {
        /// <summary>
        /// Makes <paramref name="channel"> visible for <paramref name="showRole">
        /// and hides it for <paramref name="hideRole"/>
        /// </summary>
        /// <param name="showRole">Discord role</param>
        /// <param name="hideRole">Discord role</param>
        /// <param name="channel">Discord channel</param>
        /// <returns>Nothing</returns>
        public async static Task SetShownOnlyForRole(IRole showRole, IRole hideRole, IGuildChannel channel)
        {
            await SetHiddenForRole(hideRole, channel);
            await SetShownForRole(showRole, channel);
        }

        /// <summary>
        /// Makes <paramref name="channel"/> visible for all roles from <paramref name="showRoles"/>
        /// and hides it for <paramref>hideRole</paramref>
        /// </summary>
        /// <param name="showRole">Discord roles</param>
        /// <param name="hideRole">Discord role</param>
        /// <param name="channel">Discord channel</param>
        /// <returns>Nothing</returns>
        public async static Task SetShownForRoles(ICollection<IRole> showRoles, IRole hideRole, IGuildChannel channel)
        {
            await SetHiddenForRole(hideRole, channel);

            foreach (var role in showRoles)
            {
                await SetShownForRole(role, channel);
            }
        }

        /// <summary>
        /// Sets <paramref name="channel"/> hidden from <paramref name="role">
        /// </summary>
        /// <param name="role">Discord role</param>
        /// <param name="channel">Discord channel</param>
        /// <returns>Nothing</returns>
        public async static Task SetHiddenForRole(IRole role, IGuildChannel channel)
        {
            // Deny viewing channel for given role
            await UpdateHelper(role, channel, "viewChannel", PermValue.Deny);
        }

        /// <summary>
        /// Sets <paramref name="channel"/> shown for <paramref name="role"/>
        /// </summary>
        /// <param name="role">Discord role</param>
        /// <param name="channel">Discord channel</param>
        /// <returns>Nothing</returns>
        public async static Task SetShownForRole(IRole role, IGuildChannel channel)
        {
            // Allow viewing channel for given role
            await UpdateHelper(role, channel, "viewChannel", PermValue.Allow);
        }

        /// <summary>
        /// Updates the addReaction permission of <paramref name="channel"/> for <paramref name="role"/> with value <paramref name="value"/>
        /// </summary>
        /// <param name="role">Discord role</param>
        /// <param name="channel">Discord channel</param>
        /// <param name="value">Value</param>
        /// <returns>Nothing</returns>
        public async static Task UpdateAddReaction(IRole role, IGuildChannel channel, PermValue value)
        {
            // Stops users with role from adding new reactions, they can still react with the ones already there
            await UpdateHelper(role, channel, "addReactions", value);
        }

        /// <summary>
        /// Updates the viewChannel permission of <paramref name="channel"/> for <paramref name="role"/> with value <paramref name="value"/>
        /// </summary>
        /// <param name="role">Discord role</param>
        /// <param name="channel">Discord channel</param>
        /// <param name="value">Value</param>
        /// <returns>Nothing</returns>
        public async static Task UpdateViewChannel(IRole role, IGuildChannel channel, PermValue value)
        {
            await UpdateHelper(role, channel, "viewChannel", value);
        }

        /// <summary>
        /// Updates given <paramref name="permission"/> with <paramref name="value"/> for <paramref name="role"/> for <paramref name="channel"/>
        /// </summary>
        /// <param name="role">Discord role</param>
        /// <param name="channel">Discord channel</param>
        /// <param name="permission">Name of the permission to be changed</param>
        /// <param name="value">Value</param>
        /// <returns>Nothing</returns>
        private async static Task UpdateHelper(IRole role, IGuildChannel channel, string permission, PermValue value)
        {
            var rolePerms = channel.GetPermissionOverwrite(role);

            if (!rolePerms.HasValue)
                rolePerms = OverwritePermissions.InheritAll;

            // Removes old permission for role
            await channel.RemovePermissionOverwriteAsync(role);

            // Find which permission to modify
            switch (permission)
            {
                case "viewChannel":
                    rolePerms.Value.Modify(viewChannel: value);
                    break;
                case "addReactions":
                    rolePerms.Value.Modify(addReactions: value);
                    break;
            }

            // Adds updated permissions for role
            await channel.AddPermissionOverwriteAsync(role, rolePerms.Value);
        }
    }
}
