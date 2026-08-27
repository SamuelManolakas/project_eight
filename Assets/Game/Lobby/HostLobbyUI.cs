using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class HostLobbyUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField lobbyNameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button hostButton;
    [SerializeField] private GameObject startGameButton; // shown only to host after session created

    private int selectedClassIndex = 0;

    public void OnClassSelected(int classIndex) => selectedClassIndex = classIndex;

    public async void OnHostClicked()
    {
        hostButton.interactable = false;

        try
        {
            await ServicesBootstrap.InitTask;

            // Class choice travels in NGO's connection payload, same as before
            NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
            NetworkManager.Singleton.NetworkConfig.ConnectionData = new byte[] { (byte)selectedClassIndex };
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;

            var options = new SessionOptions
            {
                Name = string.IsNullOrEmpty(lobbyNameInput.text) ? "Unnamed Lobby" : lobbyNameInput.text,
                MaxPlayers = 4,
                IsPrivate = false,
                Password = string.IsNullOrEmpty(passwordInput.text) ? null : passwordInput.text
            }.WithRelayNetwork(); // ⚠️ VERIFY: confirm the exact extension method name (WithRelayNetwork /
                                   // WithNetcodeForGameObjects / similar) against your installed package version's
                                   // autocomplete or docs — Unity has renamed this across SDK revisions.

            var session = await MultiplayerService.Instance.CreateSessionAsync(options);
            Debug.Log($"Hosting '{session.Name}' — ID: {session.Id}");

            startGameButton.SetActive(true);
        }
        catch (SessionException e)
        {
            Debug.LogError($"Failed to host session: {e}");
            hostButton.interactable = true;
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        byte classIndex = request.Payload.Length > 0 ? request.Payload[0] : (byte)0;
        PlayerClassRegistry.Instance.SetClassChoice(request.ClientNetworkId, classIndex);
        response.Approved = true;
        response.CreatePlayerObject = false;
    }

    public void OnStartGameClicked()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        NetworkManager.Singleton.SceneManager.LoadScene("Gameplay", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}