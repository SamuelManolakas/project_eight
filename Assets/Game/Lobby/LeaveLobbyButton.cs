using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class LeaveLobbyButton : MonoBehaviour
{
    [SerializeField] private Button leaveButton; // the button component itself, or use gameObject if it IS the button

    private void Awake()
    {
        leaveButton.gameObject.SetActive(false); // start hidden
    }

    private void OnEnable()
    {
        ActiveSessionManager.SessionChanged += HandleSessionChanged;

        // Cover the case where a session already exists when this becomes active
        if (ActiveSessionManager.Current != null)
            leaveButton.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        ActiveSessionManager.SessionChanged -= HandleSessionChanged;
    }

    private void HandleSessionChanged(ISession session)
    {
        leaveButton.gameObject.SetActive(session != null);
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
            ActiveSessionManager.Set(null); // this fires SessionChanged, which hides this button automatically
        }
    }
}