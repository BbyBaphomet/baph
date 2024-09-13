
using System;
using UnityEngine;
using BaphsFika.Plugin.Utils;

namespace BaphsFika.Plugin.Models
{
    [Serializable]
    public class BotState
    {
        public string Id { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public float Health { get; set; }
        public string BehaviorState { get; set; }
        public string BotType { get; set; }
        public int DifficultyLevel { get; set; }
        public string TargetPlayerId { get; set; }

        // Simplified inventory representation
        public string[] Equipment { get; set; }

        public BotState Clone()
        {
            return new BotState
            {
                Id = this.Id,
                Position = this.Position,
                Rotation = this.Rotation,
                Health = this.Health,
                BehaviorState = this.BehaviorState,
                BotType = this.BotType,
                DifficultyLevel = this.DifficultyLevel,
                TargetPlayerId = this.TargetPlayerId,
                Equipment = (string[])this.Equipment.Clone()
            };
        }

        public void InterpolateState(BotState targetState, float t)
        {
            Position = Vector3.Lerp(Position, targetState.Position, t);
            Rotation = Quaternion.Slerp(Rotation, targetState.Rotation, t);
            Health = Mathf.Lerp(Health, targetState.Health, t);
        }

        public string Serialize()
        {
            return JsonSerializer.Serialize(this);
        }

        public static BotState Deserialize(string json)
        {
            return JsonSerializer.Deserialize<BotState>(json);
        }
    }
}
