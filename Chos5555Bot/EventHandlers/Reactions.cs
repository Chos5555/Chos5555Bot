using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DAL;

namespace Chos5555Bot.EventHandlers
{
    public class Reactions
    {
        public static async Task AddHandler(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            BotRepository repo = new BotRepository();
            var game = await repo.FindGameByMessage(reaction.MessageId);

            if (game is null)
            {
                return;
            }

            var role = await repo.FindRoleByGame(game);
            //TODO figure out how to get role from guild and assign role to user in reaction
            
        }

        public static async Task RemoveHandler(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            BotRepository repo = new BotRepository();
            var game = await repo.FindGameByMessage(reaction.MessageId);

            if (game is null)
            {
                return;
            }

            var role = await repo.FindRoleByGame(game);

            //TODO remove role from user
        }
    }
}
