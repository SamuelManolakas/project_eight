using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quits the game when its button is clicked (stops Play mode when running in the editor).
/// </summary>
[RequireComponent(typeof(Button))]
public class ExitGameButton : MonoBehaviour
{
    private void Awake() => GetComponent<Button>().onClick.AddListener(ExitGame);

    private void ExitGame()
    {
        // Disconnect cleanly first if we're hosting or connected
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
