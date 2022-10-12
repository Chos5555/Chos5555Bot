﻿using DAL.Misc;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
                return (Emote) emote;
            return (Emoji) emote;
        }
    }
}