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
    [SerializeField] private Button closeLobbyButton;

    private ISession currentSession; // store this when you create it

    public async void OnHostClicked()
    {
        hostButton.interactable = false;

        try
        {
            await ServicesBootstrap.InitTask;

            ActiveSessionManager.ClearIfExists(); // replaces the old "if (currentSession != null)..." block

            NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;

            var options = new SessionOptions
            {
                Name = string.IsNullOrEmpty(lobbyNameInput.text) ? "Unnamed Lobby" : lobbyNameInput.text,
                MaxPlayers = 4,
                IsPrivate = false,
                Password = PasswordUtils.Normalize(passwordInput.text)
            }.WithRelayNetwork();

            ISession newSession = await MultiplayerService.Instance.CreateSessionAsync(options);
            ActiveSessionManager.Set(newSession); // replaces "currentSession = newSession"

            Debug.Log($"Hosting '{newSession.Name}' — ID: {newSession.Id}");

            startGameButton.SetActive(true);
            closeLobbyButton.gameObject.SetActive(true);
            classSelectUI.Show();
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
    
    public async void OnCloseLobbyClicked()
    {
        if (ActiveSessionManager.Current == null) return;

        try
        {
            await ActiveSessionManager.Current.AsHost().DeleteAsync();
            Debug.Log("Lobby closed.");
            ActiveSessionManager.Set(null);
            closeLobbyButton.gameObject.SetActive(false);
            startGameButton.SetActive(false);
            hostButton.interactable = true; // NEW — re-enable so they can host again
        }
        catch (SessionException e)
        {
            Debug.LogError($"Failed to close lobby: {e}");
        }
    }
}