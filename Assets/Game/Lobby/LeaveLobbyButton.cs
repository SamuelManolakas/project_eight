using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class LeaveLobbyButton : MonoBehaviour
{
    [SerializeField] private Button leaveButton;

    private void Awake()
    {
        leaveButton.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        ActiveSessionManager.SessionChanged += HandleSessionChanged;

        if (ActiveSessionManager.Current != null)
            HandleSessionChanged(ActiveSessionManager.Current);
    }

    private void OnDisable()
    {
        ActiveSessionManager.SessionChanged -= HandleSessionChanged;
    }

    private void HandleSessionChanged(ISession session)
    {
        bool isJoiningClient = session != null
                               && NetworkManager.Singleton != null
                               && !NetworkManager.Singleton.IsHost;

        leaveButton.gameObject.SetActive(isJoiningClient);
    }

    public async void OnLeaveClicked()
    {
        if (ActiveSessionManager.Current == null) return;

        try
        {
            await ActiveSessionManager.Current.LeaveAsync();
        }
        catch (SessionException e)
        {
            Debug.LogError($"Failed to leave: {e}");
        }
        finally
        {
            ActiveSessionManager.Set(null);
        }
    }
}