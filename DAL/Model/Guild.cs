using System.Collections.Generic;

namespace DAL
{
    /// <summary>
    /// Model class representing a guild to be stored in the database
    /// </summary>
    public class Guild
    {
        public int Id { get; set; }
        // Id of corresponding guild in discord
        public ulong DiscordId { get; set; }
        // Command prefix used for this guild
        public char Prefix { get; set; } = '.';
        // Room to send game selection messages to
        public Room SelectionRoom { get; set; }
        // Discord role to be considered role for members of the guild
        public Role MemberRole { get; set; }
        // Id of a category channel to which text channels are stored when "deleted" by bot
        public ulong ArchiveCategoryId { get; set; }
        // Room to which rule messages are sent
        public Room RuleRoom { get; set; } = null;
        // Text of rule message
        public string RuleMessageText { get; set; }
        // Id of rule message on discord
        public ulong RuleMessageId { get; set; } = 0;
        // Queue of songs for the guild
        public ICollection<Song> Songs { get; set; } = new List<Song>();
        // Id of room to which messages are sent when a user leaves the guild on discord
        public ulong UserLeaveMessageRoomId { get; set; } = 0;
    }
}
