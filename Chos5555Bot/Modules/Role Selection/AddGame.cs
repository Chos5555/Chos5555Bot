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

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [Command("addGame")]
        private async Task Command(IRole discordRole, [Remainder] string name)
        {
            // TODO: parse emote
            string emote = "<:heart:856258639177842708>";

            DAL.Model.Game game = new() { Name = name, Emote = emote };

            Role role = new() { DisordId = discordRole.Id, Game = game };

            var discordTextRoom = await Context.Guild.CreateTextChannelAsync(name);
            var discordVoiceRoom = await Context.Guild.CreateVoiceChannelAsync(name);

            Room textRoom = new() { DiscordId = discordTextRoom.Id };
            Room voiceRoom = new() { DiscordId = discordVoiceRoom.Id };

            role.Rooms.Add(textRoom);
            role.Rooms.Add(voiceRoom);

            await repo.AddRole(role);

            var guild = await repo.FindGuild(Context.Guild);

            guild.Roles.Add(role);
            await repo.UpdateGuild(guild);

            var selectionRoom = guild.SelectionRoom;

            await GameAnnouncer.AnnounceGame(role, selectionRoom, Context);
        }
    }
}
