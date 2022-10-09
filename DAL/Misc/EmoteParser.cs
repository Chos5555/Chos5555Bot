using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DAL.Model;
using Discord;
using static DAL.Model.EmoteEmoji;

namespace DAL.Misc
{
    public class EmoteParser
    {
        public static EmoteEmoji ParseEmote(string emoteString)
        {
            var emoteParse = Emote.TryParse(emoteString, out var newEmote);
            var emojiParse = Emoji.TryParse(emoteString, out var newEmoji);

            IEmote result;
            EmoteType type;

            if (emoteParse)
            {
                result = newEmote;
                type = EmoteType.Emote;
            } else if (emojiParse)
            {
                result = newEmoji;
                type = EmoteType.Emoji;
            } else
            {
                throw new Exception("Couldn't parse emote with either Emote or Emoji.");
            }

            return new EmoteEmoji(type, result);
        }
    }
}
