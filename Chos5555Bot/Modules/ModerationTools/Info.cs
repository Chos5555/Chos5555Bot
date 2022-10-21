using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chos5555Bot.Modules.ModerationTools
{
    public class Info : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        private async Task Ping()
        {
            var time = DateTimeOffset.Now - Context.Message.CreatedAt;
            await Context.Channel.SendMessageAsync($"🏓 **Pong!**\n**Latency:** {((int)time.TotalMilliseconds)} ms");
        }

        // TODO: Add .help
    }
}
