using System.Collections.Generic;

namespace DAL
{
    public class Guild
    {
        public int Id { get; set; }
        public ulong DiscordId { get; set; }
        public virtual Room SelectionRoom { get; set; }
        public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
        public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    }
}
