using System;

namespace Chos5555Bot.Exceptions
{
    internal class GuildNotFoundException : Exception
    {
        public GuildNotFoundException() { }
        public GuildNotFoundException(string message) : base(message) { }
        public GuildNotFoundException(string message, Exception inner) : base(message, inner) { }
    }
}
