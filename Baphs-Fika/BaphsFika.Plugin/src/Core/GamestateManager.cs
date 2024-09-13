  using UnityEngine;
  using BaphsFika.Plugin.Networking.Packets;
  using BaphsFika.Plugin.Networking;
  using System.Collections.Generic;
  using EFT;
  using BaphsFika.Plugin.Models;
  using LiteNetLib;
using BaphsFika.Core.Networking;

namespace BaphsFika.Plugin.Core
{
    public class GameStateManager : MonoBehaviour
    {
        private LobbyManager _lobbyManager;
        private NetworkIntegrationManager _networkIntegrationManager;
        private Client _clientNetworkManager;
        private Dictionary<int, PlayerState> _playerStates;
        private Dictionary<int, BaphsFika.Plugin.Models.BotState> _botStates;
        private GameStatePacket _currentGameState;

        public void Initialize(LobbyManager lobbyManager, NetworkIntegrationManager networkIntegrationManager, Client clientNetworkManager)
        {
            _lobbyManager = lobbyManager;
            _networkIntegrationManager = networkIntegrationManager;
            _clientNetworkManager = clientNetworkManager;
            _playerStates = new Dictionary<int, PlayerState>();
            _botStates = new Dictionary<int, BaphsFika.Plugin.Models.BotState>();

            _lobbyManager.OnAllPlayersReady += HandleAllPlayersReady;
            _clientNetworkManager.OnGameStartReceived += HandleGameStartReceived;
        }
          public void UpdatePlayerState(PlayerStatePacket packet)
          {
              if (_playerStates.TryGetValue(packet.PlayerId, out PlayerState state))
              {
                  state.InterpolateState(packet, Time.deltaTime);
              }
              else
              {
                  _playerStates[packet.PlayerId] = new PlayerState(packet);
              }
        }

        public void UpdateBotState(BotStatePacket packet)
        {
            if (_botStates.TryGetValue(packet.BotId, out BaphsFika.Plugin.Models.BotState state))
            {
                state.InterpolateState(packet.Position, packet.Rotation, packet.Behavior);
            }
            else
            {
                _botStates[packet.BotId] = new BaphsFika.Plugin.Models.BotState(packet);
            }
        }

        public void ProcessWeaponFire(WeaponFirePacket packet)
        {
            // Implement weapon fire logic
        }

        public void ProcessItemInteraction(ItemInteractionPacket packet)
        {
            // Implement item interaction logic
        }

        private void HandleAllPlayersReady()
        {
            if (_lobbyManager.IsHost())
            {
                RequestGameStart();
            }
        }

        private void RequestGameStart()
        {
            LobbyInfo currentLobby = _lobbyManager.GetCurrentLobby();
            GameStartRequestPacket gameStartRequestPacket = new GameStartRequestPacket
            {
                LobbyId = currentLobby.LobbyId
            };
            _clientNetworkManager.SendPacket(gameStartRequestPacket);
        }

        private void HandleGameStartReceived(GameStartPacket gameStartPacket)
        {
            InitializeGameWorld(gameStartPacket);
            TransitionToGameplay();
        }

        private void InitializeGameWorld(GameStartPacket gameStartPacket)
        {
            SynchronizeGameState(gameStartPacket.InitialGameState);
            SpawnPlayers(gameStartPacket.Players);
            InitializeGameSystems(gameStartPacket);
        }

        private void SynchronizeGameState(InitialGameState initialState)
        {
            _currentGameState = new GameState(initialState);
            _networkIntegrationManager.SynchronizeEnvironmentState(initialState.EnvironmentState);
        }

        private void SpawnPlayers(List<PlayerInitialState> playerStates)
        {
            foreach (var playerState in playerStates)
            {
                _networkIntegrationManager.SpawnOrUpdatePlayer(playerState);
            }
        }

        private void InitializeGameSystems(GameStartPacket gameStartPacket)
        {
            _networkIntegrationManager.InitializeGameSystems(gameStartPacket.SystemStates);
        }

        private void TransitionToGameplay()
        {
            _lobbyManager.HideLobbyUI();
            _networkIntegrationManager.EnableGameplayMode();
        }

        public void Update()
        {
            InterpolateStates();
            SendStateUpdates();
        }

        private void InterpolateStates()
        {
            foreach (var playerState in _playerStates.Values)
            {
                playerState.Interpolate(Time.deltaTime);
            }

            foreach (var botState in _botStates.Values)
            {
                botState.Interpolate(Time.deltaTime);
            }
        }

        private void SendStateUpdates()
        {
            if (_lobbyManager.IsHost())
            {
                GameStatePacket statePacket = new GameStatePacket
                {
                    GameState = _currentGameState,
                    PlayerStates = new List<PlayerState>(_playerStates.Values),
                    BotStates = new List<BaphsFika.Plugin.Models.BotState>(_botStates.Values)
                };
                _clientNetworkManager.BroadcastPacket(statePacket);
            }
        }
    }
}