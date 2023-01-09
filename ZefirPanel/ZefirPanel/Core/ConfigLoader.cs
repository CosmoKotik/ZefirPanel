using Newtonsoft.Json;

namespace ZefirPanel.Core
{
    public class ConfigLoader
    {
        public static Config Load()
        {
            string lines = System.IO.File.ReadAllText(@"config.conf");

            Config conf = new Config();

            conf = JsonConvert.DeserializeObject<Config>(lines);

            return conf;
        }
    }
}
