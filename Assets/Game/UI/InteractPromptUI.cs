using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// HUD prompt telling the local player which button to press, e.g. "[C]  Pick up".
/// The key is read from the Interact action, so it follows rebinds and control schemes.
/// Keep this component on an always-active object (FindObjectOfType skips inactive ones)
/// and assign a child as promptText — that child is what gets shown and hidden.
/// </summary>
public class InteractPromptUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI promptText;

    private PlayerInput _playerInput;
    private InputAction _interactAction;
    private string _action;
    private string _labelScheme;

    private void Awake() => promptText.gameObject.SetActive(false);

    public void Bind(PlayerInput playerInput)
    {
        _playerInput = playerInput;
        _interactAction = playerInput != null ? playerInput.actions.FindAction("Interact") : null;
    }

    /// <summary>Shows the prompt with the given action text (e.g. "Pick up"), or hides it when null.</summary>
    public void SetPrompt(string action)
    {
        bool visible = action != null;
        if (visible && (action != _action || CurrentScheme != _labelScheme))
        {
            _action = action;
            RefreshLabel();
        }

        if (promptText.gameObject.activeSelf != visible)
            promptText.gameObject.SetActive(visible);
    }

    private string CurrentScheme => _playerInput != null ? _playerInput.currentControlScheme : null;

    private void RefreshLabel()
    {
        _labelScheme = CurrentScheme;

        string key = null;
        if (_interactAction != null)
        {
            key = _interactAction.GetBindingDisplayString(group: _labelScheme);
            if (string.IsNullOrEmpty(key)) key = _interactAction.GetBindingDisplayString(); // no binding for this scheme
        }
        if (string.IsNullOrEmpty(key)) key = "Interact";

        promptText.text = $"[{key}]  {_action}";
    }
}
