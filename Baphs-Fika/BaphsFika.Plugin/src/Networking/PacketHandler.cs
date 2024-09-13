  using BaphsFika.Plugin.Core;
  using BaphsFika.Plugin.Networking.Packets;
  using UnityEngine;
  using LiteNetLib;

  namespace BaphsFika.Plugin.Networking
  {
      public class PacketHandler
      {
          private ConnectionManager _connectionManager;
          private GameStateManager _gameStateManager;

          public PacketHandler(ConnectionManager connectionManager, GameStateManager gameStateManager)
          {
              _connectionManager = connectionManager;
              _gameStateManager = gameStateManager;
          }

          public void SendPacket<T>(T packet, NetPeer peer) where T : BasePacket
          {
              _connectionManager.SendPacket(packet, peer);
          }

          public void BroadcastPacket<T>(T packet) where T : BasePacket
          {
              _connectionManager.BroadcastPacket(packet);
          }
      }
  }