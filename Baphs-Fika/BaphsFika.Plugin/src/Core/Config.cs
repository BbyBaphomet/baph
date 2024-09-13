
using BepInEx.Configuration;

namespace BaphsFika.Plugin.Core
{
    public class Config
    {
        public static ConfigEntry<string> ServerAddress { get; private set; }
        public static ConfigEntry<int> ServerPort { get; private set; }

        public static void Initialize(ConfigFile config)
        {
            ServerAddress = config.Bind("Network", "ServerAddress", "127.0.0.1", "The IP address of the server");
            ServerPort = config.Bind("Network", "ServerPort", 7777, "The port of the server");
        }
    }
}
