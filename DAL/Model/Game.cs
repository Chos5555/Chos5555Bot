
using System.Collections.Generic;

namespace DAL.Model
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guild Guild { get; set; }
        public string Emote { get; set; }
        public ulong SelectionMessageId { get; set; } = 0;
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
        public ICollection<Role> Roles { get; set; } = new List<Role>();
        public bool HaveActiveRole { get; set; }
        public Role ActiveRole { get; set; }
        public Room ActiveCheckRoom { get; set; }
        public Room ModAcceptRoom { get; set; }
        public ICollection<Role> ModAcceptRoles { get; set; } = new List<Role>();
    }
}
