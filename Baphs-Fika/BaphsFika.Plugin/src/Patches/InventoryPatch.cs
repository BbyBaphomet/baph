
using EFT;
using EFT.InventoryLogic;
using BaphsFika.Plugin.Networking;
using BaphsFika.Plugin.Networking.Packets;
using Comfort.Common;
using System.Collections.Generic;
using UnityEngine;

namespace BaphsFika.Plugin.Patches
{
    public class InventoryPatch
    {
        private static ClientNetworkManager _networkManager;

        public static void Initialize(ClientNetworkManager networkManager)
        {
            _networkManager = networkManager;
            HookInventoryEvents();
        }

        private static void HookInventoryEvents()
        {
            // Hook into relevant inventory events
            Singleton<GameWorld>.Instance.ItemFactory.OnItemAddedOrRemovedEvent += HandleItemAddedOrRemoved;
        }

        private static void HandleItemAddedOrRemoved(Item item, ItemAddress location, bool added)
        {
            ItemInteractionPacket packet = new ItemInteractionPacket
            {
                PlayerId = Singleton<GameWorld>.Instance.MainPlayer.Id,
                ItemId = item.Id,
                LocationId = location.ToString(),
                IsAdded = added,
                ItemTemplateId = item.TemplateId
            };

            _networkManager.SendPacket(packet);
        }

        public static void ProcessItemInteraction(ItemInteractionPacket packet)
        {
            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            Player player = gameWorld.GetPlayerByProfileId(packet.PlayerId);

            if (player == null)
            {
                Debug.LogWarning($"Player not found for ID: {packet.PlayerId}");
                return;
            }

            if (packet.IsAdded)
            {
                AddItemToInventory(player, packet);
            }
            else
            {
                RemoveItemFromInventory(player, packet);
            }
        }

        private static void AddItemToInventory(Player player, ItemInteractionPacket packet)
        {
            Item item = Singleton<GameWorld>.Instance.ItemFactory.CreateItem(packet.ItemTemplateId);
            ItemAddress location = new ItemAddress(packet.LocationId);
            player.Inventory.TryAddItem(item, location, out _);
        }

        private static void RemoveItemFromInventory(Player player, ItemInteractionPacket packet)
        {
            Item item = player.Inventory.GetAllItems().Find(i => i.Id == packet.ItemId);
            if (item != null)
            {
                player.Inventory.TryRemove(item, out _);
            }
        }
    }
}
