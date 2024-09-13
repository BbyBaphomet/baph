
using EFT;
using BaphsFika.Plugin.Networking;
using BaphsFika.Plugin.Networking.Packets;
using Comfort.Common;
using System.Collections.Generic;
using UnityEngine;

namespace BaphsFika.Plugin.Patches
{
    public class AIPatch
    {
        private static Dictionary<int, BotOwner> _networkBots = new Dictionary<int, BotOwner>();
        private static ClientNetworkManager _networkManager;

        public static void Initialize(ClientNetworkManager networkManager)
        {
            _networkManager = networkManager;
            _networkManager.OnBotStateReceived += HandleBotStateReceived;
        }

        private static void HandleBotStateReceived(BotStatePacket packet)
        {
            if (_networkBots.TryGetValue(packet.BotId, out BotOwner botOwner))
            {
                UpdateBotState(botOwner, packet.State);
            }
            else
            {
                CreateNetworkBot(packet);
            }
        }

        private static void UpdateBotState(BotOwner botOwner, BotState state)
        {
            botOwner.Transform.position = state.Position;
            botOwner.Transform.rotation = state.Rotation;
            botOwner.HealthController.SetHealth(EBodyPart.Common, state.Health);
            botOwner.BotBrainController.SetBehaviorState(state.BehaviorState);
            
            if (state.CurrentPath != null && state.CurrentPath.Length > 0)
            {
                botOwner.BotBrainController.SetPath(state.CurrentPath);
            }

            botOwner.BotBrainController.SetCombatDecision(state.CurrentCombatDecision);
        }

        private static void CreateNetworkBot(BotStatePacket packet)
        {
            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            BotOwner newBot = gameWorld.CreateBot(packet.BotId);
            _networkBots[packet.BotId] = newBot;
            UpdateBotState(newBot, packet.State);
        }
    }
}
