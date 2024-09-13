using UnityEngine;
using BaphsFika.Plugin.Networking.Packets;
using System.Collections.Generic;
using EFT;
using EFT.InventoryLogic;
using EFT.Interactive;

namespace BaphsFika.Plugin.Core
{
    public class NetworkIntegrationManager : MonoBehaviour
    {
        private ClientNetworkManager _networkManager;
        private Dictionary<int, Player> _networkPlayers = new Dictionary<int, Player>();
        private Dictionary<int, BotOwner> _networkBots = new Dictionary<int, BotOwner>();
        private Dictionary<string, WorldInteractiveObject> _interactiveObjects = new Dictionary<string, WorldInteractiveObject>();
        private GameWorld _gameWorld;
        private ClientStatePredictor _statePredictor;
        private StateInterpolator _stateInterpolator;
        private QuestSynchronizer _questSynchronizer;
        private EnvironmentSynchronizer _environmentSynchronizer;
        private BotSynchronizer _botSynchronizer;
        private BotSyncConfig _botSyncConfig;

        public void Initialize(ClientNetworkManager networkManager, GameStateManager gameStateManager, BotSyncConfig botSyncConfig)
        {
            _networkManager = networkManager;
            _botSyncConfig = botSyncConfig;
            _gameWorld = Singleton<GameWorld>.Instance;

            _statePredictor = new ClientStatePredictor();
            _stateInterpolator = new StateInterpolator();
            _questSynchronizer = new QuestSynchronizer(gameStateManager);
            _environmentSynchronizer = new EnvironmentSynchronizer(gameStateManager);
            _botSynchronizer = new BotSynchronizer(gameStateManager, networkManager, botSyncConfig);

            RegisterNetworkHandlers();
            HookGameEvents();
        }

        private void RegisterNetworkHandlers()
        {
            _networkManager.OnPlayerStateReceived += HandlePlayerStateReceived;
            _networkManager.OnBotStateReceived += HandleBotStateReceived;
            _networkManager.OnWeaponFireReceived += HandleWeaponFireReceived;
            _networkManager.OnItemInteractionReceived += HandleItemInteractionReceived;
            _networkManager.OnGameStateReceived += HandleGameStateReceived;
            _networkManager.OnEnvironmentStateReceived += HandleEnvironmentStateReceived;
            _networkManager.OnEquipmentUpdateReceived += HandleEquipmentUpdateReceived;
            _networkManager.OnGameStartReceived += HandleGameStart;
        }

        private void HookGameEvents()
        {
            if (_gameWorld.MainPlayer != null)
            {
                _gameWorld.MainPlayer.OnPlayerMoveEvent += HandlePlayerMove;
                _gameWorld.MainPlayer.OnInventoryChangedEvent += HandleInventoryChanged;
            }

            _gameWorld.ShotFactory.OnShotEvent += HandleShot;
            _gameWorld.ItemFactory.OnItemAddedOrRemovedEvent += HandleItemInteraction;
            _gameWorld.InteractiveObjectsController.OnObjectInteractionEvent += HandleObjectInteraction;
        }

        private void HandlePlayerMove(Vector3 position, Vector3 rotation)
        {
            PlayerState playerState = new PlayerState
            {
                PlayerId = _gameWorld.MainPlayer.Id,
                Position = position,
                Rotation = Quaternion.Euler(rotation),
                Velocity = _gameWorld.MainPlayer.MovementContext.CharacterController.velocity,
                Health = _gameWorld.MainPlayer.HealthController.GetBodyPartHealth(EBodyPart.Common).Current,
                Stance = _gameWorld.MainPlayer.MovementContext.CurrentState.Name
            };

            _networkManager.SendPlayerState(playerState);
        }

        private void HandleInventoryChanged(Player player)
        {
            EquipmentPacket equipmentPacket = CreateEquipmentPacket(player);
            _networkManager.SendEquipmentUpdate(equipmentPacket);
        }

        private EquipmentPacket CreateEquipmentPacket(Player player)
        {
            EquipmentPacket packet = new EquipmentPacket
            {
                PlayerId = player.Id
            };

            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                Item item = player.Equipment.GetSlot((EFT.InventoryLogic.EquipmentSlot)slot).ContainedItem;
                if (item != null)
                {
                    packet.Equipment[slot] = item.TemplateId;
                }
            }

            foreach (Weapon weapon in player.Inventory.GetAllEquipmentItems().OfType<Weapon>())
            {
                WeaponInfo weaponInfo = new WeaponInfo
                {
                    WeaponId = weapon.Id,
                    TemplateId = weapon.TemplateId
                };

                foreach (Mod mod in weapon.GetAllMods())
                {
                    weaponInfo.Mods.Add(new ModInfo { ModId = mod.Id, TemplateId = mod.TemplateId });
                }

                packet.Weapons.Add(weaponInfo);
            }

            return packet;
        }

        private void HandleEquipmentUpdateReceived(EquipmentPacket packet)
        {
            if (_networkPlayers.TryGetValue(packet.PlayerId, out Player player))
            {
                UpdatePlayerEquipment(player, packet);
            }
            else if (_networkBots.TryGetValue(packet.PlayerId, out BotOwner bot))
            {
                UpdateBotEquipment(bot, packet);
            }
        }

        private void UpdatePlayerEquipment(Player player, EquipmentPacket packet)
        {
            foreach (var kvp in packet.Equipment)
            {
                Item item = _gameWorld.ItemFactory.CreateItem(kvp.Value);
                player.Equipment.GetSlot((EFT.InventoryLogic.EquipmentSlot)kvp.Key).AddItem(item);
            }

            foreach (WeaponInfo weaponInfo in packet.Weapons)
            {
                Weapon weapon = _gameWorld.ItemFactory.CreateWeapon(weaponInfo.TemplateId) as Weapon;
                foreach (ModInfo modInfo in weaponInfo.Mods)
                {
                    Mod mod = _gameWorld.ItemFactory.CreateMod(modInfo.TemplateId) as Mod;
                    weapon.AddMod(mod);
                }
                player.Inventory.AddItem(weapon);
            }
        }

        private void UpdateBotEquipment(BotOwner bot, EquipmentPacket packet)
        {
            // Implement bot-specific equipment update logic here
            // This will be similar to UpdatePlayerEquipment, but tailored for bots
        }

        private void HandleGameStart(GameStartPacket packet)
        {
            InitializeGameWorld(packet);
            TransitionToGameplay();
        }

        private void InitializeGameWorld(GameStartPacket packet)
        {
            SynchronizeGameState(packet.InitialGameState);
            SpawnPlayers(packet.Players);
            InitializeGameSystems(packet);
        }

        private void SynchronizeGameState(InitialGameState initialState)
        {
            _environmentSynchronizer.UpdateEnvironmentState(initialState.EnvironmentState);
            // Implement additional synchronization logic as needed
        }

        private void SpawnPlayers(List<PlayerInitialState> playerStates)
        {
            foreach (var playerState in playerStates)
            {
                if (_networkPlayers.TryGetValue(playerState.PlayerId, out Player player))
                {
                    player.Transform.position = playerState.Position;
                    player.Transform.rotation = playerState.Rotation;
                }
                else
                {
                    Player newPlayer = CreateNetworkPlayer(playerState.PlayerId);
                    newPlayer.Transform.position = playerState.Position;
                    newPlayer.Transform.rotation = playerState.Rotation;
                    _networkPlayers[playerState.PlayerId] = newPlayer;
                }
            }
        }

        private void InitializeGameSystems(GameStartPacket packet)
        {
            // Initialize other game systems based on the received packet
            // This might include setting up AI, loot spawns, etc.
        }

        private void TransitionToGameplay()
        {
            // Implement logic to transition from lobby to active gameplay
            // This might include enabling player controls, showing gameplay UI, etc.
        }

        private void HandleShot(ShotInfo shotInfo)
        {
            WeaponFirePacket weaponFire = new WeaponFirePacket
            {
                PlayerId = shotInfo.Player.Id,
                WeaponId = shotInfo.Weapon.Id,
                Position = shotInfo.Position,
                Direction = shotInfo.Direction,
                AmmoType = shotInfo.Ammo.TemplateId
            };

            _networkManager.SendWeaponFire(weaponFire);
        }

        private void HandleItemInteraction(Item item, ItemAddress location, bool added)
        {
            ItemInteractionPacket itemInteraction = new ItemInteractionPacket
            {
                PlayerId = _gameWorld.MainPlayer.Id,
                ItemId = item.Id,
                LocationId = location.ToString(),
                IsAdded = added,
                ItemTemplate = item.TemplateId
            };

            _networkManager.SendItemInteraction(itemInteraction);
        }

        private void HandleObjectInteraction(WorldInteractiveObject interactiveObject, Player player)
        {
            ObjectInteractionPacket objectInteraction = new ObjectInteractionPacket
            {
                PlayerId = player.Id,
                ObjectId = interactiveObject.Id,
                InteractionType = interactiveObject.InteractionType
            };

            _networkManager.SendObjectInteraction(objectInteraction);
        }

        private void HandlePlayerStateReceived(PlayerStatePacket packet)
        {
            _statePredictor.UpdatePlayerState(packet.PlayerId, packet.State);
            _stateInterpolator.UpdateEntityState(packet.PlayerId, packet.State);

            if (!_networkPlayers.TryGetValue(packet.PlayerId, out Player player))
            {
                player = CreateNetworkPlayer(packet.PlayerId);
                _networkPlayers[packet.PlayerId] = player;
            }

            UpdatePlayerState(player, packet.State);
        }

        private void HandleBotStateReceived(BotStatePacket packet)
        {
            _botSynchronizer.HandleNetworkBotState(packet);
        }

        private void HandleEnvironmentStateReceived(EnvironmentStatePacket packet)
        {
            _environmentSynchronizer.UpdateEnvironmentState(packet.EnvironmentState);
            UpdateInteractiveObjects(packet.EnvironmentState.InteractiveObjects);
        }

        private void UpdatePlayerState(Player player, PlayerState state)
        {
            Vector3 interpolatedPosition = _stateInterpolator.GetInterpolatedPosition(state.PlayerId);
            Quaternion interpolatedRotation = _stateInterpolator.GetInterpolatedRotation(state.PlayerId);

            player.Transform.position = interpolatedPosition;
            player.Transform.rotation = interpolatedRotation;
            player.MovementContext.CharacterController.velocity = state.Velocity;
            player.HealthController.SetBodyPartHealth(EBodyPart.Common, state.Health);
            player.MovementContext.SetState(state.Stance);
        }

        private void UpdateInteractiveObjects(Dictionary<string, InteractiveObjectState> objectStates)
        {
            foreach (var objectState in objectStates)
            {
                if (_interactiveObjects.TryGetValue(objectState.Key, out WorldInteractiveObject interactiveObject))
                {
                    interactiveObject.SetState(objectState.Value.State);
                }
                else
                {
                    WorldInteractiveObject newObject = _gameWorld.CreateInteractiveObject(objectState.Value.TemplateId);
                    newObject.SetState(objectState.Value.State);
                    _interactiveObjects[objectState.Key] = newObject;
                }
            }
        }

        private void Update()
        {
            UpdatePlayers();
            _botSynchronizer.Update();
            _environmentSynchronizer.Update(Time.deltaTime);
        }

        private void UpdatePlayers()
        {
            foreach (var player in _networkPlayers.Values)
            {
                Vector3 predictedPosition = _statePredictor.PredictPosition(player.Id, player.MovementContext.InputDirection, Time.deltaTime);
                Quaternion predictedRotation = _statePredictor.PredictRotation(player.Id, player.MovementContext.RotationDelta.y, player.MovementContext.RotationDelta.x);

                player.Transform.position = Vector3.Lerp(player.Transform.position, predictedPosition, Time.deltaTime * 10f);
                player.Transform.rotation = Quaternion.Slerp(player.Transform.rotation, predictedRotation, Time.deltaTime * 10f);
            }
        }
    }
}