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
        public static async void SetOnlyViewableByRole(IRole role, IRole everyoneRole, IGuildChannel channel)
        {
            // Deny viewing channel for everyone role
            await channel.AddPermissionOverwriteAsync(everyoneRole,
                OverwritePermissions.InheritAll.Modify(viewChannel: PermValue.Deny));

            // Allow viewing channel for given role
            await channel.AddPermissionOverwriteAsync(role,
                OverwritePermissions.InheritAll.Modify(viewChannel: PermValue.Allow));
        }
    }
}
