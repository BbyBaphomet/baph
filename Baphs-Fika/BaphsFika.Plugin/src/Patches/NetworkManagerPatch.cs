  using EFT;
  using BaphsFika.Plugin.Networking;
  using BaphsFika.Plugin.Core;
  using Comfort.Common;
  using System;
  using UnityEngine;
  using LiteNetLib;

  namespace BaphsFika.Plugin.Patches
  {
      public class NetworkManagerPatch
      {
          private static ConnectionManager _connectionManager;
          private static PacketHandler _packetHandler;
          private static GameStateManager _gameStateManager;
          private static NetworkStatistics _networkStats;

          public static void Initialize()
          {
              _connectionManager = new GameObject("ConnectionManager").AddComponent<ConnectionManager>();
              _gameStateManager = new GameStateManager();
              _packetHandler = new PacketHandler(_connectionManager, _gameStateManager);
              _networkStats = new NetworkStatistics();

              HookNetworkEvents();
              InitializeNetworking();
          }

          private static void HookNetworkEvents()
          {
              _connectionManager.OnPeerConnected += HandlePeerConnected;
              _connectionManager.OnPeerDisconnected += HandlePeerDisconnected;
              _connectionManager.OnNetworkReceive += HandleNetworkReceive;
              _connectionManager.OnConnectionStateChanged += HandleConnectionStateChanged;
          }

          private static void InitializeNetworking()
          {
              string serverIp = BaphsFikaPlugin.Instance.Config.Bind("Network", "ServerIP", "127.0.0.1").Value;
              int serverPort = BaphsFikaPlugin.Instance.Config.Bind("Network", "ServerPort", 5555).Value;

              if (IsServer())
              {
                  _connectionManager.StartAsServer(serverPort);
              }
              else
              {
                  _connectionManager.StartAsClient();
                  _connectionManager.Connect(serverIp, serverPort);
              }
          }

          private static bool IsServer()
          {
              // Implement logic to determine if this instance should act as a server
              return false; // Default to client for now
          }

          private static void HandlePeerConnected(NetPeer peer)
          {
              Debug.Log($"Peer connected: {peer.EndPoint}");
              _gameStateManager.AddPlayer(peer.Id);
              _networkStats.RecordConnection();
          }

          private static void HandlePeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
          {
              Debug.Log($"Peer disconnected: {peer.EndPoint}, Reason: {disconnectInfo.Reason}");
              _gameStateManager.RemovePlayer(peer.Id);
              _networkStats.RecordDisconnection();
          }

          private static void HandleNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
          {
              _packetHandler.HandlePacket(peer, reader, channelNumber, deliveryMethod);
              _networkStats.RecordPacketReceived(reader.UserDataSize);
          }

          private static void HandleConnectionStateChanged(ConnectionState newState)
          {
              Debug.Log($"Connection state changed to: {newState}");
              if (newState == ConnectionState.Disconnected)
              {
                  AttemptReconnection();
              }
          }

          private static void AttemptReconnection()
          {
              // Implement reconnection logic
          }

          public static void Update()
          {
              _connectionManager.Update();
              _gameStateManager.Update();
              _networkStats.Update();
          }

          public static void SendPacket<T>(T packet) where T : INetSerializable
          {
              _packetHandler.SendPacket(packet, _connectionManager.ServerPeer);
              _networkStats.RecordPacketSent(PacketSizeEstimator.EstimateSize(packet));
          }

          public static void BroadcastPacket<T>(T packet) where T : INetSerializable
          {
              _packetHandler.BroadcastPacket(packet);
              _networkStats.RecordPacketBroadcast(PacketSizeEstimator.EstimateSize(packet));
          }

          public static void SimulateNetworkConditions(int latency, float packetLoss)
          {
              _connectionManager.SimulateNetworkConditions(latency, packetLoss);
          }
      }
  }
