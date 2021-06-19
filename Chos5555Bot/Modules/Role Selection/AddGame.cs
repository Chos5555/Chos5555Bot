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
        private BotRepository repo = new BotRepository();

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [Command("addGame")]
        private async Task Command(string args)
        {
            //TODO parse input arguments
            string emote = args.Split(' ').Last();
            IRole discordRole;
            string name;

            DAL.Model.Game game = new() { Name = name, Emote = emote };
            Role role = new() { Game = game, DisordId = discordRole.Id };

            var discordTextRoom = await Context.Guild.CreateTextChannelAsync(name);
            var discordVoiceRoom = await Context.Guild.CreateVoiceChannelAsync(name);

            Room textRoom = new() { DiscordId = discordTextRoom.Id };
            Room voiceRoom = new() { DiscordId = discordVoiceRoom.Id };

            role.Rooms.Add(textRoom);
            role.Rooms.Add(voiceRoom);

            await repo.AddGame(game);
            await repo.AddRole(role);

            var guild = await repo.FindGuild(Context.Guild);
            var selectionRoom = guild.SelectionRoom;

            await GameAnnouncer.AnnounceGame(role, selectionRoom, Context);
        }
    }
}
