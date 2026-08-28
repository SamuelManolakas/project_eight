using UnityEngine;

public class ClassSelectUI : MonoBehaviour
{
    [SerializeField] private GameObject panel; // the parent object holding the 3 buttons

    private void Awake()
    {
        panel.SetActive(false);
    }

    public void Show()
    {
        panel.SetActive(true);
    }

    // Wire each of your 3 buttons to call this with a different index (0, 1, 2)
    public void OnClassButtonClicked(int classIndex)
    {
        ClassSelectionManager.Instance.SubmitClassChoiceServerRpc(classIndex);
        Debug.Log($"Sent class choice: {classIndex}");

        // Optional: visually indicate selection, e.g. highlight the chosen button
        // or hide the panel entirely once picked — your call
    }
}