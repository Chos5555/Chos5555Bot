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
        public static async Task SetShownOnlyForRole(IRole role, IRole everyoneRole, IGuildChannel channel)
        {
            await SetHiddenForRole(everyoneRole, channel);
            await SetShownForRole(role, channel);
        }

        public static async Task SetShownForRoles(ICollection<IRole> roles, IRole everyoneRole, IGuildChannel channel)
        {
            await SetHiddenForRole(everyoneRole, channel);

            foreach (var role in roles)
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
            // Stops users with role from adding new reactions, they can still react with the ones already there
            await channel.AddPermissionOverwriteAsync(role,
                OverwritePermissions.InheritAll.Modify(addReactions: PermValue.Deny));
        }
    }
}
