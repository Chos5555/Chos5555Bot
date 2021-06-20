using DAL;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chos5555Bot.Modules.Voice
{
    class Stop : ModuleBase<SocketCommandContext>
    {
        private BotRepository repo = new BotRepository();
        private Queue queue;

        [Command("addGame")]
        private async Task Command()
        {
            //TODO stop music playing
            var remainingQueue = queue.QueueMap.Where(g => g.Key == Context.Guild.Id).FirstOrDefault().Value;

            if (remainingQueue.Count != 0)
            {
                var guild = await repo.FindGuild(Context.Guild);
                //guild.Queue = remainingQueue;
                await Context.Channel.SendMessageAsync("Saved your remaining queue, so you can play it next time");
            }
            queue.QueueMap.Remove(Context.Guild.Id);
        }
    }
}
