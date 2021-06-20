using Newtonsoft.Json;
using System.IO;

namespace Config
{
    public class ConfigService
    {
        public Config GetConfig()
        {
            var file = "../../../Config.json";
            var data = File.ReadAllText(file);
            var result =  JsonConvert.DeserializeObject<Config>(data);
            return result;
        }
    }
}
