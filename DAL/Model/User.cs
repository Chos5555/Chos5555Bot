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
        // List of quest scores for different games
        [ForeignKey("CompletedQuestsUserId")]
        public List<CompletedQuests> CompletedQuests { get; set; } = new List<CompletedQuests>();
        // List of activity in different games
        [ForeignKey("GameActivityUserId")]
        public List<GameActivity> GameActivities { get; set; } = new List<GameActivity>();
    }
}
