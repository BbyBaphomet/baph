
using LiteNetLib.Utils;
using System;

namespace BaphsFika.Plugin.Networking.Packets
{
    public abstract class BasePacket : INetSerializable
    {
        public ushort PacketId { get; set; }
        public long Timestamp { get; set; }
        public ushort SequenceNumber { get; set; }

        protected BasePacket()
        {
            Timestamp = DateTime.UtcNow.Ticks;
        }

        public virtual void Serialize(NetDataWriter writer)
        {
            writer.Put(PacketId);
            writer.Put(Timestamp);
            writer.Put(SequenceNumber);
        }

        public virtual void Deserialize(NetDataReader reader)
        {
            PacketId = reader.GetUShort();
            Timestamp = reader.GetLong();
            SequenceNumber = reader.GetUShort();
        }
    }
}
