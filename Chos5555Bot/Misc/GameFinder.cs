using Chos5555Bot.Services;
using DAL;
using Discord;
using System;
using System.Threading.Tasks;
using Game = DAL.Model.Game;

namespace Chos5555Bot.Misc
{
    /// <summary>
    /// Class containing method for finding message based on different parameters
    /// </summary>
    public class GameFinder
    {
        private static BotRepository _repo;
        private static LogService _log;

        public static void InitFinder(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        /// <summary>
        /// Tries to find a game in DB based on channel and its category.
        /// </summary>
        /// <param name="channel">hannel based on whose categoryId a game is supposed to be found</param>
        /// <returns>result of parse (potentially with exception), game, channel as INestedChannel</returns>
        public async static Task<((bool, Exception), Game, INestedChannel)> TryFindGameForChannel(IChannel channel)
        {
            // Try to find game, if FindGameForChannel throws exception, return unsuccessful and reason
            try
            {
                var (g, ch) = await FindGameForChannel(channel);
                return ((true, null), g, ch);
            } catch (GameNotFoundException ex)
            {
                return ((false, ex), null, null);
            }
        }

        /// <summary>
        /// Finds a game in DB based on channel and its category, throws exception otherwise.
        /// </summary>
        /// <param name="channel">Channel based on whose categoryId a game is supposed to be found</param>
        /// <returns>game and channel cast as INestedChannel</returns>
        /// <exception cref="GameNotFoundException">Exception if no game can be found</exception>
        public async static Task<(Game, INestedChannel)> FindGameForChannel(IChannel channel)
        {
            // Check that channel is in a category and belongs to a game
            var nestedChannel = (channel as INestedChannel);
            if (!nestedChannel.CategoryId.HasValue)
            {
                throw new GameNotFoundException("This channel is not in a category.");
            }

            var game = await _repo.FindGameByCategoryId(nestedChannel.CategoryId.Value);
            if (game is null)
            {
                throw new GameNotFoundException("This category doesn't belong to a game.");
            }

            // Return game that was found
            return (game, nestedChannel);
        }

        public class GameNotFoundException : Exception
        {
            public GameNotFoundException() { }
            public GameNotFoundException(string message) : base(message) { }
            public GameNotFoundException(string message, Exception inner) : base(message, inner) { }
        }
    }
}
