using Newtonsoft.Json;
using System;
using System.IO;

namespace Config
{
    public class Configuration
    {
        public string Token { get; set; }
        public string ConnectionString { get; set; }
        public char Prefix { get; set; }

        public static Configuration GetConfig()
        {
#if DEBUG
            var file = "../../../../Chos5555Bot/Config.json";
#else
            var file = "../Chos5555Bot/Config.json";
#endif

            Configuration result = null;
            try
            {
                var data = File.ReadAllText(file);
                result = JsonConvert.DeserializeObject<Configuration>(data);
            }
            catch (Exception _)
            {
                // If config file is not present, take values from env variables
                result = new Configuration();
                result.Token = Environment.GetEnvironmentVariable("TOKEN");
                result.Prefix = (Environment.GetEnvironmentVariable("PREFIX"))[0];

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
