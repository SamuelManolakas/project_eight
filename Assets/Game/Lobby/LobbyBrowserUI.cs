using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Multiplayer;
using UnityEngine;

public class LobbyBrowserUI : MonoBehaviour
{
    [SerializeField] private Transform contentParent;   // ScrollRect's Content object
    [SerializeField] private GameObject sessionListItemPrefab;
    [SerializeField] private PasswordPopup passwordPopup;
    [SerializeField] private GameObject refreshingIndicator; // optional spinner/text
    [SerializeField] private GameObject lobbyFullMessagePanel; // simple panel with text + OK/close button
    
    private readonly List<GameObject> spawnedItems = new();

    public void ShowLobbyFullMessage()
    {
        lobbyFullMessagePanel.SetActive(true);
    }

    public void HideLobbyFullMessage()
    {
        lobbyFullMessagePanel.SetActive(false);
    }

    public async void OnRefreshClicked()
    {
        ClearList();
        if (refreshingIndicator != null) refreshingIndicator.SetActive(true);

        try
        {
            await ServicesBootstrap.InitTask; // waits here if not finished yet, returns instantly if it already is

            var options = new QuerySessionsOptions();
            var results = await MultiplayerService.Instance.QuerySessionsAsync(options);
            
            // In LobbyBrowserUI.OnRefreshClicked, right after QuerySessionsAsync:
            Debug.Log($"Query returned {results.Sessions.Count} sessions");
            foreach (var s in results.Sessions)
                Debug.Log($"Found: {s.Name} | {s.Id}");

            foreach (var sessionInfo in results.Sessions)
            {
                GameObject itemGO = Instantiate(sessionListItemPrefab, contentParent);
                itemGO.GetComponent<SessionListItem>().Setup(sessionInfo, this);
                spawnedItems.Add(itemGO);
            }

            if (results.Sessions.Count == 0)
                Debug.Log("No public sessions found.");
        }
        catch (SessionException e)
        {
            Debug.LogError($"Failed to query sessions: {e}");
        }
        finally
        {
            if (refreshingIndicator != null) refreshingIndicator.SetActive(false);
        }
    }

    private void ClearList()
    {
        foreach (var item in spawnedItems)
            Destroy(item);
        spawnedItems.Clear();
    }

    // Called by SessionListItem when its Join button is clicked
    public void RequestJoin(ISessionInfo sessionInfo)
    {
        passwordPopup.Show(sessionInfo);
    }
    
    public async Task JoinDirectly(ISessionInfo sessionInfo)
    {
        try
        {
            ActiveSessionManager.ClearIfExists();

            var options = new JoinSessionOptions();
            ISession session = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionInfo.Id, options);
            ActiveSessionManager.Set(session);
            Debug.Log($"Joined session: {session.Name}");
        }
        catch (SessionException e)
        {
            Debug.LogError($"Failed to join: {e}");
        }
    }

    private void OnEnable() => OnRefreshClicked(); // auto-refresh when the browser opens
}