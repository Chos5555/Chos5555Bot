using DAL.Misc;
using Chos5555Bot.Services;
using DAL;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chos5555Bot.Misc;
using DAL.Model;
using Game = DAL.Model.Game;
using System.Data;

namespace Chos5555Bot.Modules
{
    public class AddGame : ModuleBase<SocketCommandContext>
    {
        private readonly BotRepository _repo;
        private readonly LogService _log;

        public AddGame(BotRepository repo, LogService log)
        {
            _repo = repo;
            _log = log;
        }

        /// <summary>
        /// Add command for basic games (without active role)
        /// </summary>
        /// <param name="discordRole">GameRole of the new game</param>
        /// <param name="emote">Active ChoiceEmote of the new game (Needs to have '\' in front when using this command)</param>
        /// <param name="name">Name of the new game</param>
        /// <returns></returns>

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("addGame")]
        private async Task AddBaseGame(IRole discordRole, string emote, [Remainder] string name)
        {
            await AddGameHelper(discordRole, emote, name, false);
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("addGame")]
        private async Task AddGameByRole(IRole discordRole, string emote, bool hasActiveRole = false)
        {
            await AddGameHelper(discordRole, emote, discordRole.Name, hasActiveRole);
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("addGame")]
        private async Task AddGameByName(string emote, bool hasActiveRole, [Remainder] string name)
        {
            var discordRole = await Context.Guild.CreateRoleAsync(name);
            await AddGameHelper(discordRole, emote, name, hasActiveRole);
        }

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("addActiveGame")]
        private async Task AddActiveGame(IRole discordRole, string emote, [Remainder] string name)
        {
            await AddGameHelper(discordRole, emote, name, true);
        }

        private async Task AddGameHelper(IRole discordRole, string emote, string name, bool hasActiveRole)
        {
            if (_repo.FindDuplicateGame(name, discordRole.Id))
            {
                await Context.Channel.SendMessageAsync("A game with this name or role is already created, please choose a different one.");
                await _log.Log($"User {Context.User.Username} tried to create game {name} with role {discordRole.Name}, but it already exists", LogSeverity.Verbose);
            }

            await _log.Log($"Started addGame command with role: {discordRole.Name}, name: {name}, emote: {emote}, active: {hasActiveRole}.",
                LogSeverity.Verbose);

            var (guild, role, game) = await SetupNewGame(discordRole, emote, name, hasActiveRole);

            var gameCategory = await Context.Guild.CreateCategoryChannelAsync(name);

            await PermissionSetter.SetShownOnlyForRole(discordRole, Context.Guild.EveryoneRole, gameCategory);

            var remainder = game.HasActiveRole ? "Recruit" : "General";

            var discordTextRoom = await Context.Guild.CreateTextChannelAsync($"{name} {remainder}", p =>
            {
                p.CategoryId = gameCategory.Id;
                p.Topic = $"{remainder} channel for {name}.";
            });
            var discordVoiceRoom = await Context.Guild.CreateVoiceChannelAsync($"{name} {remainder}", p =>
            {
                p.CategoryId = gameCategory.Id;
            });

            Room textRoom = new() { DiscordId = discordTextRoom.Id };
            Room voiceRoom = new() { DiscordId = discordVoiceRoom.Id };

            game.Rooms.Add(textRoom);
            game.Rooms.Add(voiceRoom);

            game.GameRole = role;

            // Setup ActiveRole part of game
            if (game.HasActiveRole)
                await SetupGameWithActiveRole(game, gameCategory.Id, discordRole);

            await _repo.AddRole(role);

            await _repo.AddRoom(textRoom);
            await _repo.AddRoom(voiceRoom);

            await _repo.AddGame(game);

            await _log.Log($"Added new game {(hasActiveRole ? "with":"without")} active role named {name} with {discordRole.Name} and emote {emote}.", LogSeverity.Info);

            await GameAnnouncer.AnnounceGame(game, guild.SelectionRoom, Context);
        }

        private async Task<(Guild, Role, Game)> SetupNewGame (IRole discordRole, string emote, string name, bool hasActiveRole)
        {
            var parsedEmote = EmoteParser.ParseEmote(emote);

            var guild = await _repo.FindGuild(Context.Guild);

            Role role = new()
            {
                DisordId = discordRole.Id,
                Name = discordRole.Name,
                ChoiceEmote = parsedEmote,
                NeedsModApproval = true
            };

            Game game = new()
            {
                Name = name,
                ActiveEmote = parsedEmote,
                Guild = guild,
                GameRole = role,
                HasActiveRole = hasActiveRole
            };

            return (guild, role, game);
        }

        private async Task SetupGameWithActiveRole (Game game, ulong categoryId, IRole discordGameRole)
        {
            // Create ActiveCheckRoom (doesn't need to have view permission set, the category is already hidden for everyone except GameRole)
            var discordActiveCheckRoom = await Context.Guild.CreateTextChannelAsync($"{game.Name} role choice",
                p => {
                    p.CategoryId = categoryId;
                    p.Topic = $"Here you can choose roles for {game.Name}";
                });

            // Disable users from adding new reactions
            await discordActiveCheckRoom.SyncPermissionsAsync();
            await PermissionSetter.DenyAddReaction(Context.Guild.EveryoneRole, discordActiveCheckRoom);

            var activeCheckRoom = new Room() { DiscordId = discordActiveCheckRoom.Id };

            game.Rooms.Add(activeCheckRoom);
            game.ActiveCheckRoom = activeCheckRoom;

            // Create ModAcceptRoom only viewable for Admin
            var discordmodAcceptRoom = await Context.Guild.CreateTextChannelAsync($"{game.Name} mod accept",
                p => {
                    p.CategoryId = categoryId;
                    p.Topic = $"Here mods can choose whether to give a role to a user that asks for it.";
                });

            // Find all admin roles that are not bots
            var adminRoles = Context.Guild.Roles
                .Where(r => r.Permissions.Administrator)
                .Where(r => r.Tags.BotId is null)
                .ToArray();

            // Hide room for people that can see the game category and show it only to admins
            // (since the game has just been created, there are no more mod roles yet)
            await PermissionSetter.SetHiddenForRole(discordGameRole, discordmodAcceptRoom);
            await PermissionSetter.SetShownForRoles(adminRoles, Context.Guild.EveryoneRole, discordmodAcceptRoom);

            var modAcceptRoom = new Room() { DiscordId = discordmodAcceptRoom.Id };

            game.Rooms.Add(modAcceptRoom);
            game.ModAcceptRoom = modAcceptRoom;

            // Create a default activeRole and 2 general rooms for it
            var discordActiveRole = await Context.Guild.CreateRoleAsync(
                $"{game.Name} Active",
                color: discordGameRole.Color,
                isMentionable: true
            );

            var activeRole = new Role()
            {
                DisordId = discordActiveRole.Id,
                Name = discordActiveRole.Name,
                Resettable = true,
                NeedsModApproval = true,
                ChoiceEmote = game.ActiveEmote
            };

            game.MainActiveRole = activeRole;
            game.ActiveRoles.Add(activeRole);

            var discordGeneralTextRoom = await Context.Guild.CreateTextChannelAsync($"{game.Name} General", p =>
            {
                p.CategoryId = categoryId;
                p.Topic = $"General channel for {game.Name}.";
            });
            var discordGeneralVoiceRoom = await Context.Guild.CreateVoiceChannelAsync($"{game.Name} General", p =>
            {
                p.CategoryId = categoryId;
            });

            // Set general rooms only viewable for users with active role
            await PermissionSetter.SetShownOnlyForRole(discordActiveRole, discordGameRole, discordGeneralTextRoom);
            await PermissionSetter.SetShownOnlyForRole(discordActiveRole, discordGameRole, discordGeneralVoiceRoom);

            Room generalTextRoom = new() { DiscordId = discordGeneralTextRoom.Id };
            Room generalVoiceRoom = new() { DiscordId = discordGeneralVoiceRoom.Id };

            game.Rooms.Add(generalTextRoom);
            game.Rooms.Add(generalVoiceRoom);

            // Anounce active roles to games ActiveCheckRoom
            await GameAnnouncer.AnnounceActiveRoles(game, discordActiveCheckRoom, Context, discordActiveRole);

            await _repo.AddRoom(activeCheckRoom);
            await _repo.AddRoom(modAcceptRoom);
            await _repo.AddRole(activeRole);
            await _repo.AddRoom(generalTextRoom);
            await _repo.AddRoom(generalVoiceRoom);
        }
    }
}
