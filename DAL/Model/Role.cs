using DAL.Model;

namespace DAL
{
    /// <summary>
    /// Model class representing a role to be stored in the database
    /// </summary>
    public class Role
    {
        public int Id { get; set; }
        // Id of the corresponding role in discord
        public ulong DisordId { get; set; }
        // Name of the role
        public string Name { get; set; }
        // Whether the role can be reset or not (role removed from all users that have it)
        public bool Resettable { get; set; } = true;
        // Whether the request to get this role needs to be approved by a mod
        public bool NeedsModApproval { get; set; } = false;
        // Emote with which this role can be selected in games ActiveCheckRoom
        public EmoteEmoji ChoiceEmote { get; set; } = null;
        // Description of the role
        public string Description { get; set; } = "";
    }
}
