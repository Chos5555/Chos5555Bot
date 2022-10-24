using Discord;

namespace DAL.Model
{
    public class EmoteEmoji
    {
        private EmoteType type;
        public IEmote emote { get; set; }

        public enum EmoteType
        {
            Emote,
            Emoji
        }

        public EmoteEmoji(EmoteType type, IEmote emote)
        {
            this.type = type;
            this.emote = emote;
        }

        public IEmote Out()
        {
            if (type == EmoteType.Emote)
                return (Emote)emote;
            return (Emoji)emote;
        }

        public bool Equals(EmoteEmoji emoteEmoji)
        {
            if (type != emoteEmoji.type)
                return false;

            if (type == EmoteType.Emote)
                return ((Emote)emote).Id == ((Emote)emoteEmoji.emote).Id;

            return ((Emoji)emote).Name == ((Emoji)emoteEmoji.emote).Name;
        }
    }
}
