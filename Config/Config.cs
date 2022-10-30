using Newtonsoft.Json;
using System;
using System.IO;

namespace Config
{
    /// <summary>
    /// Class Configuration contains all configs for external connections, including lavalink, db connection,
    /// discord client token.
    /// </summary>
    public class Configuration
    {
        public string Token { get; set; }
        public string ConnectionString { get; set; }
        public string LavalinkHostname { get; set; }
        public ushort LavalinkPort { get; set; }
        public string LavalinkPassword { get; set; }
        public DatabaseType DBType { get; set; }

        public enum DatabaseType
        {
            Local,
            External
        }

        /// <summary>
        /// Fills an instance of Configuration with configs needed for the bot.
        /// Configs can be either loaded from a Config.json file or from env variables.
        /// If the bot is run in debug mode, it will run in the bin/debug/... folder, so Config.json is further out,
        /// otherwise, it's in the main folder.
        /// </summary>
        /// <returns> Returns new configuration filled with configs. </returns>
        public static Configuration GetConfig()
        {
#if DEBUG
            var file = "../../../../Config.json";
#else
            var file = "../Config.json";
#endif

            Configuration result = null;
            try
            {
                var data = File.ReadAllText(file);
                result = JsonConvert.DeserializeObject<Configuration>(data);
                result.DBType = DatabaseType.Local;
            }
            catch (Exception _)
            {
                // If config file is not present, take values from env variables
                result = new Configuration();
                result.Token = Environment.GetEnvironmentVariable("TOKEN");
                result.DBType = DatabaseType.External;

                // Get Lavalink configs
                result.LavalinkHostname = Environment.GetEnvironmentVariable("LAVALINK_HOSTNAME");
                result.LavalinkPort = ushort.Parse(Environment.GetEnvironmentVariable("LAVALINK_PORT"));
                result.LavalinkPassword = Environment.GetEnvironmentVariable("LAVALINK_PASSWORD");

                if (Environment.GetEnvironmentVariable("CONNECTION_STRING") is not null)
                {
                    result.ConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
                }
                else
                {
                    // Transform Heroku Postgres DB ULR to connection string
                    var herokuConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

                    herokuConnectionString = herokuConnectionString.Replace("postgres://", "");
                    var pgCredentials = herokuConnectionString.Split("@")[0];
                    var pgDBUrl = herokuConnectionString.Split("@")[1];

                    var pgUser = pgCredentials.Split(":")[0];
                    var pgPassword = pgCredentials.Split(":")[1];

                    var pgHostPort = pgDBUrl.Split("/")[0];
                    var pgDatabaseName = pgDBUrl.Split("/")[1];

                    var pgHost = pgHostPort.Split(":")[0];
                    var pgPort = pgHostPort.Split(":")[1]; ;

                    result.ConnectionString = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPassword};Database={pgDatabaseName}";
                }
            }
            return result;
        }
    }
}
