  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using UnityEngine;
  using BaphsFika.Plugin.Networking;
  using BaphsFika.Plugin.Models;

  namespace BaphsFika.Plugin.Core
  {
      public class LobbyManager : MonoBehaviour
      {
          [SerializeField] private NetworkManager _networkManager;
          private Dictionary<string, Lobby> _lobbies;
          private Lobby _currentLobby;

          public event Action OnAllPlayersReady;
          public event Action<Lobby> OnLobbyJoined;
          public event Action OnLobbyLeft;
          public event Action<LobbySettings> OnLobbySettingsUpdated;

          private void Awake()
          {
              _lobbies = new Dictionary<string, Lobby>();
          }

          public async Task<bool> CreateLobbyAsync(string lobbyName, LobbySettings settings)
          {
              string lobbyId = Guid.NewGuid().ToString();
              Lobby newLobby = new Lobby(lobbyId, lobbyName, settings);
              _lobbies[lobbyId] = newLobby;
              _currentLobby = newLobby;
              return await _networkManager.CreateLobbyAsync(newLobby);
          }

          public async Task<bool> JoinLobbyAsync(string lobbyId)
          {
              if (_lobbies.TryGetValue(lobbyId, out Lobby lobby))
              {
                  if (await _networkManager.JoinLobbyAsync(lobbyId))
                  {
                      _currentLobby = lobby;
                      OnLobbyJoined?.Invoke(lobby);
                      return true;
                  }
              }
              Debug.LogError($"Failed to join lobby with ID {lobbyId}.");
              return false;
          }

          public async Task LeaveLobbyAsync()
          {
              if (_currentLobby != null)
              {
                  await _networkManager.LeaveLobbyAsync(_currentLobby.Id);
                  _currentLobby = null;
                  OnLobbyLeft?.Invoke();
              }
          }

          public void SetPlayerReady(string playerId, bool isReady)
          {
              if (_currentLobby != null)
              {
                  _currentLobby.SetPlayerReady(playerId, isReady);
                  CheckAllPlayersReady();
              }
          }

          private void CheckAllPlayersReady()
          {
              if (_currentLobby != null && _currentLobby.AreAllPlayersReady())
              {
                  OnAllPlayersReady?.Invoke();
              }
          }

          public async Task UpdateLobbySettingsAsync(LobbySettings newSettings)
          {
              if (_currentLobby != null)
              {
                  _currentLobby.UpdateSettings(newSettings);
                  await _networkManager.UpdateLobbySettingsAsync(_currentLobby);
                  OnLobbySettingsUpdated?.Invoke(newSettings);
              }
          }

          public List<Lobby> GetAvailableLobbies()
          {
              return new List<Lobby>(_lobbies.Values);
          }

          public Lobby GetCurrentLobby()
          {
              return _currentLobby;
          }

          public bool IsHost()
          {
              return _currentLobby != null && _currentLobby.HostId == _networkManager.LocalPlayerId;
          }

          public void HideLobbyUI()
          {
              // Implement UI hiding logic
          }
      }

      public class Lobby
      {
          public string Id { get; }
          public string Name { get; }
          public string HostId { get; private set; }
          public LobbySettings Settings { get; private set; }
          public IReadOnlyDictionary<string, PlayerInfo> Players => _players;

          private Dictionary<string, PlayerInfo> _players;

          public Lobby(string id, string name, LobbySettings settings)
          {
              Id = id;
              Name = name;
              Settings = settings;
              _players = new Dictionary<string, PlayerInfo>();
          }

          public void SetPlayerReady(string playerId, bool isReady)
          {
              if (_players.TryGetValue(playerId, out PlayerInfo player))
              {
                  player.IsReady = isReady;
              }
          }

          public bool AreAllPlayersReady()
          {
              return _players.Values.All(p => p.IsReady);
          }

          public void UpdateSettings(LobbySettings newSettings)
          {
              Settings = newSettings;
          }
      }

      public class LobbySettings
      {
          public int MaxPlayers { get; set; }
          public string GameMode { get; set; }
          public string MapName { get; set; }
          public bool IsPrivate { get; set; }
      }

      public class PlayerInfo
      {
          public string Id { get; set; }
          public string Name { get; set; }
          public bool IsReady { get; set; }
      }
  }
