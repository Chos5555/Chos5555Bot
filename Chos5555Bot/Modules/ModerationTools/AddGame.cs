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
        /// <param name="emote">Active Emote of the new game (Needs to have '\' in front when using this command)</param>
        /// <param name="name">Name of the new game</param>
        /// <returns></returns>

        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("addGame")]
        private async Task Command(IRole discordRole, string emote, [Remainder] string name)
        {
            await _log.Log($"Started addGame command with role: {discordRole.Name}, name: {name}, emote: {emote}.", LogSeverity.Verbose);
            
            var parsedEmote = EmoteParser.ParseEmote(emote);

            var guild = await _repo.FindGuild(Context.Guild);
            Role role = new() { DisordId = discordRole.Id };

            DAL.Model.Game game = new()
            {
                Name = name,
                ActiveEmote = parsedEmote,
                Guild = guild
            };            

            var gameCategory = await Context.Guild.CreateCategoryChannelAsync(name);

            PermissionSetter.SetOnlyViewableByRole(discordRole, Context.Guild.EveryoneRole, gameCategory);

            var discordTextRoom = await Context.Guild.CreateTextChannelAsync(name, p => {
                p.CategoryId = gameCategory.Id;
                p.Topic = $"General channel for {name}.";
            });
            var discordVoiceRoom = await Context.Guild.CreateVoiceChannelAsync(name, p =>
            {
                p.CategoryId = gameCategory.Id;
            });

            Room textRoom = new() { DiscordId = discordTextRoom.Id };
            Room voiceRoom = new() { DiscordId = discordVoiceRoom.Id };

            game.Rooms.Add(textRoom);
            game.Rooms.Add(voiceRoom);

            await _repo.AddRole(role);

            game.GameRole = role;
            await _repo.UpdateGuild(guild);

            await _log.Log($"Added new game {name} with {discordRole.Name}and emote {emote}.", LogSeverity.Info);

            await GameAnnouncer.AnnounceGame(game, guild.SelectionRoom, Context);
        }

        /* TODO: Add extended command
         * Extended command for games with active roles
         */
    }
}
