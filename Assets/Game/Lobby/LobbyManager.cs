using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private TMP_InputField portInputField;

    private UnityTransport transport;
    private int selectedClassIndex = 0; // 0=Warrior, 1=Mage, 2=Rogue

    private void Awake()
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    }

    public void OnClassSelected(int classIndex)
    {
        selectedClassIndex = classIndex;
    }

    public void OnHostClicked()
    {
        transport.SetConnectionData("0.0.0.0", GetPort());

        // Host is also a client, so it needs to send its own payload too
        NetworkManager.Singleton.NetworkConfig.ConnectionData =
            new byte[] { (byte)selectedClassIndex };

        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost();
    }

    public void OnJoinClicked()
    {
        string ip = string.IsNullOrEmpty(ipInputField.text) ? "127.0.0.1" : ipInputField.text;
        transport.SetConnectionData(ip, GetPort());

        NetworkManager.Singleton.NetworkConfig.ConnectionData =
            new byte[] { (byte)selectedClassIndex };

        NetworkManager.Singleton.StartClient();
    }

    private ushort GetPort()
    {
        return ushort.TryParse(portInputField.text, out ushort port) ? port : (ushort)7777;
    }

    // Only runs on the server/host
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        byte classIndex = request.Payload.Length > 0 ? request.Payload[0] : (byte)0;
        PlayerClassRegistry.Instance.SetClassChoice(request.ClientNetworkId, classIndex);

        response.Approved = true;
        response.CreatePlayerObject = false; // we'll spawn manually, not the default prefab
    }
}