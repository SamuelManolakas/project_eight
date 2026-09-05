using TMPro;
using UnityEngine;

public class HealUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healText;

    private PlayerBehaviour _player;

    public void Bind(PlayerBehaviour player)
    {
        if (_player != null)
            _player.OnHealConsumableChanged -= UpdateDisplay;

        _player = player;
        _player.OnHealConsumableChanged += UpdateDisplay;

        UpdateDisplay(_player.healConsumableAmount, _player.maxHealConsumableAmount);
    }

    private void UpdateDisplay(int current, int max)
    {
        healText.text = $"{current} / {max}";
    }

    private void OnDestroy()
    {
        if (_player != null)
            _player.OnHealConsumableChanged -= UpdateDisplay;
    }
}