using System.Threading.Tasks;
using Unity.Services.Multiplayer;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public async Task HostSession(string lobbyName, string password, int classIndex)
    {
        try
        {
            var options = new SessionOptions
            {
                Name = lobbyName,
                MaxPlayers = 4,
                IsPrivate = false, // false = shows up in QuerySessionsAsync for everyone
                Password = string.IsNullOrEmpty(password) ? null : password
            }.WithRelayNetwork();

            var session = await MultiplayerService.Instance.CreateSessionAsync(options);

            Debug.Log($"Hosting '{session.Name}' — Session ID: {session.Id}");
            // NGO's NetworkManager.StartHost() is triggered automatically by the SDK here
        }
        catch (SessionException e)
        {
            Debug.LogError($"Failed to create session: {e}");
        }
    }
}