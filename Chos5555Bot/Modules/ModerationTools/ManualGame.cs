using DAL;
using Chos5555Bot.Services;
using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using Chos5555Bot.Misc;
using DAL.Misc;

namespace Chos5555Bot.Modules.ModerationTools
{
    /// <summary>
    /// Module class containing commands for managing games
    /// </summary>
    [Name("Manual Game Management")]
    public class ManualGame : ModuleBase<SocketCommandContext>
    {
        private readonly BotRepository _repo;
        private readonly LogService _log;

        public ManualGame(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("deleteGame", RunMode = RunMode.Async)]
        [Summary("Deletes game, all of its channels and roles.")]
        private async Task DeleteGameByRoleCommand(
            [Name("Role")][Summary("Role of game to be deleted (needs to be a mention).")] IRole discordRole)
        {
            var game = await _repo.FindGameByGameRole(await _repo.FindRole(discordRole));
            if (game is null)
            {
                await Context.Channel.SendMessageAsync("Couldn't find a game with this role.");
                return;
            }

            await DeleteGameCommand(discordRole, game.Name);
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("deleteGame", RunMode = RunMode.Async)]
        [Summary("Deletes game, all of its channels and roles.")]
        private async Task DeleteGameByRoleCommand(
            [Name("Name")][Summary("Name of game to be deleted.")][Remainder] string gameName)
        {
            var game = await _repo.FindGame(gameName);
            if (game is null)
            {
                await Context.Channel.SendMessageAsync("Couldn't find a game with this name.");
                return;
            }

            await DeleteGameCommand(Context.Guild.GetRole(game.GameRole.DisordId), gameName);
        }

        /// <summary>
        /// Main deleteGame method, deletes all roles and channels connected to the game, the game itself from DB and from discord.
        /// </summary>
        /// <param name="discordRole">Discord role of games GameRole</param>
        /// <param name="gameName">Name of the game</param>
        /// <returns>Nothing</returns>
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("deleteGame", RunMode = RunMode.Async)]
        [Summary("Deletes game, all of its channels and roles.")]
        private async Task DeleteGameCommand(
            [Name("Role")][Summary("Role of game to be deleted (needs to be a mention).")] IRole discordRole,
            [Name("Name")][Summary("Name of game to be deleted.")][Remainder] string gameName)
        {
            var game = await _repo.FindGameByNameAndGameRole(gameName, discordRole.Id);
            var guild = await _repo.FindGuild(game.Guild);

            if (game is null)
                await Context.Channel.SendMessageAsync($"Couldn't find game named {gameName} with role {discordRole}");

            // Delete all roles belonging to this game in DB and on discord
            foreach (var role in await _repo.FindAllRolesByGame(game))
            {
                await _repo.RemoveRole(await _repo.FindRole(role));
                await Context.Guild.GetRole(role.DisordId).DeleteAsync();
            }

            // Delete all channels belonging to this game in DB and on discord
            ulong? categoryId = 0;
            foreach (var room in game.Rooms.ToArray())
            {
                await _repo.RemoveRoom(await _repo.FindRoom(room));
                var discordChannel = Context.Guild.GetChannel(room.DiscordId);
                await discordChannel.DeleteAsync();

                // Get category of the game
                categoryId = (discordChannel as INestedChannel).CategoryId;
                if (categoryId.HasValue)
                    categoryId = categoryId.Value;
            }

            // Delete the category
            var categoryChannel = Context.Guild.GetChannel(categoryId.Value);
            await categoryChannel.DeleteAsync();

            // Delete games selection message if there is one (if SelectionRoom is set)
            if (guild.SelectionRoom is not null)
            {
                var channel = Context.Guild.GetChannel(guild.SelectionRoom.DiscordId) as ISocketMessageChannel;
                await (await channel.GetMessageAsync(game.SelectionMessageId)).DeleteAsync();
            }

            await _repo.RemoveGame(game);

            await _log.Log($"Deleted game {game.Name} from server {Context.Guild.Name}", LogSeverity.Info);
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("addModRole")]
        [Summary("Adds a new role to mod roles of a game.")]
        private async Task AddModRoleCommand(
            [Name("Role")][Summary("Role to be added into games mod roles.")] IRole role,
            [Name("Name")][Summary("Name of the game")][Remainder] string gameName)
        {
            var game = await _repo.FindGame(gameName);

            // Try to find the role in DB, if it's not present, create a new one
            var modRole = await _repo.FindRole(role);
            if (modRole is null)
            {
                modRole = new Role()
                {
                    DisordId = role.Id,
                    Name = role.Name,
                    Resettable = false,
                    NeedsModApproval = true,
                };
                await _repo.AddRole(modRole);
            }

            // Set ModAcceptRoom viewable for new mod role
            await PermissionSetter.UpdateViewChannel(role, Context.Guild.GetChannel(game.ModAcceptRoom.DiscordId), PermValue.Allow);

            game.ModAcceptRoles.Add(modRole);
            await _repo.UpdateGame(game);

            // If the first ModAcceptRole was added, announce all active roles into selection channel
            if (game.ModAcceptRoles.Count == 1)
            {
                var activeCheckChannel = Context.Guild.GetChannel(game.ActiveCheckRoom.DiscordId);
                await GameAnnouncer.AnnounceNonMainActiveRoles(game, activeCheckChannel as ITextChannel, Context);
            }
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("removeModRole")]
        [Summary("Removes mod role from a game")]
        private async Task RemoveModRoleCommand(
            [Name("Role")][Summary("Role to be removed from games mod roles.")] IRole role,
            [Name("Name")][Summary("Name of the game.")][Remainder] string gameName)
        {
            var game = await _repo.FindGame(gameName);
            var modRole = await _repo.FindRole(role);

            game.ModAcceptRoles.Remove(modRole);
            await _repo.UpdateGame(game);

            // Hide ModAcceptRoom from removed role
            await PermissionSetter.UpdateViewChannel(role, Context.Guild.GetChannel(game.ModAcceptRoom.DiscordId), PermValue.Deny);
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("setModRoom")]
        [Summary("Sets mod room for a game.")]
        private async Task setMemberRoleCommand(
            [Name("Channel")][Summary("Channel to be set (needs to be a mention).")] IChannel discordChannel,
            [Name("Name")][Summary("Name of the game.")][Remainder] string gameName)
        {
            var game = await _repo.FindGame(gameName);

            // Try to find the channel in DB, if it's not present, create a new one
            var channel = await _repo.FindRoom(discordChannel);
            if (channel is null)
            {
                channel = new Room()
                {
                    DiscordId = discordChannel.Id,
                };
                await _repo.AddRoom(channel);
            }

            game.ModAcceptRoom = channel;
            await _repo.UpdateGame(game);

            // Create a list of discord roles out of games ModAcceptRoles
            var modDiscordRoles = new List<IRole>();
            foreach (var role in game.ModAcceptRoles)
            {
                modDiscordRoles.Add(Context.Guild.GetRole(role.DisordId));
            }

            // Set the ModAcceptRoom visible only for ModAcceptRoles
            await PermissionSetter.EnableViewOnlyForRoles(modDiscordRoles, Context.Guild.GetRole(game.MainActiveRole.DisordId), discordChannel as IGuildChannel);
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("setGameEmote")]
        [Summary("Sets emote for a game (unfortunately this can't change the reacted emote and will remove the old emote with all of its reactions).")]
        private async Task SetGameEmoteCommand(
            [Name("Emote")][Summary("Emote to be set.")] IEmote emote,
            [Name("Name")][Summary("Name of the game.")][Remainder] string gameName)
        {
            var game = await _repo.FindGame(gameName);

            var parsedEmote = EmoteParser.ParseEmote(emote.ToString());

            // Save old emote to replace it with new emote
            var oldEmote = game.ActiveEmote.Out();

            game.ActiveEmote = parsedEmote;
            await _repo.UpdateGame(game);

            // Update emote on announce message
            var guildSelectionChannelId = (await _repo.FindGuild(Context.Guild)).SelectionRoom.DiscordId;
            var message = await MessageFinder.FindAnnouncedMessage(game.GameRole, Context.Guild.GetTextChannel(guildSelectionChannelId));
            var newMessageContent = message.Content.Replace(oldEmote.ToString(), game.ActiveEmote.Out().ToString());

            // Remove reactions for old emote and react with new emote
            await message.RemoveAllReactionsForEmoteAsync(oldEmote);
            await message.AddReactionAsync(parsedEmote.Out());

            await (message as IUserMessage).ModifyAsync(m => { m.Content = newMessageContent; });
        }

        /// <summary>
        /// Adds channel in which the command is used to the games Rooms.
        /// </summary>
        /// <param name="gameName">Name of the game</param>
        /// <returns>Nothing</returns>
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [Command("addChannelToGame")]
        [Summary("Adds this channel to a game.")]
        private async Task AddChannelToGameCommand(
            [Name("Name")][Summary("Name of the game.")][Remainder] string gameName)
        {
            var game = await _repo.FindGame(gameName);

            var room = await _repo.FindRoom(Context.Channel);

            if (room is null)
            {
                room = new Room()
                {
                    DiscordId = Context.Channel.Id
                };
                await _repo.AddRoom(room);
            }

            game.Rooms.Add(room);
            await _repo.UpdateGame(game);
        }

        /// <summary>
        /// Adds a role to the games ActiveRoles.
        /// </summary>
        /// <param name="discordRole">Discord role</param>
        /// <param name="resettable">Whether the role should be resettable or not</param>
        /// <param name="needModApproval">Whether the role needs approval by a mod or not</param>
        /// <param name="emote">Emote of the role in selection room</param>
        /// <param name="gameName">Name of the game</param>
        /// <returns>Nothing</returns>
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Command("addRoleToGame")]
        [Summary("Adds role to a game.")]
        private async Task AddRoleToGameCommand(
            [Name("Role")][Summary("Role to be added to a game (needs to be a mention).")] IRole discordRole,
            [Name("Is Resettable")][Summary("Whether the role should be resettable (true/false).")] bool resettable,
            [Name("Needs mod approval")][Summary("Whether giving the role to a user need to be approved by a moderator (true/false).")] bool needModApproval,
            [Name("Emote")][Summary("Emote of the role in selection room.")] IEmote emote,
            [Name("Name")][Summary("Name of the game.")][Remainder] string gameName)
        {
            var game = await _repo.FindGame(gameName);

            var role = await _repo.FindRole(discordRole);

            if (role is null)
            {
                role = new Role()
                {
                    DisordId = discordRole.Id,
                    Name = discordRole.Name,
                    Resettable = resettable,
                    NeedsModApproval = needModApproval,
                    ChoiceEmote = EmoteParser.ParseEmote(emote.ToString())
                };
                await _repo.AddRole(role);
            }

            game.ActiveRoles.Add(role);

            // Announce new active role only if there is a mod role present
            if (game.ModAcceptRoles.Count > 0)
                await GameAnnouncer.AnnounceActiveRole(role, game, Context.Guild.GetChannel(game.ActiveCheckRoom.DiscordId) as ITextChannel, Context);

            await _repo.UpdateGame(game);
        }

        /// <summary>
        /// Sets quest channel for game that has the same category as the channel it was posted in,
        /// sets permission to the channel, so only people with mod roles of the game can send messages in it
        /// </summary>
        /// <returns>Nothing</returns>
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Command("SetModQuestChannel")]
        [Summary("Sets this channel as a mod quest channel for this game.")]
        private async Task SetQuestChannel()
        {
            // Find a game for the category this channel is in
            var categoryId = (Context.Channel as INestedChannel).CategoryId.Value;
            var game = await _repo.FindGameByCategoryId(categoryId);

            if (game is null)
            {
                await Context.Channel.SendMessageAsync("This channel is not in a category of a game.");
                return;
            }

            // Find channel in DB, if it's not there, add it
            var room = await _repo.FindRoom(Context.Channel);
            if (room is null)
            {
                room = new Room()
                {
                    DiscordId = Context.Channel.Id
                };
                await _repo.AddRoom(room);
            }

            game.ModQuestRoom = room;

            // Get a list of IRole of ModRoles and only enable them to send message into the quest channel
            var modRoles = new List<IRole>();
            foreach (var role in game.ModAcceptRoles)
            {
                modRoles.Add(Context.Guild.GetRole(role.DisordId));
            }
            await PermissionSetter.EnableMessagesOnlyForRoles(modRoles, Context.Guild.EveryoneRole, Context.Channel as IGuildChannel);

            await _repo.UpdateGame(game);

            await _log.Log($"Set {Context.Channel.Name} channel as mod Quest chanel for {game.Name} in {Context.Guild.Name}.", LogSeverity.Info);
        }
    }
}
