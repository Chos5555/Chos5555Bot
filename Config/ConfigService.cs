using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;

namespace Config
{
    public class ConfigService
    {
        public Config GetConfig()
        {
#if DEBUG
            var file = "../../../../Chos5555Bot/Config.json";
#else
            var file = "../Chos5555Bot/Config.json";
#endif

            var data = File.ReadAllText(file);
            var result =  JsonConvert.DeserializeObject<Config>(data);
            return result;
        }
    }
}
