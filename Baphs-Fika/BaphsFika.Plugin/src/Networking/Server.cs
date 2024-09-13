  using System;
  using System.Collections.Generic;
  using UnityEngine;
  using BaphsFika.Plugin.Core;
  using BaphsFika.Plugin.Utils;
  using LiteNetLib;
  using LiteNetLib.Utils;

  namespace BaphsFika.Plugin.Networking
  {
      public class Server : MonoBehaviour
      {
          private NetManager _netServer;
          private NetPacketProcessor _packetProcessor;
          private Dictionary<int, NetPeer> _connectedClients;
          private GameStateManager _gameStateManager;
          private float _updateInterval = 0.05f;
          private float _lastUpdateTime;

          public event Action<PlayerStatePacket, NetPeer> OnPlayerStateReceived;
          public event Action<WeaponFirePacket, NetPeer> OnWeaponFireReceived;
          public event Action<ItemInteractionPacket, NetPeer> OnItemInteractionReceived;

          private void Awake()
          {
              _netServer = new NetManager(new EventBasedNetListener());
              _packetProcessor = new NetPacketProcessor();
              _gameStateManager = new GameStateManager();
              _connectedClients = new Dictionary<int, NetPeer>();

              RegisterPacketHandlers();
              SetupEventHandlers();
          }

          public void StartServer(int port)
          {
              _netServer.Start(port);
              Logger.LogInfo($"Server started on port {port}");
          }

          public void StopServer()
          {
              _netServer.Stop();
              Logger.LogInfo("Server stopped");
          }

          private void RegisterPacketHandlers()
          {
              _packetProcessor.RegisterNestedType<PlayerState>();
              _packetProcessor.RegisterNestedType<BotState>();
              _packetProcessor.RegisterNestedType<ItemState>();
              _packetProcessor.RegisterNestedType<WeaponFireEvent>();
              _packetProcessor.RegisterNestedType<GameState>();
              _packetProcessor.SubscribeReusable<PlayerStatePacket, NetPeer>(OnPlayerStateUpdate);
              _packetProcessor.SubscribeReusable<WeaponFirePacket, NetPeer>(OnWeaponFireUpdate);
              _packetProcessor.SubscribeReusable<ItemInteractionPacket, NetPeer>(OnItemInteractionUpdate);
              _packetProcessor.SubscribeReusable<BotStatePacket, NetPeer>(OnBotStateUpdate);
          }

          private void SetupEventHandlers()
          {
              _netServer.PeerConnectedEvent += OnPeerConnected;
              _netServer.PeerDisconnectedEvent += OnPeerDisconnected;
              _netServer.NetworkReceiveEvent += OnNetworkReceive;
          }

          private void OnPeerConnected(NetPeer peer)
          {
              _connectedClients[peer.Id] = peer;
              Logger.LogInfo($"Client connected: ID {peer.Id}");
              SendFullGameState(peer);
          }

          private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
          {
              _connectedClients.Remove(peer.Id);
              _gameStateManager.RemovePlayer(peer.Id);
              Logger.LogInfo($"Client disconnected: ID {peer.Id}, Reason: {disconnectInfo.Reason}");
              BroadcastPlayerDisconnect(peer.Id);
          }

          private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
          {
              _packetProcessor.ReadAllPackets(reader, peer);
          }

          private void OnPlayerStateUpdate(PlayerStatePacket packet, NetPeer peer)
          {
              PlayerState currentState = _gameStateManager.GetPlayerState(packet.PlayerId);
              if (currentState == null)
              {
                  currentState = new PlayerState { Id = packet.PlayerId };
              }

              if (packet.Position.HasValue) currentState.Position = packet.Position.Value;
              if (packet.Rotation.HasValue) currentState.Rotation = packet.Rotation.Value;
              if (packet.Velocity.HasValue) currentState.Velocity = packet.Velocity.Value;
              if (packet.Health.HasValue) currentState.Health = packet.Health.Value;

              _gameStateManager.UpdateEntityState(packet.PlayerId, currentState);
              OnPlayerStateReceived?.Invoke(packet, peer);
              BroadcastPacket(packet, peer);
          }

          private void OnWeaponFireUpdate(WeaponFirePacket packet, NetPeer peer)
          {
              _gameStateManager.ProcessWeaponFire(packet);
              OnWeaponFireReceived?.Invoke(packet, peer);
              BroadcastPacket(packet, peer);
          }

          private void OnItemInteractionUpdate(ItemInteractionPacket packet, NetPeer peer)
          {
              _gameStateManager.ProcessItemInteraction(packet);
              OnItemInteractionReceived?.Invoke(packet, peer);
              BroadcastPacket(packet, peer);
          }

          private void OnBotStateUpdate(BotStatePacket packet, NetPeer peer)
          {
              _gameStateManager.UpdateEntityState(packet.BotId, packet.State);
              BroadcastPacket(packet, peer);
          }

          private void Update()
          {
              _netServer.PollEvents();

              if (Time.time - _lastUpdateTime >= _updateInterval)
              {
                  BroadcastGameState();
                  _lastUpdateTime = Time.time;
              }
          }

          private void BroadcastGameState()
          {
              GameState currentState = _gameStateManager.GetCurrentGameState();
              var packet = new GameStatePacket { GameState = currentState };
              BroadcastPacket(packet);
          }

          private void BroadcastPacket<T>(T packet, NetPeer excludePeer = null) where T : class, new()
          {
              var data = _packetProcessor.Write(packet);
              foreach (var client in _connectedClients.Values)
              {
                  if (client != excludePeer)
                  {
                      client.Send(data, DeliveryMethod.Unreliable);
                  }
              }
          }

          private void SendFullGameState(NetPeer peer)
          {
              GameState fullState = _gameStateManager.GetCurrentGameState();
              var packet = new GameStatePacket { GameState = fullState };
              SendPacketToClient(packet, peer.Id);
          }

          private void BroadcastPlayerDisconnect(int playerId)
          {
              var packet = new PlayerDisconnectPacket { PlayerId = playerId };
              BroadcastPacket(packet);
          }

          public void SendPacketToClient<T>(T packet, int clientId) where T : class, new()
          {
              if (_connectedClients.TryGetValue(clientId, out NetPeer peer))
              {
                  var data = _packetProcessor.Write(packet);
                  peer.Send(data, DeliveryMethod.ReliableOrdered);
              }
          }

          public void SetUpdateInterval(float interval)
          {
              _updateInterval = Mathf.Max(0.01f, interval);
          }
      }
  }
