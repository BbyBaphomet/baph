
using EFT;
using BaphsFika.Plugin.Networking;
using BaphsFika.Plugin.Networking.Packets;
using Comfort.Common;
using System.Collections.Generic;
using UnityEngine;

namespace BaphsFika.Plugin.Patches
{
    public class MapPatch
    {
        private static ClientNetworkManager _networkManager;
        private static Dictionary<string, ExfiltrationPoint> _exfiltrationPoints = new Dictionary<string, ExfiltrationPoint>();

        public static void Initialize(ClientNetworkManager networkManager)
        {
            _networkManager = networkManager;
            _networkManager.RegisterPacketHandler<MapStatePacket>(HandleMapStatePacket);
            HookMapEvents();
        }

        private static void HookMapEvents()
        {
            // Hook into relevant map events
            Singleton<GameWorld>.Instance.OnExfiltrationPointsChanged += HandleExfiltrationPointsChanged;
        }

        private static void HandleExfiltrationPointsChanged(List<ExfiltrationPoint> exfiltrationPoints)
        {
            foreach (var point in exfiltrationPoints)
            {
                _exfiltrationPoints[point.Id] = point;
                SendExfiltrationPointUpdate(point);
            }
        }

        private static void SendExfiltrationPointUpdate(ExfiltrationPoint point)
        {
            ExfiltrationPointPacket packet = new ExfiltrationPointPacket
            {
                PointId = point.Id,
                IsAvailable = point.IsAvailable,
                Position = point.transform.position,
                ExfiltrationTime = point.Settings.ExfiltrationTime
            };
            _networkManager.SendPacket(packet);
        }

        private static void HandleMapStatePacket(MapStatePacket packet)
        {
            UpdateExfiltrationPoints(packet.ExfiltrationPoints);
            UpdateDynamicMapEvents(packet.DynamicEvents);
        }

        private static void UpdateExfiltrationPoints(List<ExfiltrationPointPacket> points)
        {
            foreach (var pointPacket in points)
            {
                if (_exfiltrationPoints.TryGetValue(pointPacket.PointId, out ExfiltrationPoint point))
                {
                    point.IsAvailable = pointPacket.IsAvailable;
                    point.transform.position = pointPacket.Position;
                    point.Settings.ExfiltrationTime = pointPacket.ExfiltrationTime;
                }
                else
                {
                    // Create new exfiltration point if it doesn't exist
                    ExfiltrationPoint newPoint = Singleton<GameWorld>.Instance.CreateExfiltrationPoint(pointPacket.PointId, pointPacket.Position);
                    newPoint.IsAvailable = pointPacket.IsAvailable;
                    newPoint.Settings.ExfiltrationTime = pointPacket.ExfiltrationTime;
                    _exfiltrationPoints[pointPacket.PointId] = newPoint;
                }
            }
        }

        private static void UpdateDynamicMapEvents(List<DynamicEventPacket> events)
        {
            foreach (var eventPacket in events)
            {
                // Handle dynamic map events (e.g., airdrops, scav boss spawns)
                // Implementation depends on the specific dynamic events in your game
            }
        }
    }
}
