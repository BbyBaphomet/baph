using LiteNetLib.Utils;
using System;

namespace BaphsFika.Plugin.Networking.Packets
{
    public enum InteractionType
    {
        Pickup,
        Drop,
        Use,
        Examine
    }

    public class ItemInteractionPacket : INetSerializable
    {
        public int PlayerId { get; set; }
        public string ItemId { get; set; }
        public string LocationId { get; set; }
        public bool IsAdded { get; set; }
        public long Timestamp { get; set; }
        public InteractionType InteractionType { get; set; }
        public int Quantity { get; set; }
        public string ItemTemplateId { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerId);
            writer.Put(ItemId);
            writer.Put(LocationId);
            writer.Put(IsAdded);
            writer.Put(Timestamp);
            writer.Put((byte)InteractionType);
            writer.Put(Quantity);
            writer.Put(ItemTemplateId);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetInt();
            ItemId = reader.GetString();
            LocationId = reader.GetString();
            IsAdded = reader.GetBool();
            Timestamp = reader.GetLong();
            InteractionType = (InteractionType)reader.GetByte();
            Quantity = reader.GetInt();
            ItemTemplateId = reader.GetString();
        }
    }
}