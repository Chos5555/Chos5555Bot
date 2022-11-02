using System;
using System.Linq;
using System.Threading.Tasks;
using Chos5555Bot.Services;
using DAL;
using Discord;
using Discord.Commands;
using Game = DAL.Model.Game;

namespace Chos5555Bot.Modules
{
    // TODO: Move to misc
    /// <summary>
    /// Class containing methods for announcing games/ roles for games into selection channels
    /// </summary>
    public static class GameAnnouncer
    {
        private static BotRepository repo;
        private static LogService log;

        public static void InitAnnouncer(BotRepository repo, LogService log)
        {
            GameAnnouncer.repo = repo;
            GameAnnouncer.log = log;
        }

        /// <summary>
        /// Announces a gamme into guilds selection room
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="selectionRoom">Guilds selection room</param>
        /// <param name="context">Command context</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException">Thrown when selection room has not been set.</exception>
        public async static Task AnnounceGame(Game game, Room selectionRoom, SocketCommandContext context)
        {
            IEmote reactEmote = game.ActiveEmote.Out();
            // Send message to user if no selectionRoom is set
            if (selectionRoom is null)
            {
                await context.Channel.SendMessageAsync("Selection room has not yet been set. Couldn't post game selector message.");
                throw new NullReferenceException("Selection room has not yet been set.");
            }

            var discordSelectionRoom = context.Guild.GetTextChannel(selectionRoom.DiscordId);

            var message = await discordSelectionRoom.SendMessageAsync($"{game.Name} {reactEmote}");

            game.SelectionMessageId = message.Id;

            await repo.UpdateGame(game);

            await message.AddReactionAsync(reactEmote);

            await log.Log($"Announced game {game.Name} in guild {context.Guild.Id}({context.Guild.Name})", LogSeverity.Info);
        }

        /// <summary>
        /// Announces all of games active roles into games ActiveCheckRoom
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="channel">Games ActiveCheckRoom</param>
        /// <param name="context">Command context</param>
        /// <param name="mainDiscordRole">MainActiveRole to be announced separately</param>
        /// <returns>Nothing</returns>
        public async static Task AnnounceActiveRoles(Game game, ITextChannel channel, SocketCommandContext context, IRole mainDiscordRole = null)
        {
            // Delete old messages
            await channel.DeleteMessagesAsync(await channel.GetMessagesAsync().FlattenAsync());

            // Post MainActiveRole separately, other roles afterwards
            await channel.SendMessageAsync($"Main {game.Name} active role:");

            await AnnounceActiveRole(game.MainActiveRole, game, channel, context, mainDiscordRole);

            // Do not announce other roles if there are no mod roles
            if (game.ModAcceptRoles.Count == 0)
                return;

            await channel.SendMessageAsync($"Other roles:");

            await AnnounceNonMainActiveRoles(game, channel, context);
        }

        /// <summary>
        /// Announce all active roles, which are not MainActivRole of the game.
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="channel">Games ActiveCheckRoom</param>
        /// <param name="context">Command context</param>
        /// <returns>Nothing</returns>
        public async static Task AnnounceNonMainActiveRoles(Game game, ITextChannel channel, SocketCommandContext context)
        {
            foreach (var role in game.ActiveRoles.Where(r => r.Id != game.MainActiveRole.Id))
            {
                await AnnounceActiveRole(role, game, channel, context);
            }
        }

        /// <summary>
        /// Announces single active role
        /// </summary>
        /// <param name="role">Role</param>
        /// <param name="game">Game</param>
        /// <param name="channel">Games ActiveCheckRoom</param>
        /// <param name="context">Command context</param>
        /// <param name="discordRole">(Optional) discord role to be announced if <paramref name="role"/> has not yet been put into the DB.</param>
        /// <returns>Nothing</returns>
        public async static Task AnnounceActiveRole(Role role, Game game, ITextChannel channel, SocketCommandContext context, IRole discordRole = null)
        {
            // If role doesn't have an emote, don't post it
            if (role.ChoiceEmote is null)
                return;

            // If discordRole is not passed, find it
            discordRole ??= context.Guild.GetRole(role.DisordId);

            var message = await channel.SendMessageAsync($"{discordRole.Mention} {role.ChoiceEmote.Out()} - {role.Description}");

            // Add selection reaction
            await message.AddReactionAsync(role.ChoiceEmote.Out());

            await log.Log($"Announced role {role.Name} into {game.Name}'s active channel.", LogSeverity.Info);
        }
    }
}
