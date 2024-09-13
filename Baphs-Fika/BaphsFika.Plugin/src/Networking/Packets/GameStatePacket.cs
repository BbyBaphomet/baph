using LiteNetLib.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace BaphsFika.Plugin.Networking.Packets
{
    public class GameStatePacket : INetSerializable
    {
        public enum EntityType { Player, Bot, Item }

        public struct EntityState
        {
            public int EntityId;
            public EntityType Type;
            public Vector3 Position;
            public Quaternion Rotation;
            public float Health;
            public string AdditionalData; // JSON string for type-specific data

            public void Serialize(NetDataWriter writer)
            {
                writer.Put(EntityId);
                writer.Put((byte)Type);
                writer.Put(Position);
                writer.Put(Rotation);
                writer.Put(Health);
                writer.Put(AdditionalData);
            }

            public void Deserialize(NetDataReader reader)
            {
                EntityId = reader.GetInt();
                Type = (EntityType)reader.GetByte();
                Position = reader.GetVector3();
                Rotation = reader.GetQuaternion();
                Health = reader.GetFloat();
                AdditionalData = reader.GetString();
            }
        }

        public List<EntityState> Entities { get; set; } = new List<EntityState>();
        public float ServerTime { get; set; }
        public uint SequenceNumber { get; set; }
        public bool IsDeltaCompressed { get; set; }
        public uint Checksum { get; set; }
        public string WeatherState { get; set; }
        public float TimeOfDay { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ServerTime);
            writer.Put(SequenceNumber);
            writer.Put(IsDeltaCompressed);
            writer.Put(Checksum);
            writer.Put(WeatherState);
            writer.Put(TimeOfDay);
            writer.Put(Entities.Count);
            foreach (var entity in Entities)
            {
                entity.Serialize(writer);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            ServerTime = reader.GetFloat();
            SequenceNumber = reader.GetUInt();
            IsDeltaCompressed = reader.GetBool();
            Checksum = reader.GetUInt();
            WeatherState = reader.GetString();
            TimeOfDay = reader.GetFloat();
            int count = reader.GetInt();
            Entities.Clear();
            for (int i = 0; i < count; i++)
            {
                var entity = new EntityState();
                entity.Deserialize(reader);
                Entities.Add(entity);
            }
        }
    }
}