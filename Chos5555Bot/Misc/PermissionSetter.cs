using DAL;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chos5555Bot.Misc
{
    internal class PermissionSetter
    {
        public static async Task SetShownOnlyForRole(IRole showRole, IRole hideRole, IGuildChannel channel)
        {
            var perms = channel.PermissionOverwrites;

            await SetHiddenForRole(hideRole, channel);
            await SetShownForRole(showRole, channel);
        }

        public static async Task SetShownForRoles(ICollection<IRole> showRoles, IRole hideRole, IGuildChannel channel)
        {
            await SetHiddenForRole(hideRole, channel);

            foreach (var role in showRoles)
            {
                await SetShownForRole(role, channel);
            }
        }

        public static async Task SetHiddenForRole(IRole role, IGuildChannel channel)
        {
            // Deny viewing channel for given role
            await channel.AddPermissionOverwriteAsync(role,
                OverwritePermissions.InheritAll.Modify(viewChannel: PermValue.Deny));
        }

        public static async Task SetShownForRole(IRole role, IGuildChannel channel)
        {
            // Allow viewing channel for given role
            await channel.AddPermissionOverwriteAsync(role,
                OverwritePermissions.InheritAll.Modify(viewChannel: PermValue.Allow));
        }

        public static async Task DenyAddReaction(IRole role, IGuildChannel channel)
        {
            // TODO: Try in situ edit
            var rolePerms = channel.GetPermissionOverwrite(role);

            if (rolePerms is null)
                rolePerms = OverwritePermissions.InheritAll;

            await channel.RemovePermissionOverwriteAsync(role);

            // Stops users with role from adding new reactions, they can still react with the ones already there
            await channel.AddPermissionOverwriteAsync(role, rolePerms.Value.Modify(addReactions: PermValue.Deny));
        }

        public static async Task UpdateViewChannel(IRole role, IGuildChannel channel, PermValue value)
        {
            var rolePerms = channel.GetPermissionOverwrite(role);

            if (rolePerms is null)
                rolePerms = OverwritePermissions.InheritAll;

            await channel.RemovePermissionOverwriteAsync(role);

            await channel.AddPermissionOverwriteAsync(role, rolePerms.Value.Modify(viewChannel: value));
        }

    }
}
