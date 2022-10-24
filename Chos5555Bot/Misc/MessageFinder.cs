using Chos5555Bot.Services;
using DAL;
using Discord;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Chos5555Bot.Misc
{
    public class MessageFinder
    {
        private static BotRepository _repo;
        private static LogService _log;

        public static void InitFinder(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        public static async Task<IMessage> FindAnnouncedMessage(Role role, ITextChannel channel)
        {
            return (await channel
                .GetMessagesAsync()
                .FlattenAsync())
                .Where(m => m.MentionedRoleIds.Contains(role.DisordId))
                .SingleOrDefault();
        }
    }
}
