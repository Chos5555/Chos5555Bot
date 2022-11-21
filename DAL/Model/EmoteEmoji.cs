using Discord;
using System.Runtime.CompilerServices;

namespace DAL.Model
{
    /// <summary>
    /// Class representing either Emote or Emoji
    /// </summary>
    public class EmoteEmoji
    {
        private readonly EmoteType Type;
        public IEmote Emote { get; set; }

        public enum EmoteType
        {
            Emote,
            Emoji
        }

        public EmoteEmoji(EmoteType type, IEmote emote)
        {
            Type = type;
            Emote = emote;
        }

        /// <summary>
        /// Returns either Emote or Emoji, depending on what type the emote stored is 
        /// </summary>
        /// <returns>Emote or Emoji</returns>
        public IEmote Out()
        {
            if (Type == EmoteType.Emote)
                return (Emote)Emote;
            return (Emoji)Emote;
        }

        /// <summary>
        /// Compares to another EmoteEmoji, returns whether they are the same emote or not
        /// </summary>
        /// <param name="emoteEmoji">EmoteEmoji for this one to be compared to</param>
        /// <returns>bool</returns>
        public bool Equals(EmoteEmoji emoteEmoji)
        {
            if (Type != emoteEmoji.Type)
                return false;

            if (Type == EmoteType.Emote)
                return ((Emote)Emote).Id == ((Emote)emoteEmoji.Emote).Id;

            return ((Emoji)Emote).Name == ((Emoji)emoteEmoji.Emote).Name;
        }

        /// <summary>
        /// Compares to IEmote
        /// </summary>
        /// <param name="emote">IEmote object to compare to</param>
        /// <returns>bool</returns>
        public bool Equals(IEmote emote)
        {
            if(Emote.GetType() == typeof(Emote))
                return Equals((Emote)emote);

            return Equals((Emoji)emote);
        }

        /// <summary>
        /// Compares to Emote
        /// </summary>
        /// <param name="emote">Emoji object to compare to</param>
        /// <returns>bool</returns>
        public bool Equals(Emote emote)
        {
            if (Type == EmoteType.Emoji)
                return false;

            return ((Emote)Emote).Id == emote.Id;
        }

        /// <summary>
        /// Compares to Emoji
        /// </summary>
        /// <param name="emoji">Emoji object to compare to</param>
        /// <returns>bool</returns>
        public bool Equals(Emoji emoji)
        {
            if (Type == EmoteType.Emote)
                return false;

            return ((Emoji)Emote).Name == emoji.Name;
        }
    }
}
