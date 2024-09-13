  using BepInEx;
  using BepInEx.Configuration;
  using BepInEx.Logging;
  using BaphsFika.Plugin.UI;
  using BaphsFika.Plugin.Networking;
  using BaphsFika.Plugin.Networking.Packets;
  using UnityEngine;
  using LiteNetLib;

  namespace BaphsFika.Plugin.Core
  {
      [BepInPlugin("com.baphsfika.plugin", "BaphsFika", "1.0.0")]
      public class BaphsFikaPlugin : BaseUnityPlugin
      {
          public static BaphsFikaPlugin Instance { get; private set; }
          public ConnectionManager ConnectionManager { get; private set; }
          public MultiplayerMenu MultiplayerMenu { get; private set; }
          public ServerBrowser ServerBrowser { get; private set; }

          private ConfigEntry<string> serverAddress;
          private ConfigEntry<int> serverPort;

          private void Awake()
          {
              Instance = this;
        
              InitializeConfiguration();
              InitializeNetworking();
              InitializeUI();
        
              Logger.LogInfo("BaphsFika plugin loaded!");
          }

          private void InitializeConfiguration()
          {
              serverAddress = Config.Bind("Network", "ServerAddress", "127.0.0.1", "The IP address of the server");
              serverPort = Config.Bind("Network", "ServerPort", 7777, "The port of the server");
          }

          private void InitializeNetworking()
          {
              ConnectionManager = gameObject.AddComponent<ConnectionManager>();
              ConnectionManager.OnConnectionStatusChanged += HandleConnectionStatusChanged;
              ConnectionManager.OnPlayerStateReceived += HandlePlayerStateReceived;
              ConnectionManager.OnWeaponFireReceived += HandleWeaponFireReceived;
              ConnectionManager.OnItemInteractionReceived += HandleItemInteractionReceived;
              ConnectionManager.OnGameStateReceived += HandleGameStateReceived;
              ConnectionManager.OnBotStateReceived += HandleBotStateReceived;
              ConnectionManager.OnServerInfoReceived += HandleServerInfoReceived;

              if (IsHost())
              {
                  ConnectionManager.StartServer(serverPort.Value);
              }
              else
              {
                  ConnectionManager.StartClient();
              }
          }

          private void InitializeUI()
          {
              MultiplayerMenu = gameObject.AddComponent<MultiplayerMenu>();
              ServerBrowser = gameObject.AddComponent<ServerBrowser>();
          }

          private bool IsHost()
          {
              // Implement logic to determine if this instance should act as a host
              return false; // Placeholder
          }

          private void HandleConnectionStatusChanged(string status)
          {
              MultiplayerMenu.UpdateConnectionStatus(status);
          }

          private void HandlePlayerStateReceived(PlayerStatePacket packet, NetPeer peer)
          {
              // Handle player state update
          }

          private void HandleWeaponFireReceived(WeaponFirePacket packet, NetPeer peer)
          {
              // Handle weapon fire event
          }

          private void HandleItemInteractionReceived(ItemInteractionPacket packet, NetPeer peer)
          {
              // Handle item interaction event
          }

          private void HandleGameStateReceived(GameStatePacket packet, NetPeer peer)
          {
              // Handle game state update
          }

          private void HandleBotStateReceived(BotStatePacket packet, NetPeer peer)
          {
              // Handle bot state update
          }

          private void HandleServerInfoReceived(ServerInfoPacket packet, NetPeer peer)
          {
              // Handle server info update
          }

          private void Update()
          {
              // ConnectionManager now handles its own updates
          }

          private void OnDestroy()
          {
              if (IsHost())
              {
                  ConnectionManager.Disconnect();
              }
          }

          public void SendPlayerAction(PlayerStatePacket packet)
          {
              ConnectionManager.EnqueuePacket(packet);
          }

          public void ConnectToServer(string address, int port)
          {
              ConnectionManager.Connect(address, port);
          }

          // Add more methods as needed for game-specific functionality
      }
  }