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
            var file = "Chos5555Bot/Config.json";
#endif

            Configuration result = null;
            try
            {
                var data = File.ReadAllText(file);
                result = JsonConvert.DeserializeObject<Configuration>(data);
            }
            catch (Exception e)
            {
                // If config file is not present, take values from env variables
                result = new Configuration();
                result.Token = Environment.GetEnvironmentVariable("Token");
                result.ConnectionString = Environment.GetEnvironmentVariable("ConnectionString");
                result.Prefix = Environment.GetEnvironmentVariable("Prefix")[0];
            }

            return result;
        }
    }
}
