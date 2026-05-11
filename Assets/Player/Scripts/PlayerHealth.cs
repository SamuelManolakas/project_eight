using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;

    public NetworkVariable<int> _health = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private PlayerBehaviour player;
    
    public int Health => _health.Value;
    public bool IsAlive => _health.Value > 0f;

    private void Awake()
    {
        player = GetComponent<PlayerBehaviour>();
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

        _health.Value = Mathf.Max(0, _health.Value - amount);

        if (!IsAlive)
            HandleDeath();
    }

    private void HandleDeath()
    {
        // Notify all clients the player has died
        OnDeathClientRpc();
    }

    [ClientRpc]
    private void OnDeathClientRpc()
    {
        // Fires on all clients — play death animation, disable input, show UI, etc.
        Debug.Log($"{gameObject.name} has died.");
        
        player.TransitToDeadStateClientRpc();
    }

    private void OnHealthChanged(int previous, int current)
    {
        // Fires on all clients — update health bar UI here
        Debug.Log($"Health changed: {previous} → {current}");

        if (IsOwner)
        {
            HUDManager.Instance.SetMaxHealth(maxHealth);
            HUDManager.Instance.SetHealth(_health.Value);
        }
    }
}