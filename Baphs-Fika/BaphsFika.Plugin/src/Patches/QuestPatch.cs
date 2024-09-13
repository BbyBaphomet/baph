
using EFT;
using BaphsFika.Plugin.Networking;
using BaphsFika.Plugin.Networking.Packets;
using Comfort.Common;
using System.Collections.Generic;
using UnityEngine;

namespace BaphsFika.Plugin.Patches
{
    public class QuestPatch
    {
        private static ClientNetworkManager _networkManager;
        private static Dictionary<string, Quest> _activeQuests = new Dictionary<string, Quest>();

        public static void Initialize(ClientNetworkManager networkManager)
        {
            _networkManager = networkManager;
            _networkManager.RegisterPacketHandler<QuestStatePacket>(HandleQuestStatePacket);
            HookQuestEvents();
        }

        private static void HookQuestEvents()
        {
            // Hook into relevant quest events
            Singleton<GameWorld>.Instance.QuestManager.OnQuestStarted += HandleQuestStarted;
            Singleton<GameWorld>.Instance.QuestManager.OnQuestCompleted += HandleQuestCompleted;
            Singleton<GameWorld>.Instance.QuestManager.OnQuestObjectiveUpdated += HandleQuestObjectiveUpdated;
        }

        private static void HandleQuestStarted(Quest quest)
        {
            _activeQuests[quest.Id] = quest;
            SendQuestUpdate(quest, QuestUpdateType.Started);
        }

        private static void HandleQuestCompleted(Quest quest)
        {
            _activeQuests.Remove(quest.Id);
            SendQuestUpdate(quest, QuestUpdateType.Completed);
        }

        private static void HandleQuestObjectiveUpdated(Quest quest, QuestObjective objective)
        {
            SendQuestUpdate(quest, QuestUpdateType.ObjectiveUpdated, objective);
        }

        private static void SendQuestUpdate(Quest quest, QuestUpdateType updateType, QuestObjective objective = null)
        {
            QuestStatePacket packet = new QuestStatePacket
            {
                QuestId = quest.Id,
                UpdateType = updateType,
                QuestStatus = quest.Status,
                ObjectiveId = objective?.Id,
                ObjectiveProgress = objective?.Progress
            };
            _networkManager.SendPacket(packet);
        }

        private static void HandleQuestStatePacket(QuestStatePacket packet)
        {
            Quest quest = Singleton<GameWorld>.Instance.QuestManager.GetQuest(packet.QuestId);
            if (quest == null)
            {
                Debug.LogWarning($"Quest not found: {packet.QuestId}");
                return;
            }

            switch (packet.UpdateType)
            {
                case QuestUpdateType.Started:
                    StartQuest(quest);
                    break;
                case QuestUpdateType.Completed:
                    CompleteQuest(quest);
                    break;
                case QuestUpdateType.ObjectiveUpdated:
                    UpdateQuestObjective(quest, packet);
                    break;
            }
        }

        private static void StartQuest(Quest quest)
        {
            if (!_activeQuests.ContainsKey(quest.Id))
            {
                quest.Start();
                _activeQuests[quest.Id] = quest;
            }
        }

        private static void CompleteQuest(Quest quest)
        {
            if (_activeQuests.ContainsKey(quest.Id))
            {
                quest.Complete();
                _activeQuests.Remove(quest.Id);
            }
        }

        private static void UpdateQuestObjective(Quest quest, QuestStatePacket packet)
        {
            QuestObjective objective = quest.GetObjective(packet.ObjectiveId);
            if (objective != null)
            {
                objective.SetProgress(packet.ObjectiveProgress);
            }
        }
    }
}
