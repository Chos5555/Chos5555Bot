
using Discord;
using System.Collections.Generic;

namespace DAL.Model
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guild Guild { get; set; }
        public Emote ActiveEmote { get; set; }
        public ulong SelectionMessageId { get; set; } = 0;
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
        public Role GameRole { get; set; }
        public bool HasActiveRole { get; set; }
        public ICollection<Role> ActiveRoles { get; set; } = new List<Role>();
        public Room ActiveCheckRoom { get; set; }
        public Room ModAcceptRoom { get; set; }
        public ICollection<Role> ModAcceptRoles { get; set; } = new List<Role>();
    }
}
