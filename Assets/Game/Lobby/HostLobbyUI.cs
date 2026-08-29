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
    [SerializeField] private TMP_Text errorText; // NEW

    private ISession currentSession; // store this when you create it

    public async void OnHostClicked()
    {
        if (string.IsNullOrEmpty(passwordInput.text))
        {
            ShowError("A password is required to host a lobby.");
            return;
        }

        hostButton.interactable = false;
        HideError(); // clear any previous error once a valid attempt starts

        try
        {
            await ServicesBootstrap.InitTask;
            await ActiveSessionManager.ClearIfExists();

            NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
            NetworkManager.Singleton.ConnectionApprovalCallback = null;
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;

            var options = new SessionOptions
            {
                Name = string.IsNullOrEmpty(lobbyNameInput.text) ? "Unnamed Lobby" : lobbyNameInput.text,
                MaxPlayers = 4,
                IsPrivate = false,
                Password = PasswordUtils.Normalize(passwordInput.text)
            }.WithRelayNetwork();

            ISession newSession = await MultiplayerService.Instance.CreateSessionAsync(options);
            ActiveSessionManager.Set(newSession);

            Debug.Log($"Hosting '{newSession.Name}' — ID: {newSession.Id}");

            startGameButton.SetActive(true);
            closeLobbyButton.gameObject.SetActive(true);
        }
        catch (SessionException e)
        {
            Debug.LogError($"Failed to host session: {e}");
            ShowError("Failed to create lobby. Please try again.");
            hostButton.interactable = true;
        }
    }

    private void HideError()
    {
        errorText.gameObject.SetActive(false);
    }

    private void ShowError(string message)
    {
        Debug.LogWarning(message);
        // If you have an error text field on this panel, show it here too:
        errorText.text = message;
        errorText.gameObject.SetActive(true);
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