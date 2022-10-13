
using Discord;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Model
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guild Guild { get; set; }
        public EmoteEmoji ActiveEmote { get; set; }
        public ulong SelectionMessageId { get; set; }
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
        [ForeignKey("GameRoleGameId")]
        public Role GameRole { get; set; }
        [ForeignKey("MainActiveRoleGameId")]
        public Role MainActiveRole { get; set; }
        public bool HasActiveRole { get; set; }
        [ForeignKey("ActiveRoleGameId")]
        public ICollection<Role> ActiveRoles { get; set; } = new List<Role>();
        public Room ActiveCheckRoom { get; set; }
        public Room ModAcceptRoom { get; set; }
        [ForeignKey("ModAcceptRoleGameId")]
        public ICollection<Role> ModAcceptRoles { get; set; } = new List<Role>();
    }
}
