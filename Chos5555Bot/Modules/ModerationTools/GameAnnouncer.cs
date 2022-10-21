using System;
using System.Linq;
using System.Threading.Tasks;
using Chos5555Bot.Services;
using DAL;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Game = DAL.Model.Game;

namespace Chos5555Bot.Modules
{
    public static class GameAnnouncer
    {
        private static BotRepository repo;
        private static LogService log;

        public static void InitAnnouncer(BotRepository repo, LogService log)
        {
            GameAnnouncer.repo = repo;
            GameAnnouncer.log = log;
        }

        public static async Task AnnounceGame(Game game, Room selectionRoom, SocketCommandContext context)
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

        public static async Task AnnounceActiveRoles(Game game, ITextChannel channel, SocketCommandContext context, IRole mainDiscordRole = null)
        {
            // Delete old messages
            await channel.DeleteMessagesAsync(await channel.GetMessagesAsync().FlattenAsync());

            // Post MainActiveRole separately, other roles afterwards
            await channel.SendMessageAsync($"Main {game.Name} active role:");

            await AnnounceActiveRole(game.MainActiveRole, game, channel, context, mainDiscordRole);

            await channel.SendMessageAsync($"Other roles:");

            foreach (var role in game.ActiveRoles.Where(r => r.Id != game.MainActiveRole.Id))
            {
                await AnnounceActiveRole(role, game, channel, context);
            }
        }

        public static async Task AnnounceActiveRole(Role role, Game game, ITextChannel channel, SocketCommandContext context, IRole discordRole = null)
        {
            // If role doesn't have an emote, don't post it
            if (role.ChoiceEmote is null)
                return;
            
            // If discordRole is not passed, find it
            if (discordRole is null)
            {
                discordRole = context.Guild.GetRole(role.DisordId);
            }
            var message = await channel.SendMessageAsync($"{discordRole.Mention} {role.ChoiceEmote.Out()} - {role.Description}");

            await message.AddReactionAsync(role.ChoiceEmote.Out());

            await log.Log($"Announced role {role.Name} into {game.Name}'s active channel.", LogSeverity.Info);
        }
    }
}
