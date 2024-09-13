
using EFT;
using BaphsFika.Plugin.Networking;
using BaphsFika.Plugin.Networking.Packets;
using Comfort.Common;
using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using Newtonsoft.Json;

namespace BaphsFika.Plugin.Patches
{
    public class SaveLoadPatch
    {
        private static ClientNetworkManager _networkManager;
        private const int SAVE_VERSION = 1;

        public static void Initialize(ClientNetworkManager networkManager)
        {
            _networkManager = networkManager;
            _networkManager.RegisterPacketHandler<SaveGamePacket>(HandleSaveGamePacket);
            _networkManager.RegisterPacketHandler<LoadGamePacket>(HandleLoadGamePacket);
        }

        public static void SaveGame(string saveName)
        {
            GameState gameState = CaptureGameState();
            byte[] serializedState = SerializeGameState(gameState);
            byte[] compressedState = CompressData(serializedState);

            SaveGamePacket packet = new SaveGamePacket
            {
                SaveName = saveName,
                SaveData = compressedState,
                SaveVersion = SAVE_VERSION
            };

            _networkManager.SendPacket(packet);
        }

        public static void LoadGame(string saveName)
        {
            LoadGamePacket packet = new LoadGamePacket
            {
                SaveName = saveName
            };

            _networkManager.SendPacket(packet);
        }

        private static void HandleSaveGamePacket(SaveGamePacket packet)
        {
            byte[] decompressedData = DecompressData(packet.SaveData);
            GameState gameState = DeserializeGameState(decompressedData);
            ApplyGameState(gameState);
        }

        private static void HandleLoadGamePacket(LoadGamePacket packet)
        {
            // Implement server-side load logic here
            // This might involve fetching the save data and sending it back to clients
        }

        private static GameState CaptureGameState()
        {
            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            GameState state = new GameState
            {
                Players = gameWorld.AllPlayers,
                Bots = gameWorld.AllBots,
                LootItems = gameWorld.LootItems,
                // Add other relevant game state data
            };
            return state;
        }

        private static byte[] SerializeGameState(GameState state)
        {
            string json = JsonConvert.SerializeObject(state, Formatting.None,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        private static GameState DeserializeGameState(byte[] data)
        {
            string json = System.Text.Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<GameState>(json);
        }

        private static void ApplyGameState(GameState state)
        {
            GameWorld gameWorld = Singleton<GameWorld>.Instance;
            // Apply the deserialized state to the game world
            // This might involve respawning players, updating bot states, repositioning loot, etc.
        }

        private static byte[] CompressData(byte[] data)
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(output, CompressionLevel.Optimal))
                {
                    gzip.Write(data, 0, data.Length);
                }
                return output.ToArray();
            }
        }

        private static byte[] DecompressData(byte[] compressedData)
        {
            using (MemoryStream input = new MemoryStream(compressedData))
            using (GZipStream gzip = new GZipStream(input, CompressionMode.Decompress))
            using (MemoryStream output = new MemoryStream())
            {
                gzip.CopyTo(output);
                return output.ToArray();
            }
        }
    }
}
