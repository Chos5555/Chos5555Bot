using System.Collections.Generic;

namespace DAL.Model
{
    /// <summary>
    /// Model class representing a user to be stored in the database
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        // Id of a corresponding channel in discord
        public ulong DiscordId { get; set; }
        // Quests for this user
        public ICollection<Quest> Quests { get; set; } = new List<Quest>();
    }
}
