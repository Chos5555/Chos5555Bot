using DAL.Model;
using Discord;

namespace DAL
{
    public class Role
    {
        public int Id { get; set; }
        public ulong DisordId { get; set; }
        public Guild Guild { get; set; }
        public bool Resetable { get; set; } = true;
        public IEmote Emote { get; set; }
    }
}
