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
using Config;

namespace DAL
{
    public class BotDbContext : DbContext
    {
        private readonly Configuration _config;

        public DbSet<Guild> Guilds { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<Game> Games { get; set; }
        
        //public BotDbContext(DbContextOptions options) : base(options) { }
        
        public BotDbContext() : base()
        {
            _config = Configuration.GetConfig();
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
            
            // Create conversions for EmoteEmoji type
            modelBuilder.Entity<Game>()
                .Property(g => g.ActiveEmote)
                .HasConversion(
                    e => e.emote.ToString(),
                    e => EmoteParser.ParseEmote(e));

            modelBuilder.Entity<Role>()
                .Property(r => r.ChoiceEmote)
                .HasConversion(
                    e => e.emote.ToString(),
                    e => EmoteParser.ParseEmote(e));

            // Set auto includes for guilds properties in other tables
            modelBuilder.Entity<Guild>().Navigation(g => g.SelectionRoom).AutoInclude();
            modelBuilder.Entity<Guild>().Navigation(g => g.MemberRole).AutoInclude();
            modelBuilder.Entity<Guild>().Navigation(g => g.RuleRoom).AutoInclude();

            // Set auto includes for games properties in other tables
            modelBuilder.Entity<Game>().Navigation(g => g.Guild).AutoInclude();
            modelBuilder.Entity<Game>().Navigation(g => g.Rooms).AutoInclude();
            modelBuilder.Entity<Game>().Navigation(g => g.GameRole).AutoInclude();
            modelBuilder.Entity<Game>().Navigation(g => g.ActiveRoles).AutoInclude();
            modelBuilder.Entity<Game>().Navigation(g => g.ActiveCheckRoom).AutoInclude();
            modelBuilder.Entity<Game>().Navigation(g => g.ModAcceptRoom).AutoInclude();
            modelBuilder.Entity<Game>().Navigation(g => g.ModAcceptRoles).AutoInclude();
        }
    }
}
