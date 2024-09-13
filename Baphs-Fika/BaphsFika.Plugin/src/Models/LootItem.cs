
using System;
using UnityEngine;
using BaphsFika.Plugin.Utils;

namespace BaphsFika.Plugin.Models
{
    [Serializable]
    public class LootItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Quantity { get; set; }
        public float Value { get; set; }
        public string Rarity { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public bool IsPickedUp { get; set; }

        public LootItem Clone()
        {
            return new LootItem
            {
                Id = this.Id,
                Name = this.Name,
                Type = this.Type,
                Quantity = this.Quantity,
                Value = this.Value,
                Rarity = this.Rarity,
                Position = this.Position,
                Rotation = this.Rotation,
                IsPickedUp = this.IsPickedUp
            };
        }

        public string Serialize()
        {
            return JsonSerializer.Serialize(this);
        }

        public static LootItem Deserialize(string json)
        {
            return JsonSerializer.Deserialize<LootItem>(json);
        }

        public bool CanPickUp(PlayerState player)
        {
            // Implement logic to check if the player can pick up this item
            // For example, check player's inventory capacity, item restrictions, etc.
            return !IsPickedUp;
        }

        public void PickUp(PlayerState player)
        {
            if (CanPickUp(player))
            {
                IsPickedUp = true;
                // Implement logic to add the item to the player's inventory
                // This might involve updating the player's state or inventory
            }
        }
    }
}
