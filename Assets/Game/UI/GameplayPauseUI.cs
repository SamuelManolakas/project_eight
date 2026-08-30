using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameplayPauseUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject closeLobbyButton;
    [SerializeField] private GameObject leaveLobbyButton;

    private const string MainMenuSceneName = "MainMenu";

    private void Awake()
    {
        panel.SetActive(false);
        SetCursorLocked(true); // NEW — locked/hidden as soon as the gameplay scene starts
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            TogglePanel();
    }

    private void TogglePanel()
    {
        bool showing = !panel.activeSelf;
        panel.SetActive(showing);

        SetCursorLocked(!showing); // NEW — locked when panel is hidden, free when panel is showing

        if (showing)
            RefreshButtonVisibility();
    }

    private void SetCursorLocked(bool locked) // NEW
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void RefreshButtonVisibility()
    {
        bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
        closeLobbyButton.SetActive(isHost);
        leaveLobbyButton.SetActive(!isHost);
    }

    public async void OnCloseLobbyClicked()
    {
        if (ActiveSessionManager.Current == null) return;

        try
        {
            NetworkManager.Singleton.SceneManager.LoadScene(MainMenuSceneName, LoadSceneMode.Single);
            await ActiveSessionManager.Current.AsHost().DeleteAsync();
            ActiveSessionManager.Set(null);
        }
        catch (SessionException e)
        {
            Debug.LogError($"Failed to close lobby: {e}");
        }
    }

    public async void OnLeaveLobbyClicked()
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
            NetworkManager.Singleton.Shutdown();
            SetCursorLocked(false); // NEW — restore cursor before returning to the menu
            SceneManager.LoadScene(MainMenuSceneName, LoadSceneMode.Single);
        }
    }
}