
namespace DAL.Model
{
    /// <summary>
    /// Model class representing a quest to be stored in a database
    /// </summary>
    public class Quest
    {
        public int Id { get; set; }
        // Game for which this quest was created
        public Game Game { get; set; }
        // ID of message in the quest room
        public ulong QuestMessage { get; set; }
        // Id of message in mod quest room
        public ulong ModMessage { get; set; }
    }
}
