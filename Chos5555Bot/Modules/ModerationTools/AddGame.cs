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
        private readonly BotRepository repo;

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
            Console.Write($"Add game command with role: {discordRole} and remainder: {name}\n");
            // TODO: parse emote
            string emote = "<:heart:856258639177842708>";

            var guild = await repo.FindGuild(Context.Guild);
            Role role = new() { DisordId = discordRole.Id };

            DAL.Model.Game game = new()
            {
                Name = name,
                ActiveEmote = Emote.Parse(emote),
                Guild = guild
            };            

            // TODO: Make rooms only accessible with game role
            var gameCategory = await Context.Guild.CreateCategoryChannelAsync(name);
            // Deny viewing channel for everyone role
            await gameCategory.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole,
                OverwritePermissions.DenyAll(gameCategory).Modify(viewChannel: PermValue.Deny));
            // Allow viewing channel for game role
            await gameCategory.AddPermissionOverwriteAsync(discordRole,
                OverwritePermissions.InheritAll.Modify(viewChannel: PermValue.Allow));

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

            await repo.AddRole(role);

            game.GameRole = role;
            await repo.UpdateGuild(guild);

            await GameAnnouncer.AnnounceGame(game, guild.SelectionRoom, Context);
        }

        /* TODO: Add extended command
         * Extended command for games with active roles
         */
    }
}
