using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

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
        // Completed for this user
        [ForeignKey("CompletedQuestsUserId")]
        public List<CompletedQuests> CompletedQuests { get; set; } = new List<CompletedQuests>();
    }
}
