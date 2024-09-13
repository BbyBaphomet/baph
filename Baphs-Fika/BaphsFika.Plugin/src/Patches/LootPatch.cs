
using EFT;
using EFT.Interactive;
using BaphsFika.Plugin.Networking;
using BaphsFika.Plugin.Networking.Packets;
using Comfort.Common;
using System.Collections.Generic;
using UnityEngine;

namespace BaphsFika.Plugin.Patches
{
    public class LootPatch
    {
        private static ClientNetworkManager _networkManager;
        private static Dictionary<string, LootItem> _lootItems = new Dictionary<string, LootItem>();
        private static Dictionary<string, LootableContainer> _lootContainers = new Dictionary<string, LootableContainer>();

        public static void Initialize(ClientNetworkManager networkManager)
        {
            _networkManager = networkManager;
            _networkManager.RegisterPacketHandler<LootStatePacket>(HandleLootStatePacket);
            HookLootEvents();
        }

        private static void HookLootEvents()
        {
            // Hook into relevant loot events
            Singleton<GameWorld>.Instance.OnLootItemSpawned += HandleLootItemSpawned;
            Singleton<GameWorld>.Instance.OnLootContainerOpened += HandleLootContainerOpened;
        }

        private static void HandleLootItemSpawned(LootItem lootItem)
        {
            _lootItems[lootItem.Id] = lootItem;
            SendLootItemUpdate(lootItem, true);
        }

        private static void HandleLootContainerOpened(LootableContainer container)
        {
            _lootContainers[container.Id] = container;
            SendLootContainerUpdate(container);
        }

        private static void SendLootItemUpdate(LootItem lootItem, bool isSpawned)
        {
            LootStatePacket packet = new LootStatePacket
            {
                ItemId = lootItem.Id,
                TemplateId = lootItem.TemplateId,
                Position = lootItem.transform.position,
                IsSpawned = isSpawned
            };
            _networkManager.SendPacket(packet);
        }

        private static void SendLootContainerUpdate(LootableContainer container)
        {
            LootContainerStatePacket packet = new LootContainerStatePacket
            {
                ContainerId = container.Id,
                IsOpened = container.IsOpened,
                Position = container.transform.position
            };
            _networkManager.SendPacket(packet);
        }

        private static void HandleLootStatePacket(LootStatePacket packet)
        {
            if (packet.IsSpawned)
            {
                SpawnLootItem(packet);
            }
            else
            {
                DespawnLootItem(packet.ItemId);
            }
        }

        private static void SpawnLootItem(LootStatePacket packet)
        {
            if (!_lootItems.ContainsKey(packet.ItemId))
            {
                LootItem newLootItem = Singleton<GameWorld>.Instance.CreateLootItem(packet.TemplateId, packet.Position);
                _lootItems[packet.ItemId] = newLootItem;
            }
        }

        private static void DespawnLootItem(string itemId)
        {
            if (_lootItems.TryGetValue(itemId, out LootItem lootItem))
            {
                Object.Destroy(lootItem.gameObject);
                _lootItems.Remove(itemId);
            }
        }
    }
}
