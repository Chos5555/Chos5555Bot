using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.EntityFrameworkCore;
using Game = DAL.Model.Game;

namespace DAL
{
    /// <summary>
    /// Class for all methods that interact with the database
    /// </summary>
    public class BotRepository
    {
        private readonly BotDbContext _context;

        public BotRepository(BotDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Add new guild to DB
        /// </summary>
        /// <param name="guild">Guild to be added</param>
        /// <returns>Nothing</returns>
        public async Task AddGuild(Guild guild)
        {
            await _context.Guilds.AddAsync(guild);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Remove give guild from the DB
        /// </summary>
        /// <param name="guild">Guild to be removed</param>
        /// <returns>Nothing</returns>
        public async Task RemoveGuild(Guild guild)
        {
            _context.Guilds.Remove(guild);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Find given guild in DB
        /// </summary>
        /// <param name="guild">Guild to be found</param>
        /// <returns>Guild</returns>
        public async Task<Guild> FindGuild(Guild guild)
        {
            return await _context.Guilds.FindAsync(guild.Id);
        }

        /// <summary>
        /// Find given discord guild in DB
        /// </summary>
        /// <param name="guild">Discord guild to be found</param>
        /// <returns>Guild</returns>
        public async Task<Guild> FindGuild(IGuild guild)
        {
            return await _context.Guilds
                .AsQueryable()
                .Where(g => g.DiscordId == guild.Id)
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Find guild by given discord guild Id in DB
        /// </summary>
        /// <param name="id">Id of the dicsord guild to be found</param>
        /// <returns>Guild</returns>
        public async Task<Guild> FindGuild(ulong id)
        {
            return await _context.Guilds
                .AsQueryable()
                .Where(g => g.DiscordId == id)
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Finds guild by Id of given stage channel
        /// </summary>
        /// <param name="id">If of the stage channel</param>
        /// <returns>Guild</returns>
        public async Task<Guild> FindGuildByStageChannel(ulong id)
        {
            var room = await FindRoom(id);

            return await _context.Guilds
                .AsQueryable()
                .Where(g => g.StageChannels.Contains(room))
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Updates guild in DB
        /// </summary>
        /// <param name="guild">Guild to be updated</param>
        /// <returns>Nothing</returns>
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
            currGuild.UserLeaveMessageRoomId = guild.UserLeaveMessageRoomId;
            currGuild.StageChannels = guild.StageChannels;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Adds new role to DB
        /// </summary>
        /// <param name="role">Role to be added</param>
        /// <returns>Nothing</returns>
        public async Task AddRole(Role role)
        {
            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Removes given role from DB
        /// </summary>
        /// <param name="role">Role to be deleted</param>
        /// <returns>Nothing</returns>
        public async Task RemoveRole(Role role)
        {
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Finds give role in DB
        /// </summary>
        /// <param name="role">Role to be found</param>
        /// <returns>Role</returns>
        public async Task<Role> FindRole(Role role)
        {
            return await _context.Roles.FindAsync(role.Id);
        }

        /// <summary>
        /// Finds role by given discord role id in DB
        /// </summary>
        /// <param name="id">Id of discord role to be found</param>
        /// <returns>Role</returns>
        public async Task<Role> FindRole(ulong id)
        {
            return await _context.Roles.AsQueryable()
                .Where(r => r.DisordId == id)
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Finds given discord role in DB
        /// </summary>
        /// <param name="role">Discord role to be found</param>
        /// <returns>Role</returns>
        public async Task<Role> FindRole(IRole role)
        {
            return await FindRole(role.Id);
        }

        /// <summary>
        /// Updates given role in DB
        /// </summary>
        /// <param name="role">Role to be updated</param>
        /// <returns>Nothing</returns>
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

        /// <summary>
        /// Finds role by given game in DB
        /// </summary>
        /// <param name="game">Game whose role is to be found </param>
        /// <returns>Role</returns>
        public async Task<Role> FindGameRoleByGame(Game game)
        {
            return await _context.Games
                .AsQueryable()
                .Where(g => g == game)
                .Select(g => g.GameRole)
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Finds active roles of a given game in DB
        /// </summary>
        /// <param name="game">Game whose roles are to be found</param>
        /// <returns>Active roles of a game</returns>
        public async Task<ICollection<Role>> FindActiveRolesByGame(Game game)
        {
            return (await FindGame(game)).ActiveRoles;
        }

        /// <summary>
        /// Finds all roles connected to a game in DB
        /// </summary>
        /// <param name="game">Game whose roles are to be found</param>
        /// <returns>All role of a game</returns>
        public async Task<ICollection<Role>> FindAllRolesByGame(Game game)
        {
            var res = new List<Role>();
            game = await FindGame(game);

            res.Add(game.GameRole);
            res.AddRange(game.ActiveRoles);
            res.AddRange(game.ModAcceptRoles);

            return res;
        }

        /// <summary>
        /// Finds Ids of all roles of a game in DB
        /// </summary>
        /// <param name="game">Game whose role Ids are to be found </param>
        /// <returns>All Ids of game's roles</returns>
        public async Task<ICollection<ulong>> FindAllRoleIdsByGame(Game game)
        {
            var res = new List<ulong>();
            foreach (Role role in await FindAllRolesByGame(game))
            {
                res.Add(role.DisordId);
            }
            return res;
        }

        /// <summary>
        /// Finds role belonging to given game with a given emote as ChoiceEmote
        /// </summary>
        /// <param name="emote">Emote of the role to be found</param>
        /// <param name="game">Game to which the role belongs</param>
        /// <returns>Role</returns>
        public async Task<Role> FindRoleByEmoteAndGame(IEmote emote, Game game)
        {
            return (await FindActiveRolesByGame(game))
                .Where(r => r.ChoiceEmote == emote)
                .SingleOrDefault();
        }

        /// <summary>
        /// Adds new room to DB
        /// </summary>
        /// <param name="room">Room to be added</param>
        /// <returns>Nothing</returns>
        public async Task AddRoom(Room room)
        {
            await _context.Rooms.AddAsync(room);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Removes given room from DB
        /// </summary>
        /// <param name="room">Room to be removed</param>
        /// <returns>Void</returns>
        public async Task RemoveRoom(Room room)
        {
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Finds given room in DB
        /// </summary>
        /// <param name="room">Room to be found</param>
        /// <returns>Room</returns>
        public async Task<Room> FindRoom(Room room)
        {
            return await _context.Rooms.FindAsync(room.Id);
        }

        /// <summary>
        /// Finds given discord channel in DB
        /// </summary>
        /// <param name="channel">Channel to be found</param>
        /// <returns>Room</returns>
        public async Task<Room> FindRoom(IChannel channel)
        {
            return await _context.Rooms
                .AsQueryable()
                .Where(r => r.DiscordId == channel.Id)
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Finds channel with given discordId in DB
        /// </summary>
        /// <param name="id">Id of channel to be found</param>
        /// <returns>Room</returns>
        public async Task<Room> FindRoom(ulong id)
        {
            return await _context.Rooms
                .AsQueryable()
                .Where(r => r.DiscordId == id)
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Adds new song to DB
        /// </summary>
        /// <param name="song">Song to be added</param>
        /// <returns>Nothing</returns>
        public async Task AddSong(Song song)
        {
            await _context.Songs.AddAsync(song);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Removes given song from DB
        /// </summary>
        /// <param name="song">Song to be removed</param>
        /// <returns>Nothing</returns>
        public async Task RemoveSong(Song song)
        {
            _context.Songs.Remove(song);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Adds new game to DB
        /// </summary>
        /// <param name="game">Game to be added</param>
        /// <returns>Nothing</returns>
        public async Task AddGame(Game game)
        {
            await _context.Games.AddAsync(game);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Removes given game from the DB
        /// </summary>
        /// <param name="game">Game to be removed</param>
        /// <returns>Nothing</returns>
        public async Task RemoveGame(Game game)
        {
            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates given game in DB
        /// </summary>
        /// <param name="game">Game to be updated</param>
        /// <returns>Nothing</returns>
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

        /// <summary>
        /// Finds given game in DB
        /// </summary>
        /// <param name="game">Game to be found</param>
        /// <returns>Game</returns>
        public async Task<Game> FindGame(Game game)
        {
            return await _context.Games.FindAsync(game.Id);
        }

        /// <summary>
        /// Finds all games belonging to a guild
        /// </summary>
        /// <param name="guild">Guild whos games are to be found</param>
        /// <returns>All games of a guild</returns>
        public async Task<ICollection<Game>> FingGamesByGuild(Guild guild)
        {
            return await _context.Games
                .AsQueryable()
                .Where(g => g.Guild.DiscordId == guild.DiscordId)
                .ToListAsync();
        }

        /// <summary>
        /// Finds game that has given SelectionMessageId
        /// </summary>
        /// <param name="messageId">SelectionMessage Id</param>
        /// <returns>Game</returns>
        public async Task<Game> FindGameBySelectionMessage(ulong messageId)
        {
            return await _context.Games
                .AsQueryable()
                .Where(g => g.SelectionMessageId == messageId)
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Finds game that has given ModRoom set as its own
        /// </summary>
        /// <param name="channelId">ModRoom Id</param>
        /// <returns>Game</returns>
        public async Task<Game> FindGameByModRoom(ulong channelId)
        {
            return await _context.Games
                .AsQueryable()
                .Where(g => g.ModAcceptRoom.DiscordId == channelId)
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Finds game that has given given ActiveCheckRoom set as its own
        /// </summary>
        /// <param name="channelId">ActiveCheckRoom Id</param>
        /// <returns>Game</returns>
        public async Task<Game> FindGameByActiveCheckRoom(ulong channelId)
        {
            return await _context.Games
                .AsQueryable()
                .Where(g => g.ActiveCheckRoom.DiscordId == channelId)
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Finds game which has given role in ActiveRoles
        /// </summary>
        /// <param name="role">ActiveRole</param>
        /// <returns>Game</returns>
        public async Task<Game> FindGameByRole(Role role)
        {
            return await _context.Games
                .AsQueryable()
                .Where(g => (g.ActiveRoles.Where(r => r.Id == role.Id)).Any())
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Finds game which has given role as GameRole
        /// </summary>
        /// <param name="role">ActiveRole</param>
        /// <returns>Game</returns>
        public async Task<Game> FindGameByGameRole(Role role)
        {
            return await _context.Games
                .AsQueryable()
                .Where(g => (g.GameRole.Id == role.Id))
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Finds game which has given room in Rooms
        /// </summary>
        /// <param name="room">Room</param>
        /// <returns>Game</returns>
        public async Task<Game> FindGameByRoom(Room room)
        {
            return await _context.Games
                .AsQueryable()
                .Where(g => (g.Rooms.Where(r => r.Id == room.Id)).Any())
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Finds game by given name and Id of GameRole
        /// </summary>
        /// <param name="name">Name of the game</param>
        /// <param name="roleId">Id of GameRole</param>
        /// <returns>Game</returns>
        public async Task<Game> FindGameByNameAndGameRole(string name, ulong roleId)
        {
            return await _context.Games
                .AsQueryable()
                .Where(g => g.Name == name && g.GameRole.DisordId == roleId)
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Finds game by Name in DB
        /// </summary>
        /// <param name="name">Name of the game to be found</param>
        /// <returns>Game</returns>
        public async Task<Game> FindGame(string name)
        {
            return await _context.Games
                .Where(g => g.Name == name)
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Finds a game that either has the same Name or GameRole in DB
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="roleId">Id of GameRole</param>
        /// <returns>Game</returns>
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
