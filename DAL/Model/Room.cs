
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
        // Speaker role
        public ulong SpeakerRoleId { get; set; } = 0;
        // Text channel for this channel, it it's a stage channel
        public ulong TextForStageId { get; set; } = 0;
    }
}
