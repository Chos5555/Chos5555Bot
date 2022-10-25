using Newtonsoft.Json;
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

            var data = File.ReadAllText(file);
            var result = JsonConvert.DeserializeObject<Configuration>(data);
            return result;
        }
    }
}
