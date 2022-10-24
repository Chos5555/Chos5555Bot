﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Game = DAL.Model.Game;

namespace DAL
{
    public class BotRepository
    {
        private readonly BotDbContext _context;

        public BotRepository(BotDbContext ctx)
        {
            _context = ctx;
        }

        public async Task AddGuild(Guild guild)
        {
            await _context.Guilds.AddAsync(guild);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveGuild(Guild guild)
        {
            _context.Guilds.Remove(guild);
            await _context.SaveChangesAsync();
        }

        public async Task<Guild> FindGuild(Guild guild)
        {
            return await _context.Guilds.FindAsync(guild.Id);
        }

        public async Task<Guild> FindGuild(IGuild guild)
        {
            return await _context.Guilds
                .AsQueryable()
                .Where(g => g.DiscordId == guild.Id)
                .SingleOrDefaultAsync();
        }

        public async Task<Guild> FindGuild(ulong id)
        {
            return await _context.Guilds
                .AsQueryable()
                .Where(g => g.DiscordId == id)
                .SingleOrDefaultAsync();
        }

        public async Task UpdateGuild(Guild guild)
        {
            _context.Guilds.Update(guild);
            var currGuild = await FindGuild(guild);
            currGuild.SelectionRoom = guild.SelectionRoom;
            currGuild.MemberRole = guild.MemberRole;
            currGuild.ArchiveCategoryId = guild.ArchiveCategoryId;
            currGuild.RuleRoom = guild.RuleRoom;
            currGuild.RuleMessageText = guild.RuleMessageText;
            currGuild.RuleMessageId = guild.RuleMessageId;
            currGuild.Songs = guild.Songs;
            await _context.SaveChangesAsync();
        }

        public async Task AddRole(Role role)
        {
            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveRole(Role role)
        {
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
        }

        public async Task<Role> FindRole(Role role)
        {
            return await _context.Roles.FindAsync(role.Id);
        }

        public async Task<Role> FindRole(ulong id)
        {
            return await _context.Roles.AsQueryable()
                .Where(r => r.DisordId == id)
                .SingleOrDefaultAsync();
        }

        public async Task<Role> FindRole(IRole role)
        {
            return await FindRole(role.Id);
        }

        public async Task UpdateRole(Role role)
        {
            _context.Roles.Update(role);
            var currRole = await FindRole(role);
            currRole.DisordId = role.DisordId;
            currRole.Name = role.Name;
            currRole.Resettable = role.Resettable;
            currRole.NeedsModApproval = role.NeedsModApproval;
            currRole.ChoiceEmote = role.ChoiceEmote;
            currRole.Description = role.Description;
            await _context.SaveChangesAsync();
        }

        public async Task<Role> FindGameRoleByGame(Game game)
        {
            return await _context.Games
                .AsQueryable()
                .Where(g => g == game)
                .Select(g => g.GameRole)
                .SingleOrDefaultAsync();
        }

        public async Task<ICollection<Role>> FindActiveRolesByGame(Game game)
        {
            return (await FindGame(game)).ActiveRoles;
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
            await _context.Rooms.AddAsync(room);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveRoom(Room room)
        {
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
        }

        public async Task<Room> FindRoom(Room room)
        {
            return await _context.Rooms.FindAsync(room.Id);
        }

        public async Task<Room> FindRoom(IChannel channel)
        {
            return await _context.Rooms
                .AsQueryable()
                .Where(r => r.DiscordId == channel.Id)
                .SingleOrDefaultAsync();
        }

        public async Task AddSong(Song song)
        {
            await _context.Songs.AddAsync(song);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveSong(Song song)
        {
            _context.Songs.Remove(song);
            await _context.SaveChangesAsync();
        }
        public async Task AddGame(Game game)
        {
            await _context.Games.AddAsync(game);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveGame(Game game)
        {
            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
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
            currGame.MainActiveRole = game.MainActiveRole;
            currGame.ActiveRoles = game.ActiveRoles;
            currGame.ActiveCheckRoom = game.ActiveCheckRoom;
            currGame.ModAcceptRoom = game.ModAcceptRoom;
            currGame.ModAcceptRoles = game.ModAcceptRoles;
            await _context.SaveChangesAsync();
        }

        public async Task<Game> FindGame(Game game)
        {
            return await _context.Games.FindAsync(game.Id);
        }

        public async Task<ICollection<Game>> FingGamesByGuild(Guild guild)
        {
            return await _context.Games
                .AsQueryable()
                .Where(g => g.Guild.DiscordId == guild.DiscordId)
                .ToListAsync();
        }

        public async Task<Game> FindGameBySelectionMessage(ulong messageId)
        {
            return await _context.Games
                .AsQueryable()
                .Where(g => g.SelectionMessageId == messageId)
                .SingleOrDefaultAsync();
        }

        public async Task<Game> FindGameByModRoom(ulong channelId)
        {
            return await _context.Games
                .AsQueryable()
                .Where(g => g.ModAcceptRoom.DiscordId == channelId)
                .SingleOrDefaultAsync();
        }

        public async Task<Game> FindGameByActiveCheckRoom(ulong channelId)
        {
            return await _context.Games
                .AsQueryable()
                .Where(g => g.ActiveCheckRoom.DiscordId == channelId)
                .SingleOrDefaultAsync();
        }

        public async Task<Game> FindGameByRole(Role role)
        {
            return await _context.Games
                .AsQueryable()
                .Where(g => (g.ActiveRoles.Where(r => r.Id == role.Id)).Any())
                .SingleOrDefaultAsync();
        }

        public async Task<Game> FindGameByRoom(Room room)
        {
            return await _context.Games
                .AsQueryable()
                .Where(g => (g.Rooms.Where(r => r.Id == room.Id)).Any())
                .SingleOrDefaultAsync();
        }

        public async Task<Game> FindGameByNameAndGameRole(string name, ulong roleId)
        {
            return await _context.Games
                .AsQueryable()
                .Where(g => g.Name == name && g.GameRole.DisordId == roleId)
                .SingleOrDefaultAsync();
        }

        public async Task<Game> FindGame(string name)
        {
            return await _context.Games
                .Where(g => g.Name == name)
                .SingleOrDefaultAsync();
        }

        public bool FindDuplicateGame(string name, ulong roleId)
        {
            return _context.Games
                .AsQueryable()
                .Where(g => g.Name == name || g.GameRole.DisordId == roleId)
                .Any();
        }

        // TODO: When making FindSongs of a guild, use Include() to get songs from Songs table
    }
}
