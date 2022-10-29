﻿using System;
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
                throw new EmoteNotParsedException("Couldn't parse emote with either Emote or Emoji.");
            }

            return new EmoteEmoji(type, result);
        }

        private class EmoteNotParsedException : Exception
        {
            public EmoteNotParsedException() {}
            public EmoteNotParsedException(string message) : base(message) {}
            public EmoteNotParsedException(string message, Exception inner) : base(message, inner) {}
        }
    }
}