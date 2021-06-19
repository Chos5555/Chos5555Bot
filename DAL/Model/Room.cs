using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class Room
    {
        public int Id { get; set; }
        public ulong DiscordId { get; set; }
        public bool IsSelectionRoom { get; set; }
    }
}
