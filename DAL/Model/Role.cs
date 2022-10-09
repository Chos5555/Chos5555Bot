using DAL.Model;
using Discord;

namespace DAL
{
    public class Role
    {
        public int Id { get; set; }
        public ulong DisordId { get; set; }
        public bool Resetable { get; set; } = true;
        public bool NeedsModApproval { get; set; } = true;
        public EmoteEmoji Emote { get; set; }
    }
}
