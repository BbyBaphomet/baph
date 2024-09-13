
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using BaphsFika.Plugin.Networking;
using BaphsFika.Plugin.Models;

namespace BaphsFika.Plugin.UI
{
    public class ServerBrowser : MonoBehaviour
    {
        [SerializeField] private Transform serverListContent;
        [SerializeField] private GameObject serverEntryPrefab;
        [SerializeField] private Button refreshButton;
        [SerializeField] private InputField filterInput;

        private NetworkManager networkManager;
        private List<ServerInfo> serverList = new List<ServerInfo>();

        private void Start()
        {
            networkManager = FindObjectOfType<NetworkManager>();
            refreshButton.onClick.AddListener(RefreshServerList);
            filterInput.onValueChanged.AddListener(FilterServerList);

            RefreshServerList();
        }

        private void RefreshServerList()
        {
            // Clear existing server entries
            foreach (Transform child in serverListContent)
            {
                Destroy(child.gameObject);
            }

            // In a real implementation, this would query servers
            // For now, we'll add some dummy servers
            serverList.Clear();
            serverList.Add(new ServerInfo { Name = "Test Server 1", PlayerCount = 5, MaxPlayers = 10, Ping = 50 });
            serverList.Add(new ServerInfo { Name = "Test Server 2", PlayerCount = 2, MaxPlayers = 8, Ping = 75 });

            PopulateServerList();
        }

        private void PopulateServerList()
        {
            foreach (var server in serverList)
            {
                GameObject entry = Instantiate(serverEntryPrefab, serverListContent);
                ServerListEntry entryScript = entry.GetComponent<ServerListEntry>();
                entryScript.SetServerInfo(server);
                entryScript.OnJoinServer += JoinServer;
            }
        }

        private void FilterServerList(string filter)
        {
            foreach (Transform child in serverListContent)
            {
                ServerListEntry entry = child.GetComponent<ServerListEntry>();
                bool shouldShow = string.IsNullOrEmpty(filter) || 
                                  entry.ServerInfo.Name.ToLower().Contains(filter.ToLower());
                child.gameObject.SetActive(shouldShow);
            }
        }

        private void JoinServer(ServerInfo server)
        {
            Debug.Log($"Joining server: {server.Name}");
            // In a real implementation, this would connect to the server
            // networkManager.Connect(server.IpAddress, server.Port);
        }
    }

    public class ServerInfo
    {
        public string Name { get; set; }
        public int PlayerCount { get; set; }
        public int MaxPlayers { get; set; }
        public int Ping { get; set; }
        // Add more properties as needed (e.g., IP address, port, game mode)
    }

    public class ServerListEntry : MonoBehaviour
    {
        public ServerInfo ServerInfo { get; private set; }
        public event System.Action<ServerInfo> OnJoinServer;

        [SerializeField] private Text serverNameText;
        [SerializeField] private Text playerCountText;
        [SerializeField] private Text pingText;
        [SerializeField] private Button joinButton;

        private void Start()
        {
            joinButton.onClick.AddListener(() => OnJoinServer?.Invoke(ServerInfo));
        }

        public void SetServerInfo(ServerInfo info)
        {
            ServerInfo = info;
            serverNameText.text = info.Name;
            playerCountText.text = $"{info.PlayerCount}/{info.MaxPlayers}";
            pingText.text = $"{info.Ping}ms";
        }
    }
}
