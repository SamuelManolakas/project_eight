using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Multiplayer;
using UnityEngine;

public class SessionBrowser : MonoBehaviour
{
    public async Task<IList<ISessionInfo>> GetAvailableSessions()
    {
        try
        {
            var options = new QuerySessionsOptions();
            var results = await MultiplayerService.Instance.QuerySessionsAsync(options);

            Debug.Log($"Found {results.Sessions.Count} sessions.");
            foreach (var s in results.Sessions)
                Debug.Log($"{s.Id} | {s.Name} | Players: {s.MaxPlayers}");

            return results.Sessions;
        }
        catch (SessionException e)
        {
            Debug.LogError($"Query failed: {e}");
            return null;
        }
    }
    
    public async Task JoinSession(string sessionId, string password, int classIndex)
    {
        try
        {
            var options = new JoinSessionOptions
            {
                Password = string.IsNullOrEmpty(password) ? null : password
            };

            var session = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId, options);
            Debug.Log($"Joined session: {session.Name}");
        }
        catch (SessionException e)
        {
            Debug.LogError($"Failed to join (wrong password, full, or session ended?): {e}");
        }
    }
}