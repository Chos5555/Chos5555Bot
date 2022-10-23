using DAL;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Newtonsoft.Json.Linq;

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

        public static async Task UpdateAddReaction(IRole role, IGuildChannel channel, PermValue value)
        {
            // Stops users with role from adding new reactions, they can still react with the ones already there
            await UpdateHelper(role, channel, "addReactions", value);
        }

        public static async Task UpdateViewChannel(IRole role, IGuildChannel channel, PermValue value)
        {
            await UpdateHelper(role, channel, "viewChannel", value);
        }

        private static async Task UpdateHelper(IRole role, IGuildChannel channel, string permission, PermValue value)
        {
            var rolePerms = channel.GetPermissionOverwrite(role);

            if (!rolePerms.HasValue)
                rolePerms = OverwritePermissions.InheritAll;

            await channel.RemovePermissionOverwriteAsync(role);

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
