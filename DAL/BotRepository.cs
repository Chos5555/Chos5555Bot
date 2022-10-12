using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Model;
using Discord;
using Microsoft.EntityFrameworkCore;
using Game = DAL.Model.Game;

namespace DAL
{
    public class BotRepository
    {
        private readonly BotDbContext context;

        public BotRepository()
        {
            context = new BotDbContext();
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

        public async Task<Guild> FindGuild(Guild guild)
        {
            return await context.Guilds.FindAsync(guild.Id);
        }

        public async Task<Guild> FindGuild(IGuild guild)
        {
            return await context.Guilds
                .AsQueryable()
                .Where(g => g.DiscordId == guild.Id)
                .SingleOrDefaultAsync();
        }

        public async Task<Guild> FindGuild(ulong id)
        {
            return await context.Guilds
                .AsQueryable()
                .Where(g => g.DiscordId == id)
                .SingleOrDefaultAsync();
        }

        public async Task UpdateGuild(Guild guild)
        {
            context.Guilds.Update(guild);
            var currGuild = await FindGuild(guild);
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

        public async Task<Role> FindRole(Role role)
        {
            return await context.Roles.FindAsync(role.Id);
        }

        public async Task UpdateRole(Role role)
        {
            context.Roles.Update(role);
            var currRole = await FindRole(role);
            currRole.DisordId = role.DisordId;
            currRole.Resetable = role.Resetable;
            currRole.NeedsModApproval = role.NeedsModApproval;
            currRole.ChoiceEmote = role.ChoiceEmote;
            currRole.Description = role.Description;
            await context.SaveChangesAsync();
        }

        public async Task<Role> FindGameRoleByGame(Game game)
        {
            return await context.Games
                .AsQueryable()
                .Where(g => g == game)
                .Select(g => g.GameRole)
                .SingleOrDefaultAsync();
        }

        public async Task<ICollection<Role>> FindActiveRolesByGame(Game game)
        {
            return (await context.Games
                .AsQueryable()
                .Where(g => g == game)
                .SingleOrDefaultAsync())
                .ActiveRoles;
        }

        public async Task<ICollection<Role>> FindAllRolesByGame(Game game)
        {
            var res = new List<Role>();
            game = await FindGame(game);

            res.Add(game.GameRole);
            res.AddRange(game.ActiveRoles);
            res.AddRange(game.ModAcceptRoles);

            return res;
        }

        public async Task<ICollection<ulong>> FindAllRoleIdsByGame(Game game)
        {
            var res = new List<ulong>();
            foreach (Role role in await FindAllRolesByGame(game))
            {
                res.Add(role.DisordId);
            }
            return res;
        }

        public async Task<Role> FindRoleByEmoteAndGame(IEmote emote, Game game)
        {
            return (await FindActiveRolesByGame(game))
                .Where(r => r.ChoiceEmote == emote)
                .SingleOrDefault();
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
            return await context.Rooms
                .AsQueryable()
                .Where(r => r.DiscordId == channel.Id)
                .SingleOrDefaultAsync();
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
        public async Task AddGame(Game game)
        {
            await context.Games.AddAsync(game);
            await context.SaveChangesAsync();
        }

        public async Task RemoveGame(Game game)
        {
            context.Games.Remove(game);
            await context.SaveChangesAsync();
        }

        public async Task UpdateGame(Game game)
        {
            var currGame = await FindGame(game);
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

        public async Task<Game> FindGame(Game game)
        {
            return await context.Games.FindAsync(game.Id);
        }

        public async Task<ICollection<Game>> FingGamesByGuild(Guild guild)
        {
            return context.Games
                .AsQueryable()
                .Where(g => g.Guild.DiscordId == guild.DiscordId)
                .ToList();
        }

        public async Task<Game> FindGameBySelectionMessage(ulong messageId)
        {
            return await context.Games
                .AsQueryable()
                .Where(g => g.SelectionMessageId == messageId)
                .SingleOrDefaultAsync();
        }

        public async Task<Game> FindGameByModRoom(ulong channelId)
        {
            return await context.Games
                .AsQueryable()
                .Where(g => g.ModAcceptRoom.DiscordId == channelId)
                .SingleOrDefaultAsync();
        }

        public async Task<Game> FindGameByActiveCheckRoom(ulong channelId)
        {
            return await context.Games
                .AsQueryable()
                .Where(g => g.ActiveCheckRoom.DiscordId == channelId)
                .SingleOrDefaultAsync();
        }

        // TODO: When making FindSongs of a guild, use Include() to get songs from Songs table
    }
}
