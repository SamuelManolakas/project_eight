using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Multiplayer;

public class PasswordPopup : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private ClassSelectUI classSelectUI;

    private ISessionInfo pendingSession;
    
    private int selectedClassIndex = 0;

    public void OnClassSelected(int classIndex) => selectedClassIndex = classIndex;

    private void Awake()
    {
        panel.SetActive(false);
        confirmButton.onClick.AddListener(OnConfirmClicked);
        cancelButton.onClick.AddListener(Hide);
    }

    public void Show(ISessionInfo sessionInfo)
    {
        pendingSession = sessionInfo;
        passwordInput.text = "";
        errorText.gameObject.SetActive(false);
        panel.SetActive(true);
    }

    private void Hide()
    {
        panel.SetActive(false);
        pendingSession = null;
    }

    private async void OnConfirmClicked()
    {
        if (pendingSession == null) return;

        confirmButton.interactable = false;
        errorText.gameObject.SetActive(false);

        try
        {
            var options = new JoinSessionOptions
            {
                Password = string.IsNullOrEmpty(passwordInput.text) ? null : passwordInput.text
            };

            // Inside PasswordPopup, before JoinSessionByIdAsync:
            NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
            //NetworkManager.Singleton.NetworkConfig.ConnectionData = new byte[] { (byte)selectedClassIndex };
            
            // In OnConfirmClicked, after a successful join:
            var session = await MultiplayerService.Instance.JoinSessionByIdAsync(pendingSession.Id, options);
            Debug.Log($"Joined session: {session.Name}");
            Hide();
            classSelectUI.Show(); // NEW
            // TODO: transition UI to "connected, waiting for host to start" state
        }
        catch (SessionException e)
        {
            errorText.text = "Incorrect password or session unavailable.";
            errorText.gameObject.SetActive(true);
            Debug.LogWarning($"Join failed: {e}");
        }
        finally
        {
            confirmButton.interactable = true;
        }
    }
}