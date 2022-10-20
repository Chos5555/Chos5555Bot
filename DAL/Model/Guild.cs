using System.Collections.Generic;

namespace DAL
{
    public class Guild
    {
        public int Id { get; set; }
        public ulong DiscordId { get; set; }
        public Room SelectionRoom { get; set; }
        public Role MemberRole { get; set; }
        public ulong ArchiveCategoryId { get; set; }
        public Room RuleRoom { get; set; } = null;
        public string RuleMessageText { get; set; }
        public ulong RuleMessageId { get; set; } = 0;
        public ICollection<Song> Songs { get; set; } = new List<Song>();
    }
}
