
using System;
using UnityEngine;
using BaphsFika.Plugin.Utils;
using LiteNetLib;
using BaphsFika.Plugin.Core;

namespace BaphsFika.Plugin.Models
{
    [Serializable]
    public class PlayerState
    {
        public string PlayerId { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public float Health { get; set; }
        public float MaxHealth { get; set; }
        public Vector3 Velocity { get; set; }
        public string CurrentAction { get; set; }
        public bool IsAiming { get; set; }
        public bool IsFiring { get; set; }
        public float LastUpdateTime { get; set; }
        public float Latency { get; set; }

        // Simplified inventory representation
        public string[] Inventory { get; set; }
        public string[] EquippedItems { get; set; }

        public PlayerState Clone()
        {
            return new PlayerState
            {
                PlayerId = this.PlayerId,
                Position = this.Position,
                Rotation = this.Rotation,
                Health = this.Health,
                MaxHealth = this.MaxHealth,
                Velocity = this.Velocity,
                CurrentAction = this.CurrentAction,
                IsAiming = this.IsAiming,
                IsFiring = this.IsFiring,
                LastUpdateTime = this.LastUpdateTime,
                Latency = this.Latency,
                Inventory = (string[])this.Inventory.Clone(),
                EquippedItems = (string[])this.EquippedItems.Clone()
            };
        }
          public void InterpolateState(PlayerStatePacket packet, float t)
          {
              Position = Vector3.Lerp(Position, packet.Position ?? Position, t);
              Rotation = Quaternion.Slerp(Rotation, packet.Rotation ?? Rotation, t);
              Velocity = Vector3.Lerp(Velocity, packet.Velocity ?? Velocity, t);
              // Interpolate other properties as needed
          }
        public string Serialize()
        {
            return JsonSerializer.Serialize(this);
        }

        public static PlayerState Deserialize(string json)
        {
            return JsonSerializer.Deserialize<PlayerState>(json);
        }

        public void UpdateFromPacket(PlayerStatePacket packet)
        {
            if (packet.Position.HasValue) Position = packet.Position.Value;
            if (packet.Rotation.HasValue) Rotation = packet.Rotation.Value;
            if (packet.Health.HasValue) Health = packet.Health.Value;
            if (packet.Velocity.HasValue) Velocity = packet.Velocity.Value;
            // Update other properties as needed
        }
    }
}