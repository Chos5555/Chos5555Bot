using System;

namespace DAL.Model
{
    /// <summary>
    /// Model class representing users activity in a game to be stored in the database
    /// </summary>
    public class GameActivity
    {
        public int Id { get; set; }
        // Game for which this activity is tracked
        public string GameName { get; set; }
        // Date and time of when user was last on voice for a game
        public DateTime LastAppearance { get; set; }
    }
}
