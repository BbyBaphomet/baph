using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading.Tasks;
using BaphsFika.Plugin.Networking.Packets;
using BaphsFika.Plugin.Core;


namespace BaphsFika.Core.Networking
{
    public class Client
    {
        private NetManager _netManager;
        private NetPeer _serverPeer;
        private NetPacketProcessor _packetProcessor;
        private NatPunchModule _natPunchModule;
        private PlayerStatePacket _lastSentState;
        private Queue<object> _outgoingPacketQueue;
        private const int MaxQueueSize = 100;

        public event Action<GameStatePacket> OnGameStateReceived;
        public event Action<WeaponFirePacket> OnWeaponFireReceived;
        public event Action<ItemInteractionPacket> OnItemInteractionReceived;
        public event Action<BotStatePacket> OnBotStateReceived;
        public event Action<List<LobbyManager>> OnLobbyListReceived;
        public event Action<bool> OnLobbyJoinResult;
        public event Action<GameStatePacket> OnGameStartReceived;

        public Client()
        {
            _netManager = new NetManager(new EventBasedNetListener());
            _packetProcessor = new NetPacketProcessor();
            _natPunchModule = _netManager.NatPunchModule;
            _outgoingPacketQueue = new Queue<object>();
            RegisterPacketHandlers();
        }

        public async Task Connect(string ip, int port, string key)
        {
            _netManager.Start();
            _serverPeer = _netManager.Connect(ip, port, key);
            await _natPunchModule.StartNatPunchAsync(ip, port);
        }

        public void Disconnect()
        {
            _netClient.Stop();
        }

        public void Update()
        {
            _netClient.PollEvents();
            ProcessOutgoingPacketQueue();
        }

        public void SendPlayerState(PlayerState currentState)
        {
            var packet = new PlayerStatePacket { PlayerId = currentState.Id };
            
            if (Vector3.Distance(_lastSentState.Position, currentState.Position) > 0.01f)
                packet.Position = currentState.Position;
            
            if (Quaternion.Angle(_lastSentState.Rotation, currentState.Rotation) > 0.1f)
                packet.Rotation = currentState.Rotation;
            
            if (Vector3.Distance(_lastSentState.Velocity, currentState.Velocity) > 0.01f)
                packet.Velocity = currentState.Velocity;
            
            if (Mathf.Abs(_lastSentState.Health - currentState.Health) > 0.1f)
                packet.Health = currentState.Health;

            if (packet.Position.HasValue || packet.Rotation.HasValue || packet.Velocity.HasValue || packet.Health.HasValue)
            {
                EnqueuePacket(packet);
                _lastSentState = currentState;
            }
        }

        public void SendWeaponFire(WeaponFirePacket weaponFire)
        {
            EnqueuePacket(weaponFire);
        }

        public void SendItemInteraction(ItemInteractionPacket itemInteraction)
        {
            EnqueuePacket(itemInteraction);
        }

        public void SendBotState(BotStatePacket botState)
        {
            EnqueuePacket(botState);
        }

        public void RequestFullBotState(int botId)
        {
            EnqueuePacket(new FullBotStateRequestPacket { BotId = botId });
        }

        public void SendGameStartRequest(GameStartRequestPacket gameStartRequest)
        {
            EnqueuePacket(gameStartRequest);
        }

        private void EnqueuePacket<T>(T packet) where T : class, new()
        {
            if (_outgoingPacketQueue.Count < MaxQueueSize)
            {
                _outgoingPacketQueue.Enqueue(packet);
            }
            else
            {
                Debug.LogWarning("Outgoing packet queue is full. Packet dropped.");
            }
        }

        private void ProcessOutgoingPacketQueue()
        {
            while (_outgoingPacketQueue.Count > 0 && _serverPeer != null && _serverPeer.ConnectionState == ConnectionState.Connected)
            {
                var packet = _outgoingPacketQueue.Dequeue();
                SendPacket(packet);
            }
        }

        private void SendPacket<T>(T packet) where T : class, new()
        {
            byte[] serializedData = _packetProcessor.Write(packet);
            byte[] compressedData = Compress(serializedData);
            _serverPeer.Send(compressedData, DeliveryMethod.ReliableOrdered);
        }

        private void RegisterPacketHandlers()
        {
            _packetProcessor.SubscribeReusable<GameStatePacket>(OnGameStateUpdate);
            _packetProcessor.SubscribeReusable<WeaponFirePacket>(OnWeaponFireUpdate);
            _packetProcessor.SubscribeReusable<ItemInteractionPacket>(OnItemInteractionUpdate);
            _packetProcessor.SubscribeReusable<BotStatePacket>(OnBotStateUpdate);
            _packetProcessor.SubscribeReusable<LobbyListPacket>(OnLobbyListUpdate);
            _packetProcessor.SubscribeReusable<LobbyJoinResultPacket>(OnLobbyJoinResultUpdate);
            _packetProcessor.SubscribeReusable<GameStartPacket>(OnGameStartUpdate);
        }

        private void OnGameStateUpdate(GameStatePacket packet)
        {
            OnGameStateReceived?.Invoke(packet);
        }

        private void OnWeaponFireUpdate(WeaponFirePacket packet)
        {
            OnWeaponFireReceived?.Invoke(packet);
        }

        private void OnItemInteractionUpdate(ItemInteractionPacket packet)
        {
            OnItemInteractionReceived?.Invoke(packet);
        }

        private void OnBotStateUpdate(BotStatePacket packet)
        {
            OnBotStateReceived?.Invoke(packet);
        }

        private void OnLobbyListUpdate(LobbyListPacket packet)
        {
            OnLobbyListReceived?.Invoke(packet.Lobbies);
        }

        private void OnLobbyJoinResultUpdate(LobbyJoinResultPacket packet)
        {
            OnLobbyJoinResult?.Invoke(packet.Success);
        }

        private void OnGameStartUpdate(GameStartPacket packet)
        {
            OnGameStartReceived?.Invoke(packet);
        }

        public bool IsConnected()
        {
            return _serverPeer != null && _serverPeer.ConnectionState == ConnectionState.Connected;
        }

        private byte[] Compress(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return compressedStream.ToArray();
            }
        }
    }
}
