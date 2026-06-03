using System;
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

        if (stamina != null && stamina.IsGuarding.Value)
        {
            stamina.ConsumeStaminaServer(amount);
            return;
        }

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

    // New: called by a reviver
    public void Revive(int healthOnRevive = 30)
    {
        if (!IsServer) return;
        _health.Value = healthOnRevive;
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