﻿using Microsoft.EntityFrameworkCore;
using System;
using Game = DAL.Model.Game;
using DAL.Misc;
using Config;
using System.Linq;
using DAL.Model;

namespace DAL
{
    /// <summary>
    /// Database context for the bot
    /// </summary>
    public class BotDbContext : DbContext
    {
        private readonly Configuration _config;

        // Define all tables in model of DB
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Quest> Quests { get; set; }
        public DbSet<CompletedQuests> CompletedQuests { get; set; }
        public DbSet<GameActivity> GameActivities { get; set; }

        public BotDbContext() : base()
        {
            _config = Configuration.GetConfig();

            var pendingMigrations = Database.GetPendingMigrations();
            if (pendingMigrations.Any())
            {
                Console.WriteLine($"There are {pendingMigrations.Count()} pending migrations. Migrating DB.");
                // Apply all migrations
                Database.Migrate();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Connect the database
            optionsBuilder.UseNpgsql(_config.ConnectionString);
            Console.WriteLine($"{_config.DBType} DB connected.");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Create conversions for EmoteEmoji type
            modelBuilder.Entity<Game>()
                .Property(g => g.ActiveEmote)
                .HasConversion(
                    e => e.Emote.ToString(),
                    e => EmoteParser.ParseEmote(e));

            modelBuilder.Entity<Role>()
                .Property(r => r.ChoiceEmote)
                .HasConversion(
                    e => e.Emote.ToString(),
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
            modelBuilder.Entity<Game>().Navigation(g => g.ModQuestRoom).AutoInclude();

            // Set auto includes for users properties in other tables
            modelBuilder.Entity<User>().Navigation(u => u.CompletedQuests).AutoInclude();
            modelBuilder.Entity<User>().Navigation(u => u.GameActivities).AutoInclude();
        }
    }
}
