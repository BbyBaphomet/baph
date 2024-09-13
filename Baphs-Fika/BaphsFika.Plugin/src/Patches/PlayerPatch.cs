
using EFT;
using BaphsFika.Plugin.Networking;
using BaphsFika.Plugin.Networking.Packets;
using Comfort.Common;
using UnityEngine;
using System.Collections.Generic;

namespace BaphsFika.Plugin.Patches
{
    public class PlayerPatch
    {
        private static ClientNetworkManager _networkManager;
        private static Dictionary<int, Player> _networkPlayers = new Dictionary<int, Player>();
        private static float _updateInterval = 0.05f; // 20 updates per second
        private static float _lastUpdateTime;

        public static void Initialize(ClientNetworkManager networkManager)
        {
            _networkManager = networkManager;
            _networkManager.OnPlayerStateReceived += HandlePlayerStateReceived;
            HookPlayerEvents();
        }

        private static void HookPlayerEvents()
        {
            // Hook into relevant player events
            Singleton<GameWorld>.Instance.OnPlayerSpawned += HandlePlayerSpawned;
            Singleton<GameWorld>.Instance.OnPlayerDead += HandlePlayerDead;
        }

        private static void HandlePlayerSpawned(Player player)
        {
            _networkPlayers[player.Id] = player;
            SendPlayerState(player);
        }

        private static void HandlePlayerDead(Player player)
        {
            _networkPlayers.Remove(player.Id);
            SendPlayerDeathPacket(player);
        }

        private static void SendPlayerState(Player player)
        {
            PlayerStatePacket packet = new PlayerStatePacket
            {
                PlayerId = player.Id,
                Position = player.Transform.position,
                Rotation = player.Transform.rotation,
                Velocity = player.MovementContext.CharacterController.velocity,
                Health = player.HealthController.GetBodyPartHealth(EBodyPart.Common).Current,
                Timestamp = Time.time
            };
            _networkManager.SendPacket(packet);
        }

        private static void SendPlayerDeathPacket(Player player)
        {
            PlayerDeathPacket packet = new PlayerDeathPacket
            {
                PlayerId = player.Id,
                Position = player.Transform.position,
                Timestamp = Time.time
            };
            _networkManager.SendPacket(packet);
        }

        private static void HandlePlayerStateReceived(PlayerStatePacket packet)
        {
            if (_networkPlayers.TryGetValue(packet.PlayerId, out Player player))
            {
                UpdatePlayerState(player, packet);
            }
            else
            {
                CreateNetworkPlayer(packet);
            }
        }

        private static void UpdatePlayerState(Player player, PlayerStatePacket packet)
        {
            player.Transform.position = Vector3.Lerp(player.Transform.position, packet.Position, 0.5f);
            player.Transform.rotation = Quaternion.Slerp(player.Transform.rotation, packet.Rotation, 0.5f);
            player.MovementContext.CharacterController.velocity = packet.Velocity;
            player.HealthController.SetBodyPartHealth(EBodyPart.Common, packet.Health);
        }

        private static void CreateNetworkPlayer(PlayerStatePacket packet)
        {
            Player newPlayer = Singleton<GameWorld>.Instance.CreatePlayer(packet.PlayerId);
            _networkPlayers[packet.PlayerId] = newPlayer;
            UpdatePlayerState(newPlayer, packet);
        }

        public static void Update()
        {
            if (Time.time - _lastUpdateTime >= _updateInterval)
            {
                foreach (var player in _networkPlayers.Values)
                {
                    if (player == Singleton<GameWorld>.Instance.MainPlayer)
                    {
                        SendPlayerState(player);
                    }
                }
                _lastUpdateTime = Time.time;
            }
        }
    }
}
