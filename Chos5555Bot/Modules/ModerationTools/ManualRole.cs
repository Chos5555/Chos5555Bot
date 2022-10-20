using DAL;
using Chos5555Bot.Services;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chos5555Bot.Modules.ModerationTools
{
    public class ManualRole : ModuleBase<SocketCommandContext>
    {
        private readonly BotRepository _repo;
        private readonly LogService _log;

        public ManualRole(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        // TODO set Description (update in active room) role
        // TODO set choiceEmote (update in active room) role
        // TODO set resetable role
    }
}
