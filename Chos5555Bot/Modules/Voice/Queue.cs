using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;

namespace Chos5555Bot.Modules.Voice
{
    class Queue
    {
        // Dictionary for guild id and list of songs
        public Dictionary<ulong, List<Song>> QueueMap { get; set; } = new();
    }
}
