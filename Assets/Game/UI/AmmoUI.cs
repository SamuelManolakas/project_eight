using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private GameObject ammoSprite;

    private PlayerBehaviour _player;

    public void Bind(PlayerBehaviour player)
    {
        if (_player != null)
            _player.OnAmmoChanged -= UpdateDisplay;

        _player = player;

        // Only bolter class has ammo — hide the whole panel otherwise
        gameObject.SetActive(player.bolter);
        ammoSprite.SetActive(player.bolter);
        if (!player.bolter) return;

        _player.OnAmmoChanged += UpdateDisplay;
        UpdateDisplay(_player.ammo, _player.maxAmmo);
    }

    private void UpdateDisplay(int current, int max)
    {
        ammoText.text = $"{current} / {max}";
    }

    private void OnDestroy()
    {
        if (_player != null)
            _player.OnAmmoChanged -= UpdateDisplay;
    }
}