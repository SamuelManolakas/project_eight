using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider staminaSlider;

    private void Awake()
    {
        Instance = this;
    }

    public void SetMaxHealth(int max)
    {
        healthSlider.maxValue = max;
        healthSlider.value = max;
    }

    public void SetHealth(int health)
    {
        healthSlider.value = health;
    }
    
    public void SetMaxStamina(int max)
    {
        staminaSlider.maxValue = max;
        staminaSlider.value = max;
    }

    public void SetStamina(float stamina)
    {
        staminaSlider.value = stamina;
    }
}