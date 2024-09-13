
using UnityEngine;
using UnityEngine.UI;
using BaphsFika.Plugin.Networking;
using BaphsFika.Plugin.Core;

namespace BaphsFika.Plugin.UI
{
    public class MultiplayerMenu : MonoBehaviour
    {
        [SerializeField] private InputField serverAddressInput;
        [SerializeField] private InputField serverPortInput;
        [SerializeField] private Button connectButton;
        [SerializeField] private Button hostButton;
        [SerializeField] private Text statusText;

        private NetworkManager networkManager;

        private void Start()
        {
            networkManager = FindObjectOfType<NetworkManager>();
            
            connectButton.onClick.AddListener(ConnectToServer);
            hostButton.onClick.AddListener(HostServer);

            LoadServerSettings();
        }

        private void LoadServerSettings()
        {
            serverAddressInput.text = Config.ServerAddress.Value;
            serverPortInput.text = Config.ServerPort.Value.ToString();
        }

        private void ConnectToServer()
        {
            string address = serverAddressInput.text;
            int port = int.Parse(serverPortInput.text);

            statusText.text = "Connecting...";
            networkManager.Connect(address, port);
        }

        private void HostServer()
        {
            int port = int.Parse(serverPortInput.text);

            statusText.text = "Starting server...";
            networkManager.StartServer(port);
        }

        public void UpdateConnectionStatus(string status)
        {
            statusText.text = status;
        }

        private void OnDestroy()
        {
            SaveServerSettings();
        }

        private void SaveServerSettings()
        {
            Config.ServerAddress.Value = serverAddressInput.text;
            Config.ServerPort.Value = int.Parse(serverPortInput.text);
        }
    }
}
