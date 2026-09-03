using UnityEngine;

public class GameplayClassSelectUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private GameplayPauseUI pauseUI; // NEW

    private void Awake()
    {
        panel.SetActive(true);
    }

    public void OnClassButtonClicked(int classIndex)
    {
        if (GameplaySpawnManager.Instance == null)
        {
            Debug.LogWarning("Spawn manager not ready yet — try again in a moment.");
            return;
        }

        GameplaySpawnManager.Instance.RequestSpawnServerRpc(classIndex);
        panel.SetActive(false);

        if (pauseUI != null) // NEW
            pauseUI.LockCursorForGameplay();
    }
}