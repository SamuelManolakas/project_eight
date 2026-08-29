using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;

public class CurrentLobbyUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text lobbyNameText;
    [SerializeField] private TMP_Text playerCountText;

    private ISession session;

    private void Awake()
    {
        panel.SetActive(false);
    }

    private void OnEnable()
    {
        ActiveSessionManager.SessionChanged += HandleSessionChanged;

        // Cover the case where a session already exists when this UI becomes active
        // (e.g. scene reload while already hosting/joined)
        if (ActiveSessionManager.Current != null)
            HandleSessionChanged(ActiveSessionManager.Current);
    }

    private void OnDisable()
    {
        ActiveSessionManager.SessionChanged -= HandleSessionChanged;
        UnsubscribeFromSession();
    }

    private void HandleSessionChanged(ISession newSession)
    {
        // Stop listening to the old session's updates before switching
        UnsubscribeFromSession();

        session = newSession;

        if (session == null)
        {
            panel.SetActive(false);
            return;
        }

        panel.SetActive(true);
        RefreshDisplay();

        session.Changed += OnSessionChanged; // ⚠️ verify exact event name via autocomplete on `session.`
    }

    private void OnSessionChanged()
    {
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        if (session == null) return;

        lobbyNameText.text = session.Name;

        int current = session.MaxPlayers - session.AvailableSlots; // both confirmed earlier via autocomplete
        playerCountText.text = $"{current}/{session.MaxPlayers}";
    }

    private void UnsubscribeFromSession()
    {
        if (session != null)
            session.Changed -= OnSessionChanged;
    }
}