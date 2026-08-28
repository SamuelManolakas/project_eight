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
    [SerializeField] private GameObject startGameButton;
    [SerializeField] private ClassSelectUI classSelectUI; // NEW

    public async void OnHostClicked()
    {
        hostButton.interactable = false;

        try
        {
            await ServicesBootstrap.InitTask;

            NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            // NOTE: no more ConnectionData/class payload here — class choice now happens after connecting

            var options = new SessionOptions
            {
                Name = string.IsNullOrEmpty(lobbyNameInput.text) ? "Unnamed Lobby" : lobbyNameInput.text,
                MaxPlayers = 4,
                IsPrivate = false,
                Password = string.IsNullOrEmpty(passwordInput.text) ? null : passwordInput.text
            }.WithRelayNetwork();

            var session = await MultiplayerService.Instance.CreateSessionAsync(options);
            Debug.Log($"Hosting '{session.Name}' — ID: {session.Id}");

            startGameButton.SetActive(true);
            classSelectUI.Show(); // NEW — host picks their own class too
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
        // No longer reading a class payload here — approval just admits the connection
        response.Approved = true;
        response.CreatePlayerObject = false;
    }

    public void OnStartGameClicked()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        NetworkManager.Singleton.SceneManager.LoadScene("Gameplay", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}