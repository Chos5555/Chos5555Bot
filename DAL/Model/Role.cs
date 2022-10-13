using DAL.Model;
using Discord;

namespace DAL
{
    public class Role
    {
        public int Id { get; set; }
        public ulong DisordId { get; set; }
        public string Name { get; set; }
        public bool Resetable { get; set; } = true;
        public bool NeedsModApproval { get; set; } = false;
        public EmoteEmoji ChoiceEmote { get; set; }
        public string Description { get; set; } = "";
    }
}
