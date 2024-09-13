using UnityEngine;
using System;
using LiteNetLib.Utils;

namespace BaphsFika.Plugin.Networking.Packets
{
    [Serializable]
    public class BotStatePacket : INetSerializable
    {
        public int BotId { get; set; }
        public BotState State { get; set; }
        public float Timestamp { get; set; }
        public ushort SequenceNumber { get; set; }
        public bool IsCompressed { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(BotId);
            writer.Put(Timestamp);
            writer.Put(SequenceNumber);
            writer.Put(IsCompressed);
            State.Serialize(writer);
        }

        public void Deserialize(NetDataReader reader)
        {
            BotId = reader.GetInt();
            Timestamp = reader.GetFloat();
            SequenceNumber = reader.GetUShort();
            IsCompressed = reader.GetBool();
            State = new BotState();
            State.Deserialize(reader);
        }
    }

    [Serializable]
    public class BotState : INetSerializable
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Velocity { get; set; }
        public float Health { get; set; }
        public BotBehaviorState BehaviorState { get; set; }
        public Vector3[] CurrentPath { get; set; }
        public BotCombatDecision CurrentCombatDecision { get; set; }
        public string EquipmentStatus { get; set; }
        public string InventoryStatus { get; set; }
        public int? TargetId { get; set; }
        public int DifficultyLevel { get; set; }
        public string AIType { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Position);
            writer.Put(Rotation);
            writer.Put(Velocity);
            writer.Put(Health);
            writer.Put((byte)BehaviorState);
            writer.Put(CurrentPath.Length);
            foreach (var point in CurrentPath)
            {
                writer.Put(point);
            }
            writer.Put((byte)CurrentCombatDecision);
            writer.Put(EquipmentStatus);
            writer.Put(InventoryStatus);
            writer.Put(TargetId.HasValue);
            if (TargetId.HasValue) writer.Put(TargetId.Value);
            writer.Put(DifficultyLevel);
            writer.Put(AIType);
        }

        public void Deserialize(NetDataReader reader)
        {
            Position = reader.GetVector3();
            Rotation = reader.GetQuaternion();
            Velocity = reader.GetVector3();
            Health = reader.GetFloat();
            BehaviorState = (BotBehaviorState)reader.GetByte();
            int pathLength = reader.GetInt();
            CurrentPath = new Vector3[pathLength];
            for (int i = 0; i < pathLength; i++)
            {
                CurrentPath[i] = reader.GetVector3();
            }
            CurrentCombatDecision = (BotCombatDecision)reader.GetByte();
            EquipmentStatus = reader.GetString();
            InventoryStatus = reader.GetString();
            if (reader.GetBool())
                TargetId = reader.GetInt();
            else
                TargetId = null;
            DifficultyLevel = reader.GetInt();
            AIType = reader.GetString();
        }
    }

    public enum BotBehaviorState
    {
        Idle,
        Patrolling,
        Investigating,
        Combating,
        Fleeing
    }

    public enum BotCombatDecision
    {
        Engage,
        TakeCover,
        Retreat,
        CallForBackup
    }
}