using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private TMP_InputField portInputField;
    [SerializeField] private GameObject startGameButton; // assign in Inspector

    private UnityTransport transport;
    private int selectedClassIndex = 0; // 0=Warrior, 1=Mage, 2=Rogue

    private void Start()
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    }

    public void OnClassSelected(int classIndex)
    {
        selectedClassIndex = classIndex;
        Debug.Log($"Class selected: {classIndex}"); 
    }
    
    public void OnHostClicked()
    {
        Debug.Log("Host clicked");
        
        transport.SetConnectionData("0.0.0.0", GetPort());
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
        NetworkManager.Singleton.NetworkConfig.ConnectionData =
            new byte[] { (byte)selectedClassIndex };

        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost();

        startGameButton.SetActive(true); // only the host sees this
    }
    
    public void OnJoinClicked()
    {
        Debug.Log("Host clicked");
        
        transport.SetConnectionData(
            string.IsNullOrEmpty(ipInputField.text) ? "127.0.0.1" : ipInputField.text,
            GetPort());
        NetworkManager.Singleton.NetworkConfig.ConnectionData =
            new byte[] { (byte)selectedClassIndex };

        NetworkManager.Singleton.StartClient();
        // startGameButton stays inactive for clients
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
        Debug.Log($"ApprovalCheck fired for client {request.ClientNetworkId}, class {classIndex}");
        PlayerClassRegistry.Instance.SetClassChoice(request.ClientNetworkId, classIndex);

        response.Approved = true;
        response.CreatePlayerObject = false; // we'll spawn manually, not the default prefab
    }

    public void OnStartGameClicked()
    {
        if (!NetworkManager.Singleton.IsServer) return; // safety guard

        NetworkManager.Singleton.SceneManager.LoadScene(
            "Gameplay", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}