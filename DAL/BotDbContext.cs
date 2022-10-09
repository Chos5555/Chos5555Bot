using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Model;
using Discord;
using Game = DAL.Model.Game;
using DAL.Misc;

namespace DAL
{
    public class BotDbContext : DbContext
    {

        private readonly Config.ConfigService _configService;
        private readonly Config.Config _config;

        public DbSet<Guild> Guilds { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<Game> Games { get; set; }
        
        //public BotDbContext(DbContextOptions options) : base(options) { }
        
        public BotDbContext() : base()
        {
            _configService = new();
            _config = _configService.GetConfig();
            Database.EnsureCreated();
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=BotDB");
            optionsBuilder.UseSqlServer(_config.ConnectionString);/*
             .UseLoggerFactory(LoggerFactory.Create(
                 builder =>
                 {
                     builder.AddFilter((category, level) => category == DbLoggerCategory.Database.Command.Name
                     && level == LogLevel.Information).AddConsole();
                 })).EnableSensitiveDataLogging();
            Console.WriteLine("Database was connected!!");*/
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Game>()
                .Property(g => g.ActiveEmote)
                .HasConversion(
                    e => e.emote.ToString(),
                    e => EmoteParser.ParseEmote(e));

            modelBuilder.Entity<Role>()
                .Property(r => r.Emote)
                .HasConversion(
                    e => e.emote.ToString(),
                    e => EmoteParser.ParseEmote(e));
        }
    }
}
