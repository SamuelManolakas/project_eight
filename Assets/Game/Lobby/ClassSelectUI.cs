using Unity.Services.Multiplayer;
using UnityEngine;

public class ClassSelectUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    private void Awake()
    {
        panel.SetActive(false);
    }

    private void OnEnable()
    {
        ActiveSessionManager.SessionChanged += HandleSessionChanged;

        if (ActiveSessionManager.Current != null)
            panel.SetActive(true);
    }

    private void OnDisable()
    {
        ActiveSessionManager.SessionChanged -= HandleSessionChanged;
    }

    private void HandleSessionChanged(ISession session)
    {
        panel.SetActive(session != null);
    }

    // Wire each of your 3 buttons to call this with a different index (0, 1, 2)
    public void OnClassButtonClicked(int classIndex)
    {
        ClassSelectionManager.Instance.SubmitClassChoiceServerRpc(classIndex);
        Debug.Log($"Sent class choice: {classIndex}");
    }
}