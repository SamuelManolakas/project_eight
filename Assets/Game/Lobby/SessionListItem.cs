using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Multiplayer;

public class SessionListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text sessionNameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private Button joinButton;

    private ISessionInfo sessionInfo;
    private LobbyBrowserUI browser;

    public void Setup(ISessionInfo info, LobbyBrowserUI ownerBrowser)
    {
        sessionInfo = info;
        browser = ownerBrowser;

        sessionNameText.text = info.Name;

        int currentPlayers = info.MaxPlayers - info.AvailableSlots;
        playerCountText.text = $"{currentPlayers}/{info.MaxPlayers}";

        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(OnJoinClicked);
    }

    // In SessionListItem.cs
    private async void OnJoinClicked()
    {
        if (sessionInfo.AvailableSlots <= 0)
        {
            browser.ShowLobbyFullMessage();
            return;
        }

        if (!sessionInfo.HasPassword)
        {
            await browser.JoinDirectly(sessionInfo); // new method, no popup needed
            return;
        }

        browser.RequestJoin(sessionInfo); // existing popup flow, unchanged
    }
}   