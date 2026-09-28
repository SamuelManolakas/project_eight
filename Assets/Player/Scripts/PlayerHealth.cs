using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    
    private NetworkVariable<int> _health = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // 0–1 while this player lies downed inside a ReviveZone; readable on all clients for UI.
    public NetworkVariable<float> ReviveProgress = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private PlayerBehaviour player;
    private PlayerStamina stamina;

    public int Health => _health.Value;
    public bool IsAlive => _health.Value > 0f;

    private void Awake()
    {
        player = GetComponent<PlayerBehaviour>();
        stamina = GetComponent<PlayerStamina>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            _health.Value = maxHealth;

        _health.OnValueChanged += OnHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        _health.OnValueChanged -= OnHealthChanged;
    }

    public void GetHit(int damage)
    {
        if (!IsServer) return;
        if (!IsAlive) return;
        
        player.GetHit(damage);
    }

    // Called directly by the server-side enemy — no RPC needed
    public void TakeDamage(int amount)
    {
        if (!IsServer) return;
        if (!IsAlive) return;

        // A blocked hit costs stamina instead of health. Stamina is written by its owner, so the
        // server decides the outcome from the value it has and asks the owner to spend it.
        if (stamina != null && stamina.IsGuarding.Value)
        {
            bool canAfford = stamina.Stamina >= amount;
            float remaining = stamina.Stamina - amount;
            stamina.DrainStaminaRpc(amount); // clamps at 0, so an unaffordable hit empties the bar

            if (!canAfford)
                ApplyDamage(amount); // the hit goes through the guard

            if (remaining <= 1f && IsAlive) // guard broken (also on an unaffordable hit)
                player.TransitToGuardBrokenStateClientRpc();

            return;
        }

        ApplyDamage(amount);
    }

    private void ApplyDamage(int amount)
    {
        _health.Value = Mathf.Max(0, _health.Value - amount);

        if (!IsAlive)
            HandleDeath();
    }

    public void Heal(int amount)
    {
        if (!IsAlive) return;

        if (IsServer)
        {
            ApplyHeal(amount);
        }
        else if (IsOwner)
        {
            HealServerRpc(amount);
        }
    }

    [ServerRpc]
    private void HealServerRpc(int amount)
    {
        if (!IsAlive) return;
        ApplyHeal(amount);
    }

    private void ApplyHeal(int amount)
    {
        _health.Value = Mathf.Min(maxHealth, _health.Value + amount);
    }

    private void HandleDeath()
    {
        OnDownedClientRpc(); // enter downed state instead
    }

    [ClientRpc]
    private void OnDownedClientRpc()
    {
        player.TransitToDownedStateClientRpc();
    }

    // Called by a ReviveZone once revive progress completes
    public void Revive(int healthOnRevive = 30)
    {
        if (!IsServer) return;
        _health.Value = healthOnRevive;
        ReviveProgress.Value = 0f;
    }

    private void OnHealthChanged(int previous, int current)
    {
        // Fires on all clients — update health bar UI here

        if (IsOwner)
        {
            HUDManager.Instance.SetMaxHealth(maxHealth);
            HUDManager.Instance.SetHealth(_health.Value);
        }
    }
}