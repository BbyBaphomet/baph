
using LiteNetLib.Utils;

namespace BaphsFika.Plugin.Networking.Packets
{
    public class ServerInfoPacket : INetSerializable
    {
        public string ServerName { get; set; }
        public int CurrentPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public string ServerVersion { get; set; }
        public string GameMode { get; set; }
        public string MapName { get; set; }
        public float TickRate { get; set; }
        public int AverageLatency { get; set; }
        public string ServerRules { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ServerName);
            writer.Put(CurrentPlayers);
            writer.Put(MaxPlayers);
            writer.Put(ServerVersion);
            writer.Put(GameMode);
            writer.Put(MapName);
            writer.Put(TickRate);
            writer.Put(AverageLatency);
            writer.Put(ServerRules);
        }

        public void Deserialize(NetDataReader reader)
        {
            ServerName = reader.GetString();
            CurrentPlayers = reader.GetInt();
            MaxPlayers = reader.GetInt();
            ServerVersion = reader.GetString();
            GameMode = reader.GetString();
            MapName = reader.GetString();
            TickRate = reader.GetFloat();
            AverageLatency = reader.GetInt();
            ServerRules = reader.GetString();
        }
    }
}
