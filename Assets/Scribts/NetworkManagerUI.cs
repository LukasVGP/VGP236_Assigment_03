using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button serverButton;
    [SerializeField] private TextMeshProUGUI ipText;
    [SerializeField] private UNetTransport transport;
    [SerializeField] private TMP_InputField ipInputField;

    private void Start()
    {
        // Get the local IP address and display it
        string localIP = IPAddressFinder.GetLocalIPv4Address();
        ipText.text = "Local IP: " + localIP;
        ipInputField.text = localIP;
    }

    private void Awake()
    {
        if (NetworkManager.Singleton != null)
        {
            hostButton.onClick.AddListener(() =>
            {
                transport.ConnectAddress = ipInputField.text;
                NetworkManager.Singleton.StartHost();
                this.gameObject.SetActive(false);
            });

            clientButton.onClick.AddListener(() =>
            {
                transport.ConnectAddress = ipInputField.text;
                NetworkManager.Singleton.StartClient();
                this.gameObject.SetActive(false);
            });

            serverButton.onClick.AddListener(() =>
            {
                transport.ConnectAddress = ipInputField.text;
                NetworkManager.Singleton.StartServer();
                this.gameObject.SetActive(false);
            });
        }
    }
}
