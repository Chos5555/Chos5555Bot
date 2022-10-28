
namespace DAL
{
    /// <summary>
    /// Model class representing a channel to be stored in the database
    /// </summary>
    public class Room
    {
        public int Id { get; set; }
        // Id of a corresponding channel in discord
        public ulong DiscordId { get; set; }
    }
}
