using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Model;
using Discord;

namespace DAL
{
    public class BotRepository
    {
        public async Task AddGuild(Guild guild)
        {
            using (var db = new BotDbContext())
            {
                await db.Guilds.AddAsync(guild);
                await db.SaveChangesAsync();
            }
        }

        public async Task RemoveGuild(Guild guild)
        {
            using (var db = new BotDbContext())
            {
                db.Guilds.Remove(guild);
                await db.SaveChangesAsync();
            }
        }

        public async Task<Guild> FindGuild(IGuild guild)
        {
            using (var db = new BotDbContext())
            {
                return db.Guilds.AsQueryable()
                    .Where(g => g.DiscordId == guild.Id)
                    .FirstOrDefault();
            }
        }
        
        public async Task UpdateGuild(Guild guild)
        {
            using (var db = new BotDbContext())
            {
                var currGuild = await db.Guilds.FirstAsync(g => g.DiscordId == guild.DiscordId);
                currGuild.Queue = guild.Queue;
                currGuild.Roles = guild.Roles;
                currGuild.SelectionRoom = guild.SelectionRoom;
                await db.SaveChangesAsync();
            }
        }

        public async Task AddRole(Role role)
        {
            using (var db = new BotDbContext())
            {
                await db.Roles.AddAsync(role);
                await db.SaveChangesAsync();
            }
        }

        public async Task RemoveRole(Role role)
        {
            using (var db = new BotDbContext())
            {
                db.Roles.Remove(role);
                await db.SaveChangesAsync();
            }
        }

        public async Task<Role> FindRoleByGame(Model.Game game)
        {
            using (var db = new BotDbContext())
            {
                return db.Roles.AsQueryable()
                    .Where(r => r.Game == game)
                    .FirstOrDefault();
            }
        }

        public async Task AddRoom(Room room)
        {
            using (var db = new BotDbContext())
            {
                await db.Rooms.AddAsync(room);
                await db.SaveChangesAsync();
            }
        }

        public async Task RemoveRoom(Room room)
        {
            using (var db = new BotDbContext())
            {
                db.Rooms.Remove(room);
                await db.SaveChangesAsync();
            }
        }

        public async Task<Room> FindRoom(IChannel channel)
        {
            using (var db = new BotDbContext())
            {
                return db.Rooms.AsQueryable()
                    .Where(r => r.DiscordId == channel.Id)
                    .FirstOrDefault();
            }
        }

        public async Task UpdateRoom(Room room)
        {
            using (var db = new BotDbContext())
            {
                var currRoom = await db.Rooms.FirstAsync(r => r.DiscordId == room.DiscordId);
                currRoom.IsSelectionRoom = room.IsSelectionRoom;
                await db.SaveChangesAsync();
            }
        }

        public async Task AddSong(Song song)
        {
            using (var db = new BotDbContext())
            {
                await db.Songs.AddAsync(song);
                await db.SaveChangesAsync();
            }
        }

        public async Task RemoveSong(Song song)
        {
            using (var db = new BotDbContext())
            {
                db.Songs.Remove(song);
                await db.SaveChangesAsync();
            }
        }
        public async Task AddGame(Model.Game game)
        {
            using (var db = new BotDbContext())
            {
                await db.Games.AddAsync(game);
                await db.SaveChangesAsync();
            }
        }

        public async Task RemoveGame(Model.Game game)
        {
            using (var db = new BotDbContext())
            {
                db.Games.Remove(game);
                await db.SaveChangesAsync();
            }
        }

        public async Task UpdateGame(Model.Game game)
        {
            using (var db = new BotDbContext())
            {
                var currGame = await db.Games.FirstAsync(g => g.Id == game.Id);
                currGame.Emote = game.Emote;
                currGame.MessageId = game.MessageId;
                currGame.Name = game.Name;
                await db.SaveChangesAsync();
            }
        }

        public async Task<Model.Game> FindGameByMessage(ulong messageId)
        {
            using (var db = new BotDbContext())
            {
                return db.Games.AsQueryable()
                    .Where(g => g.MessageId == messageId)
                    .FirstOrDefault();
            }
        }
    }
}
