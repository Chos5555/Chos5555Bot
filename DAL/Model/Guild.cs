using System;
using System.Collections.Generic;

namespace DAL
{
    public class Guild
    {
        public int Id { get; set; }
        public ulong DiscordId { get; set; }
        public List<Role> Roles { get; set; } = new List<Role>();
        public Room SelectionRoom { get; set; }
        public List<Song> Queue { get; set; } = new List<Song>();
    }
}
