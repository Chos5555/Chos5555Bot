
namespace DAL.Model
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Emote { get; set; }
        public ulong MessageId { get; set; }
    }
}
