
namespace DAL.Model
{
    /// <summary>
    /// Model class representing a quest to be stored in a database
    /// </summary>
    public class Quest
    {
        public int Id { get; set; }
        // Game for which this quest was created
        public string GameName { get; set; }
        // Text of the quest
        public string Text { get; set; }
        // Id of the user that created this quest
        public ulong AuthorId { get; set; }
        // Id of the user that took this quest
        public ulong TakerId { get; set; }
        // Id of message in the quest room
        public ulong QuestMessage { get; set; }
        // Id of the channel the QuestMessage is in
        public ulong QuestMessageChannelId { get; set; }
        // Id of message in mod quest room
        public ulong ModMessage { get; set; }
    }
}
