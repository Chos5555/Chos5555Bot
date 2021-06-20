using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Model;

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
        public BotDbContext() : base()
        {
            _configService = new();
            _config = _configService.GetConfig();
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_config.ConnectionString)
                .UseLoggerFactory(LoggerFactory.Create(
                    builder =>
                    {
                        builder.AddFilter((category, level) => category == DbLoggerCategory.Database.Command.Name
                        && level == LogLevel.Information).AddConsole();
                    })).EnableSensitiveDataLogging();
        }
        /*
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>()
            .Property(r => r.Rooms)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
        }*/
    }
}
