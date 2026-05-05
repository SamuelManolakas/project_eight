using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [SerializeField] private Slider healthSlider;

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
}