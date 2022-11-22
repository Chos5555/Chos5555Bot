using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL.Model
{
    /// <summary>
    /// Model class representing a game to be stored in the database
    /// </summary>
    public class Game
    {
        public int Id { get; set; }
        // Name of the game
        public string Name { get; set; }
        // Guild to which the game belongs
        public Guild Guild { get; set; }
        // Emote of the game (used in selection message)
        public EmoteEmoji ActiveEmote { get; set; }
        // Id of the category of the game
        public ulong CategoryId { get; set; }
        // Id of game's selection message
        public ulong SelectionMessageId { get; set; }
        // All rooms belonging to this game
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
        // Id of the role given when reaction is added in selection room
        [ForeignKey("GameRoleGameId")]
        public Role GameRole { get; set; }
        // Whether the game has active role or not
        public bool HasActiveRole { get; set; }
        // Main active role of the game
        [ForeignKey("MainActiveRoleGameId")]
        public Role MainActiveRole { get; set; }
        // All active roles of the game
        [ForeignKey("ActiveRoleGameId")]
        public ICollection<Role> ActiveRoles { get; set; } = new List<Role>();
        // Selection room for active roles of the game
        public Room ActiveCheckRoom { get; set; }
        // Room for mods to accept active role requests
        public Room ModAcceptRoom { get; set; }
        // All roles that can accept request in ModAcceptRoom
        [ForeignKey("ModAcceptRoleGameId")]
        public ICollection<Role> ModAcceptRoles { get; set; } = new List<Role>();
        // Quest room for mods to post quests in
        public Room ModQuestRoom { get; set; }
        // Whether the game has activity tracking enabled
        public bool TrackActivity { get; set; } = false;
        // Last time the activity of players was check to remove active roles
        public DateTime LastActivityCheck { get; set; }
        // TODO: When adding veteran status or something, make into a list of options of period, add/remove, role
        // TODO: Change int into something else to parse "5 days"/"7 hours", etc. (TimeSpan)
        // Period of time (amount of days) after which games active role should be removed
        public TimeSpan RemoveAfter { get; set; } = TimeSpan.FromDays(7);
    }
}
