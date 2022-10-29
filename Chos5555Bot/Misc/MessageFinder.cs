using Chos5555Bot.Services;
using DAL;
using Discord;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Chos5555Bot.Misc
{
    /// <summary>
    /// Class containing method for finding message based on different parameters
    /// </summary>
    public class MessageFinder
    {
        private static BotRepository _repo;
        private static LogService _log;

        public static void InitFinder(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        /// <summary>
        /// Finds a message in a given channel that has the given role mentioned
        /// </summary>
        /// <param name="role">Discord role</param>
        /// <param name="channel">Discord channel</param>
        /// <returns>Message</returns>
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
