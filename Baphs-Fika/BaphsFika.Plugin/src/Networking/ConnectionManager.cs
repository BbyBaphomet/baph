  using System;
  using System.Collections.Generic;
  using UnityEngine;
  using BaphsFika.Plugin.Core;
  using LiteNetLib;
  using BaphsFika.Plugin.Networking.Packets;
  using System.Collections.Concurrent;
  using EFT.InventoryLogic;
using LiteNetLib.Utils;

namespace BaphsFika.Plugin.Networking
  {
      public class ConnectionManager : MonoBehaviour
      {
          public event Action<string> OnConnectionStatusChanged;

          private NetManager _netManager;
          private NetPeer _serverPeer;
          private NetPacketProcessor _packetProcessor;
          private Dictionary<int, NetPeer> _connectedClients;
          private NatPunchModule _natPunchModule;
          private float _updateInterval = 0.05f;
          private float _lastUpdateTime;
          private GameStateManager _gameStateManager;
          private ConcurrentQueue<BasePacket> _packetQueue = new ConcurrentQueue<BasePacket>();

          public event Action<PlayerStatePacket, NetPeer> OnPlayerStateReceived;
          public event Action<WeaponFirePacket, NetPeer> OnWeaponFireReceived;
          public event Action<ItemInteractionPacket, NetPeer> OnItemInteractionReceived;
          public event Action<GameStatePacket, NetPeer> OnGameStateReceived;
          public event Action<BotStatePacket, NetPeer> OnBotStateReceived;
          public event Action<ServerInfoPacket, NetPeer> OnServerInfoReceived;

          private void Awake()
          {
              _netManager = new NetManager(new EventBasedNetListener());
              _packetProcessor = new NetPacketProcessor();
              _connectedClients = new Dictionary<int, NetPeer>();
              _natPunchModule = _netManager.NatPunchModule;
              _gameStateManager = GetComponent<GameStateManager>();

              RegisterPacketHandlers();
              SetupEventHandlers();
              InitializeCompression();
              SetupPingSystem();
          }

          private void RegisterPacketHandlers()
          {
              _packetProcessor.RegisterNestedType<PlayerStatePacket>();
              _packetProcessor.RegisterNestedType<BotState>();
              _packetProcessor.RegisterNestedType<ItemInteractionPacket>();
              _packetProcessor.RegisterNestedType<WeaponFirePacket>();
              _packetProcessor.RegisterNestedType<GameStatePacket>();
              _packetProcessor.SubscribeReusable<PlayerStatePacket, NetPeer>(HandlePlayerStatePacket);
              _packetProcessor.SubscribeReusable<WeaponFirePacket, NetPeer>(HandleWeaponFirePacket);
              _packetProcessor.SubscribeReusable<ItemInteractionPacket, NetPeer>(HandleItemInteractionPacket);
              _packetProcessor.SubscribeReusable<BotStatePacket, NetPeer>(HandleBotStatePacket);
              _packetProcessor.SubscribeReusable<GameStatePacket, NetPeer>(HandleGameStatePacket);
              _packetProcessor.SubscribeReusable<ServerInfoPacket, NetPeer>(HandleServerInfoPacket);
          }

          private void SetupEventHandlers()
          {
              _netManager.PeerConnectedEvent += OnPeerConnected;
              _netManager.PeerDisconnectedEvent += OnPeerDisconnected;
              _netManager.NetworkReceiveEvent += OnNetworkReceive;
              _natPunchModule.OnNatIntroductionSuccess += OnNatPunchSucceeded;
          }

          public void StartServer(int port)
          {
              _netManager.Start(port);
              OnConnectionStatusChanged?.Invoke($"Server started on port {port}");
          }

          public void StartClient()
          {
              _netManager.Start();
          }

          public void Connect(string ip, int port)
          {
              _serverPeer = _netManager.Connect(ip, port, "BaphsFikaConnection");
              OnConnectionStatusChanged?.Invoke($"Connecting to server at {ip}:{port}");
          }

          public void Disconnect()
          {
              _netManager.DisconnectAll();
              OnConnectionStatusChanged?.Invoke("Disconnected from server");
          }

          private void Update()
          {
              _netManager.PollEvents();

              if (Time.time - _lastUpdateTime >= _updateInterval)
              {
                  ProcessPacketQueue();
                  _lastUpdateTime = Time.time;
              }
          }

          private void OnDestroy()
          {
              _netManager.Stop();
          }

          public void SendPacket<T>(T packet, NetPeer peer) where T : BasePacket
          {
              byte[] compressedData = CompressPacket(packet);
              peer.Send(compressedData, DeliveryMethod.ReliableOrdered);
          }

          public void BroadcastPacket<T>(T packet) where T : BasePacket
          {
              byte[] compressedData = CompressPacket(packet);
              _netManager.SendToAll(compressedData, DeliveryMethod.ReliableOrdered);
          }

          private byte[] CompressPacket<T>(T packet) where T : BasePacket
          {
              // Implement compression logic
              return null; // Placeholder
          }

          public void EnqueuePacket(BasePacket packet)
          {
              _packetQueue.Enqueue(packet);
          }

          private void ProcessPacketQueue()
          {
              while (_packetQueue.TryDequeue(out BasePacket packet))
              {
                  // Process the packet
                  // This is where you'd handle different packet types
              }
          }

          private void HandlePlayerStatePacket(PlayerStatePacket packet, NetPeer peer)
          {
              _gameStateManager.UpdatePlayerState(packet);
              OnPlayerStateReceived?.Invoke(packet, peer);
          }

          private void HandleWeaponFirePacket(WeaponFirePacket packet, NetPeer peer)
          {
              _gameStateManager.ProcessWeaponFire(packet);
              OnWeaponFireReceived?.Invoke(packet, peer);
          }

          private void HandleItemInteractionPacket(ItemInteractionPacket packet, NetPeer peer)
          {
              _gameStateManager.ProcessItemInteraction(packet);
              OnItemInteractionReceived?.Invoke(packet, peer);
          }

          private void HandleGameStatePacket(GameStatePacket packet, NetPeer peer)
          {
              _gameStateManager.UpdateGameState(packet);
              OnGameStateReceived?.Invoke(packet, peer);
          }

          private void HandleBotStatePacket(BotStatePacket packet, NetPeer peer)
          {
              _gameStateManager.UpdateBotState(packet);
              OnBotStateReceived?.Invoke(packet, peer);
          }

          private void HandleServerInfoPacket(ServerInfoPacket packet, NetPeer peer)
          {
              Debug.Log($"Received server info: {packet.ServerName}, Players: {packet.CurrentPlayers}/{packet.MaxPlayers}");
              OnServerInfoReceived?.Invoke(packet, peer);
          }

          private void OnPeerConnected(NetPeer peer)
          {
              // Handle peer connection
          }

          private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
          {
              // Handle peer disconnection
          }

          private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
          {
              _packetProcessor.ReadAllPackets(reader, peer);
          }

          private void OnNatPunchSucceeded(NatAddressType type, string token, IPEndPoint remoteEndPoint)
          {
              // Handle successful NAT punch
          }

          private void InitializeCompression()
          {
              // Initialize compression system
          }

          private void SetupPingSystem()
          {
              // Setup ping system
          }
      }
  }