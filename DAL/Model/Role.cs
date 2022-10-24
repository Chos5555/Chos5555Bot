using DAL.Model;

namespace DAL
{
    public class Role
    {
        public int Id { get; set; }
        public ulong DisordId { get; set; }
        public string Name { get; set; }
        public bool Resettable { get; set; } = true;
        public bool NeedsModApproval { get; set; } = false;
        public EmoteEmoji ChoiceEmote { get; set; } = null;
        public string Description { get; set; } = "";
    }
}
