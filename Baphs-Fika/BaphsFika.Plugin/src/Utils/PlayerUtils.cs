
using UnityEngine;
using BaphsFika.Plugin.Core;
using BaphsFika.Plugin.GameState;

namespace BaphsFika.Plugin.Utils
{
    public static class PlayerUtils
    {
        public static PlayerState GetPlayerState(int playerId)
        {
            // Assuming we have a GameStateManager instance accessible
            return GameStateManager.Instance.GetPlayerState(playerId);
        }

        public static float CalculateHealthPercentage(PlayerState playerState)
        {
            return (playerState.CurrentHealth / playerState.MaxHealth) * 100f;
        }

        public static Vector3 GetPlayerPosition(int playerId)
        {
            PlayerState playerState = GetPlayerState(playerId);
            return playerState?.Position ?? Vector3.zero;
        }

        public static void UpdatePlayerPosition(int playerId, Vector3 newPosition)
        {
            PlayerState playerState = GetPlayerState(playerId);
            if (playerState != null)
            {
                playerState.Position = newPosition;
                GameStateManager.Instance.UpdateEntityState(playerId, playerState);
            }
        }

        public static bool IsPlayerInRange(int playerId1, int playerId2, float range)
        {
            Vector3 pos1 = GetPlayerPosition(playerId1);
            Vector3 pos2 = GetPlayerPosition(playerId2);
            return Vector3.Distance(pos1, pos2) <= range;
        }

        public static void DamagePlayer(int playerId, float damage)
        {
            PlayerState playerState = GetPlayerState(playerId);
            if (playerState != null)
            {
                playerState.CurrentHealth = Mathf.Max(0, playerState.CurrentHealth - damage);
                GameStateManager.Instance.UpdateEntityState(playerId, playerState);
            }
        }

        public static bool IsPlayerAlive(int playerId)
        {
            PlayerState playerState = GetPlayerState(playerId);
            return playerState != null && playerState.CurrentHealth > 0;
        }

        public static void SyncPlayerState(int playerId, PlayerState newState)
        {
            GameStateManager.Instance.UpdateEntityState(playerId, newState);
        }
    }
}
