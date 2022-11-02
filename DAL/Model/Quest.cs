
namespace DAL.Model
{
    /// <summary>
    /// Model class representing a quest to be stored in a database
    /// </summary>
    public class Quest
    {
        public int Id { get; set; }
        // Game for which this quest was created
        public Game Game { get; set; }
    }
}
