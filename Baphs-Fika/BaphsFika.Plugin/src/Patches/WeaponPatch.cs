
using EFT;
using EFT.InventoryLogic;
using BaphsFika.Plugin.Networking;
using BaphsFika.Plugin.Networking.Packets;
using Comfort.Common;
using UnityEngine;
using System.Collections.Generic;

namespace BaphsFika.Plugin.Patches
{
    public class WeaponPatch
    {
        private static ClientNetworkManager _networkManager;
        private static Dictionary<string, Weapon> _activeWeapons = new Dictionary<string, Weapon>();

        public static void Initialize(ClientNetworkManager networkManager)
        {
            _networkManager = networkManager;
            _networkManager.RegisterPacketHandler<WeaponStatePacket>(HandleWeaponStatePacket);
            _networkManager.RegisterPacketHandler<WeaponFirePacket>(HandleWeaponFirePacket);
            HookWeaponEvents();
        }

        private static void HookWeaponEvents()
        {
            // Hook into relevant weapon events
            Singleton<GameWorld>.Instance.OnWeaponFired += HandleWeaponFired;
            Singleton<GameWorld>.Instance.OnWeaponReloaded += HandleWeaponReloaded;
        }

        private static void HandleWeaponFired(Weapon weapon, Player player, AmmoTemplate ammo)
        {
            WeaponFirePacket packet = new WeaponFirePacket
            {
                WeaponId = weapon.Id,
                PlayerId = player.Id,
                Position = player.Transform.position,
                Direction = player.Transform.forward,
                AmmoType = ammo.TemplateId,
                Timestamp = Time.time
            };
            _networkManager.SendPacket(packet);
        }

        private static void HandleWeaponReloaded(Weapon weapon, Player player)
        {
            SendWeaponState(weapon, player);
        }

        private static void SendWeaponState(Weapon weapon, Player player)
        {
            WeaponStatePacket packet = new WeaponStatePacket
            {
                WeaponId = weapon.Id,
                PlayerId = player.Id,
                CurrentMagazineCount = weapon.GetCurrentMagazineCount(),
                ChamberLoaded = weapon.ChamberAmmoCount > 0,
                FireMode = weapon.FireMode,
                Durability = weapon.Durability,
                Timestamp = Time.time
            };
            _networkManager.SendPacket(packet);
        }

        private static void HandleWeaponStatePacket(WeaponStatePacket packet)
        {
            if (_activeWeapons.TryGetValue(packet.WeaponId, out Weapon weapon))
            {
                UpdateWeaponState(weapon, packet);
            }
        }

        private static void HandleWeaponFirePacket(WeaponFirePacket packet)
        {
            Player player = Singleton<GameWorld>.Instance.GetPlayerByProfileId(packet.PlayerId);
            if (player != null && _activeWeapons.TryGetValue(packet.WeaponId, out Weapon weapon))
            {
                SimulateWeaponFire(weapon, player, packet);
            }
        }

        private static void UpdateWeaponState(Weapon weapon, WeaponStatePacket packet)
        {
            weapon.SetCurrentMagazineCount(packet.CurrentMagazineCount);
            weapon.ChamberAmmoCount = packet.ChamberLoaded ? 1 : 0;
            weapon.FireMode = packet.FireMode;
            weapon.Durability = packet.Durability;
        }

        private static void SimulateWeaponFire(Weapon weapon, Player player, WeaponFirePacket packet)
        {
            // Implement weapon fire simulation logic
            // This might include creating bullet trajectories, applying recoil, etc.
        }

        public static void RegisterWeapon(Weapon weapon)
        {
            _activeWeapons[weapon.Id] = weapon;
        }

        public static void UnregisterWeapon(string weaponId)
        {
            _activeWeapons.Remove(weaponId);
        }
    }
}
