using DAL;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chos5555Bot.Modules
{
    public class AddGame : ModuleBase<SocketCommandContext>
    {
        private BotRepository repo;

        public AddGame(BotRepository repo)
        {
            this.repo = repo;
        }

        /* 
         * Add command for basic games (without active role)
         */
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("addGame")]
        private async Task Command(IRole discordRole, [Remainder] string name)
        {
            // TODO: parse emote
            string emote = "<:heart:856258639177842708>";

            var guild = await repo.FindGuild(Context.Guild);
            Role role = new() { DisordId = discordRole.Id, Guild = guild };

            DAL.Model.Game game = new()
            {
                Name = name,
                ActiveEmote = emote,
                Guild = guild
            };

            // TODO: Make rooms only accessible with game role
            var discordTextRoom = await Context.Guild.CreateTextChannelAsync(name);
            var discordVoiceRoom = await Context.Guild.CreateVoiceChannelAsync(name);

            Room textRoom = new() { DiscordId = discordTextRoom.Id };
            Room voiceRoom = new() { DiscordId = discordVoiceRoom.Id };

            game.Rooms.Add(textRoom);
            game.Rooms.Add(voiceRoom);

            await repo.AddRole(role);

            game.GameRole = role;
            await repo.UpdateGuild(guild);

            var selectionRoom = guild.SelectionRoom;

            await GameAnnouncer.AnnounceGame(game, selectionRoom, Context);
        }

        /* TODO: Add extended command
         * Extended command for games with active roles
         */
    }
}
