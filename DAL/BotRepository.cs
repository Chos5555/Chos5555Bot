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
            await using (var db = new BotDbContext())
            {
                return db.Guilds
                    .AsQueryable()
                    .Where(g => g.DiscordId == guild.Id)
                    .FirstOrDefault();
            }
        }

        public async Task<Guild> FindGuildById(ulong id)
        {
            await using (var db = new BotDbContext())
            {
                return db.Guilds
                    .AsQueryable()
                    .Where(g => g.DiscordId == id)
                    .FirstOrDefault();
            }
        }
        
        public async Task UpdateGuild(Guild guild)
        {
            using (var db = new BotDbContext())
            {
                db.Guilds.Update(guild);
                var currGuild = await db.Guilds
                    .AsQueryable()
                    .FirstAsync(g => g.DiscordId == guild.DiscordId);
                currGuild.SelectionRoom = guild.SelectionRoom;
                currGuild.GameCategoryId = guild.GameCategoryId;
                currGuild.ArchiveCategoryId = guild.ArchiveCategoryId;
                currGuild.RuleRoom = guild.RuleRoom;
                currGuild.RuleMessageText = guild.RuleMessageText;
                currGuild.RuleMessageId = guild.RuleMessageId;
                currGuild.Songs = guild.Songs;
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

        public async Task<ICollection<Role>> FindRoleByGame(Model.Game game)
        {
            await using (var db = new BotDbContext())
            {
                return db.Games
                    .AsQueryable()
                    .Where(g => g == game)
                    .Select(g => g.Roles)
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
            await using (var db = new BotDbContext())
            {
                return db.Rooms
                    .AsQueryable()
                    .Where(r => r.DiscordId == channel.Id)
                    .FirstOrDefault();
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
                var currGame = await db.Games.AsQueryable()
                    .FirstAsync(g => g.Id == game.Id);
                currGame.Name = game.Name;
                currGame.Guild = game.Guild;
                currGame.Emote = game.Emote;
                currGame.SelectionMessageId = game.SelectionMessageId;
                currGame.Rooms = game.Rooms;
                currGame.HaveActiveRole = game.HaveActiveRole;
                currGame.ActiveRole = game.ActiveRole;
                currGame.ActiveCheckRoom = game.ActiveCheckRoom;
                currGame.ModAcceptRoom = game.ModAcceptRoom;
                currGame.ModAcceptRoles = game.ModAcceptRoles;
                await db.SaveChangesAsync();
            }
        }

        public async Task<ICollection<Model.Game>> FingGamesByGuild(Guild guild)
        {
            await using (var db = new BotDbContext())
            {
                return db.Games
                    .AsQueryable()
                    .Where(g => g.Guild.DiscordId == guild.DiscordId)
                    .ToList();
            }
        }

        public async Task<Model.Game> FindGameByMessage(ulong messageId)
        {
            await using (var db = new BotDbContext())
            {
                return db.Games
                    .AsQueryable()
                    .Where(g => g.SelectionMessageId == messageId)
                    .FirstOrDefault();
            }
        }
    }
}
