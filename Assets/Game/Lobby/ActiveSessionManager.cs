using System;
using System.Threading.Tasks;
using Unity.Services.Multiplayer;
using UnityEngine;

public static class ActiveSessionManager
{
    public static ISession Current { get; private set; }

    public static event Action<ISession> SessionChanged; // fires whenever we join/host/leave

    public static void Set(ISession session)
    {
        Current = session;
        SessionChanged?.Invoke(Current);
    }

    // In ActiveSessionManager.cs
    public static async Task ClearIfExists()
    {
        if (Current == null) return;

        try
        {
            await Current.LeaveAsync();
        }
        catch (SessionException e)
        {
            Debug.LogError($"Failed to leave previous session: {e}");
        }
        finally
        {
            Current = null;
            SessionChanged?.Invoke(null);
        }
    }
}