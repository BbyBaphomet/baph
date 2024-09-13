using LiteNetLib.Utils;
using UnityEngine;

namespace BaphsFika.Plugin.Networking.Packets
{
    public class WeaponFirePacket : INetSerializable
    {
        public int PlayerId { get; set; }
        public string WeaponId { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
        public float Timestamp { get; set; }
        public ushort SequenceNumber { get; set; }
        public float Recoil { get; set; }
        public float Spread { get; set; }
        public string AmmoType { get; set; }
        public float MuzzleVelocity { get; set; }
        public bool IsHit { get; set; }
        public Vector3? HitLocation { get; set; }
        public int BurstCount { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerId);
            writer.Put(WeaponId);
            writer.Put(Position);
            writer.Put(Direction);
            writer.Put(Timestamp);
            writer.Put(SequenceNumber);
            writer.Put(Recoil);
            writer.Put(Spread);
            writer.Put(AmmoType);
            writer.Put(MuzzleVelocity);
            writer.Put(IsHit);
            writer.Put(HitLocation.HasValue);
            if (HitLocation.HasValue) writer.Put(HitLocation.Value);
            writer.Put(BurstCount);
        }

        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetInt();
            WeaponId = reader.GetString();
            Position = reader.GetVector3();
            Direction = reader.GetVector3();
            Timestamp = reader.GetFloat();
            SequenceNumber = reader.GetUShort();
            Recoil = reader.GetFloat();
            Spread = reader.GetFloat();
            AmmoType = reader.GetString();
            MuzzleVelocity = reader.GetFloat();
            IsHit = reader.GetBool();
            if (reader.GetBool()) HitLocation = reader.GetVector3();
            BurstCount = reader.GetInt();
        }
    }
}