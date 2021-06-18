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
        // TODO: Nemit connectionString tady ve zdrojaku

        private string connectionString =
            @"server=(localdb)\MSSQLLocalDB; 
        Initial Catalog=BotDB; Integrated Security=true";

        public DbSet<Guild> Guilds { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Song> Songs { get; set; }
        public DbSet<Game> Games { get; set; }
        public BotDbContext() : base()
        {
            //TODO: remove deleted before submission
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString)
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
            modelBuilder.Entity<Game>().HasData(
                new Game
                {
                    Name = "Foxhole",
                    Emote = "❤️"
                });
        }*/
    }
}
