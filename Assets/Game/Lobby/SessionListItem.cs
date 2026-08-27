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
        playerCountText.text = $"{info.MaxPlayers} max";

        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(OnJoinClicked);
    }

    private void OnJoinClicked()
    {
        browser.RequestJoin(sessionInfo);
    }
}