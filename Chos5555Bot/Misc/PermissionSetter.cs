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
        /// Updates the addReaction permission of <paramref name="channel"/> for <paramref name="role"/> with value <paramref name="value"/>
        /// </summary>
        /// <param name="role">Discord role</param>
        /// <param name="channel">Discord channel</param>
        /// <param name="value">Value</param>
        /// <returns>Nothing</returns>
        public async static Task UpdateAddReaction(IRole role, IGuildChannel channel, PermValue value)
        {
            // Stops users with role from adding new reactions, they can still react with the ones already there
            await UpdateRoleHelper(role, channel, "addReactions", value);
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
            await UpdateRoleHelper(role, channel, "viewChannel", value);
        }

        /// <summary>
        /// Updates the speak permission of <paramref name="channel"/> for <paramref name="user"/> with value <paramref name="value"/>
        /// </summary>
        /// <param name="user">Discord user</param>
        /// <param name="channel">Discord channel</param>
        /// <param name="value">Value</param>
        /// <returns>Nothing</returns>
        public async static Task UpdateSpeakUser(IUser user, IGuildChannel channel, PermValue value)
        {
            await UpdateUserHelper(user, channel, "speak", value);
        }

        /// <summary>
        /// Updates the sendMessages permission of <paramref name="channel"/> for <paramref name="user"/> with value <paramref name="value"/>
        /// </summary>
        /// <param name="user">Discord user</param>
        /// <param name="channel">Discord channel</param>
        /// <param name="value">Value</param>
        /// <returns>Nothing</returns>
        public async static Task UpdateSendMessagesUser(IUser user, IGuildChannel channel, PermValue value)
        {
            await UpdateUserHelper(user, channel, "sendMessages", value);
        }

        /// <summary>
        /// Makes <paramref name="channel"> visible for <paramref name="enableRole">
        /// and hides it for <paramref name="disableRole"/>
        /// </summary>
        /// <param name="enableRole">Discord role</param>
        /// <param name="disableRole">Discord role</param>
        /// <param name="channel">Discord channel</param>
        /// <returns>Nothing</returns>
        public async static Task EnableViewOnlyForRole(IRole enableRole, IRole disableRole, IGuildChannel channel)
        {
            await EnableOnlyForRole(enableRole, disableRole, channel, "viewChannel");
        }

        /// <summary>
        /// Enables speak for <paramref name="enableRole"/>
        /// and disables it for <paramref name="disableRole"/> for <paramref name="channel"/>
        /// </summary>
        /// <param name="enableRole">Discord roles</param>
        /// <param name="disableRole">Discord role</param>
        /// <param name="channel">Discord channel</param>
        /// <returns>Nothing</returns>
        public async static Task EnableSpeakOnlyForRole(IRole enableRole, IRole disableRole, IGuildChannel channel)
        {
            await EnableOnlyForRole(enableRole, disableRole, channel, "speak");
        }

        /// <summary>
        /// Makes <paramref name="channel"> visible for all <paramref name="enableRoles">
        /// and hides it for <paramref name="disableRole"/>
        /// </summary>
        /// <param name="enableRoles">Discord roles</param>
        /// <param name="disableRole">Discord role</param>
        /// <param name="channel">Discord channel</param>
        /// <returns>Nothing</returns>
        public async static Task EnableViewOnlyForRoles(ICollection<IRole> enableRoles, IRole disableRole, IGuildChannel channel)
        {
            await EnableOnlyForRoles(enableRoles, disableRole, channel, "viewChannel");
        }

        /// <summary>
        /// Makes <paramref name="channel"> visible for all <paramref name="enableRoles">
        /// and hides it for <paramref name="disableRole"/>
        /// </summary>
        /// <param name="enableRoles">Discord roles</param>
        /// <param name="disableRole">Discord role</param>
        /// <param name="channel">Discord channel</param>
        /// <returns>Nothing</returns>
        public async static Task EnableMessagesOnlyForRoles(ICollection<IRole> enableRoles, IRole disableRole, IGuildChannel channel)
        {
            await EnableOnlyForRoles(enableRoles, disableRole, channel, "sendMessages");
        }

        /// <summary>
        /// Enables sendMessages for <paramref name="enableRole"/>
        /// and disables it for <paramref name="disableRole"/> for <paramref name="channel"/>
        /// </summary>
        /// <param name="enableRole">Discord roles</param>
        /// <param name="disableRole">Discord role</param>
        /// <param name="channel">Discord channel</param>
        /// <returns>Nothing</returns>
        public async static Task EnableSendMessagesOnlyForRole(IRole enableRole, IRole disableRole, IGuildChannel channel)
        {
            await EnableOnlyForRole(enableRole, disableRole, channel, "sendMessages");
        }

        /// <summary>
        /// Enables <paramref name="permission"/> for <paramref name="enableRole"/>
        /// and disables it for <paramref name="disableRole"/> for <paramref name="channel"/>
        /// </summary>
        /// <param name="enableRole">Discord roles</param>
        /// <param name="disableRole">Discord role</param>
        /// <param name="channel">Discord channel</param>
        /// <param name="permission">Name of permission</param>
        /// <returns>Nothing</returns>
        private async static Task EnableOnlyForRole(IRole enableRole, IRole disableRole, IGuildChannel channel, string permission)
        {
            // Disable for disableRole, enable for enableRole
            await UpdateRoleHelper(disableRole, channel, permission, PermValue.Deny);
            await UpdateRoleHelper(enableRole, channel, permission, PermValue.Allow);
        }

        /// <summary>
        /// Enables <paramref name="permission"/> for all roles from <paramref name="enableRoles"/>
        /// and disables it for <paramref name="disableRole"/> for <paramref name="channel"/>
        /// </summary>
        /// <param name="enableRoles">List of discord roles</param>
        /// <param name="disableRole">Discord role</param>
        /// <param name="channel">Discord channel</param>
        /// <param name="permission">Name of permission</param>
        /// <returns>Nothing</returns>
        private async static Task EnableOnlyForRoles(ICollection<IRole> enableRoles, IRole disableRole, IGuildChannel channel, string permission)
        {
            // Disable for disableRole, enable for all in enableRoles
            await UpdateRoleHelper(disableRole, channel, permission, PermValue.Deny);
            foreach (var role in enableRoles)
            {
                await UpdateRoleHelper(role, channel, permission, PermValue.Allow);
            }
        }

        /// <summary>
        /// Updates given <paramref name="permission"/> with <paramref name="value"/> for <paramref name="role"/> for <paramref name="channel"/>
        /// </summary>
        /// <param name="role">Discord role</param>
        /// <param name="channel">Discord channel</param>
        /// <param name="permission">Name of the permission to be changed</param>
        /// <param name="value">Value</param>
        /// <returns>Nothing</returns>
        private async static Task UpdateRoleHelper(IRole role, IGuildChannel channel, string permission, PermValue value)
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
                    rolePerms = rolePerms.Value.Modify(viewChannel: value);
                    break;
                case "addReactions":
                    rolePerms = rolePerms.Value.Modify(addReactions: value);
                    break;
                case "speak":
                    rolePerms = rolePerms.Value.Modify(speak: value);
                    break;
                case "sendMessages":
                    rolePerms = rolePerms.Value.Modify(sendMessages: value);
                    break;
            }

            // Adds updated permissions for role
            await channel.AddPermissionOverwriteAsync(role, rolePerms.Value);
        }

        /// <summary>
        /// Updates given <paramref name="permission"/> with <paramref name="value"/> for <paramref name="user"/> for <paramref name="channel"/>
        /// </summary>
        /// <param name="user">Discord user</param>
        /// <param name="channel">Discord channel</param>
        /// <param name="permission">Name of the permission to be changed</param>
        /// <param name="value">Value</param>
        /// <returns>Nothing</returns>
        private async static Task UpdateUserHelper(IUser user, IGuildChannel channel, string permission, PermValue value)
        {
            var rolePerms = channel.GetPermissionOverwrite(user);

            if (!rolePerms.HasValue)
                rolePerms = OverwritePermissions.InheritAll;

            // Removes old permission for user
            await channel.RemovePermissionOverwriteAsync(user);

            // Find which permission to modify
            switch (permission)
            {
                case "viewChannel":
                    rolePerms = rolePerms.Value.Modify(viewChannel: value);
                    break;
                case "addReactions":
                    rolePerms = rolePerms.Value.Modify(addReactions: value);
                    break;
                case "speak":
                    rolePerms = rolePerms.Value.Modify(speak: value);
                    break;
                case "sendMessages":
                    rolePerms = rolePerms.Value.Modify(sendMessages: value);
                    break;
            }

            // Adds updated permissions for user
            await channel.AddPermissionOverwriteAsync(user, rolePerms.Value);
        }
    }
}
