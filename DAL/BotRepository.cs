using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Model;
using Discord;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class BotRepository
    {
        private readonly BotDbContext context;
        
        public BotRepository()
        {
            context = new BotDbContext(new DbContextOptions<BotDbContext>());
        }
        // TODO: Add context in dependency injection?? (research)
        public BotRepository(BotDbContext ctx)
        {
            context = ctx;
        }

        public async Task AddGuild(Guild guild)
        {
                await context.Guilds.AddAsync(guild);
                await context.SaveChangesAsync();
        }

        public async Task RemoveGuild(Guild guild)
        {
                context.Guilds.Remove(guild);
                await context.SaveChangesAsync();
        }

        public async Task<Guild> FindGuild(IGuild guild)
        {
                return context.Guilds
                    .AsQueryable()
                    .Where(g => g.DiscordId == guild.Id)
                    .FirstOrDefault();
        }

        public async Task<Guild> FindGuildById(ulong id)
        {
                return context.Guilds
                    .AsQueryable()
                    .Where(g => g.DiscordId == id)
                    .FirstOrDefault();
        }

        public async Task UpdateGuild(Guild guild)
        {
                context.Guilds.Update(guild);
                var currGuild = await context.Guilds
                    .AsQueryable()
                    .FirstAsync(g => g.DiscordId == guild.DiscordId);
                currGuild.SelectionRoom = guild.SelectionRoom;
                currGuild.MemberRole = guild.MemberRole;
                currGuild.ArchiveCategoryId = guild.ArchiveCategoryId;
                currGuild.RuleRoom = guild.RuleRoom;
                currGuild.RuleMessageText = guild.RuleMessageText;
                currGuild.RuleMessageId = guild.RuleMessageId;
                currGuild.Songs = guild.Songs;
                await context.SaveChangesAsync();
        }

        public async Task AddRole(Role role)
        {
                await context.Roles.AddAsync(role);
                await context.SaveChangesAsync();
        }

        public async Task RemoveRole(Role role)
        {
                context.Roles.Remove(role);
                await context.SaveChangesAsync();
        }

        public async Task<Role> FindGameRoleByGame(Model.Game game)
        {
                return context.Games
                    .AsQueryable()
                    .Where(g => g == game)
                    .Select(g => g.GameRole)
                    .FirstOrDefault();
        }

        public async Task<ICollection<Role>> FindAllRolesByGame(Model.Game game)
        {
                var res = new List<Role>();
                var roles = context.Games
                    .AsQueryable()
                    .Where(g => g == game)
                    .FirstOrDefault();

                res.Add(roles.GameRole);
                res.AddRange(roles.ActiveRoles);
                res.AddRange(roles.ModAcceptRoles);

                return res;
        }

        public async Task<ICollection<ulong>> FindAllRoleIdsByGame(Model.Game game)
        {
                var res = new List<ulong>();
                foreach (Role role in await FindAllRolesByGame(game))
                {
                    res.Add(role.DisordId);
                }
                return res;
        }
        
        public async Task<Role> FindRoleByGameAndGuild (IEmote emote, ulong guildId)
        {
                return context.Roles
                    .AsQueryable()
                    .Where(r => r.Guild.DiscordId == guildId)
                    .Where(r => r.Emote == emote)
                    .FirstOrDefault();
        }

        public async Task AddRoom(Room room)
        {
                await context.Rooms.AddAsync(room);
                await context.SaveChangesAsync();
        }

        public async Task RemoveRoom(Room room)
        {
                context.Rooms.Remove(room);
                await context.SaveChangesAsync();
        }

        public async Task<Room> FindRoom(IChannel channel)
        {
                return context.Rooms
                    .AsQueryable()
                    .Where(r => r.DiscordId == channel.Id)
                    .FirstOrDefault();
        }

        public async Task AddSong(Song song)
        {
                await context.Songs.AddAsync(song);
                await context.SaveChangesAsync();
        }

        public async Task RemoveSong(Song song)
        {
                context.Songs.Remove(song);
                await context.SaveChangesAsync();
        }
        public async Task AddGame(Model.Game game)
        {
                await context.Games.AddAsync(game);
                await context.SaveChangesAsync();
        }

        public async Task RemoveGame(Model.Game game)
        {
                context.Games.Remove(game);
                await context.SaveChangesAsync();
        }

        public async Task UpdateGame(Model.Game game)
        {
                var currGame = await context.Games.AsQueryable()
                    .FirstAsync(g => g.Id == game.Id);
                currGame.Name = game.Name;
                currGame.Guild = game.Guild;
                currGame.ActiveEmote = game.ActiveEmote;
                currGame.SelectionMessageId = game.SelectionMessageId;
                currGame.Rooms = game.Rooms;
                currGame.GameRole = game.GameRole;
                currGame.HasActiveRole = game.HasActiveRole;
                currGame.ActiveRoles = game.ActiveRoles;
                currGame.ActiveCheckRoom = game.ActiveCheckRoom;
                currGame.ModAcceptRoom = game.ModAcceptRoom;
                currGame.ModAcceptRoles = game.ModAcceptRoles;
                await context.SaveChangesAsync();
        }

        public async Task<ICollection<Model.Game>> FingGamesByGuild(Guild guild)
        {
                return context.Games
                    .AsQueryable()
                    .Where(g => g.Guild.DiscordId == guild.DiscordId)
                    .ToList();
        }

        public async Task<Model.Game> FindGameBySelectionMessage(ulong messageId)
        {
                return context.Games
                    .AsQueryable()
                    .Where(g => g.SelectionMessageId == messageId)
                    .FirstOrDefault();
        }

        public async Task<Model.Game> FindGameByModRoom(ulong channelId)
        {
                return context.Games
                    .AsQueryable()
                    .Where(g => g.ModAcceptRoom.DiscordId == channelId)
                    .FirstOrDefault();
        }

        public async Task<Model.Game> FindGameByActiveCheckRoom(ulong channelId)
        {
                return context.Games
                    .AsQueryable()
                    .Where(g => g.ActiveCheckRoom.DiscordId == channelId)
                    .FirstOrDefault();
        }
    }
}
