using System;
using DAL.Model;
using Discord;
using static DAL.Model.EmoteEmoji;

namespace DAL.Misc
{
    public class EmoteParser
    {
        /// <summary>
        /// Parses a string (either string of Emote or Emoji) into EmoteEmoji.
        /// </summary>
        /// <param name="emoteString">String of the emote to be parsed.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Thrown if neither Emote nor Emoji could be parsed.</exception>
        public static EmoteEmoji ParseEmote(string emoteString)
        {
            // TODO: Create NonparableEmoteException which is thrown when nothing could be parsed
            // Try to parse both Emote and emoji
            var emoteParse = Emote.TryParse(emoteString, out var newEmote);
            var emojiParse = Emoji.TryParse(emoteString, out var newEmoji);

            IEmote result;
            EmoteType type;

            if (emoteParse)
            {
                result = newEmote;
                type = EmoteType.Emote;
            }
            else if (emojiParse)
            {
                result = newEmoji;
                type = EmoteType.Emoji;
            }
            else
            {
                throw new Exception("Couldn't parse emote with either Emote or Emoji.");
            }

            return new EmoteEmoji(type, result);
        }
    }
}