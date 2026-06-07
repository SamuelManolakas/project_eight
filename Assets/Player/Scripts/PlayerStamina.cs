using Unity.Netcode;
using UnityEngine;

public class PlayerStamina : NetworkBehaviour
{
     [Header("Stamina Settings")]
        public float maxStamina = 100f;
        public float staminaRegenRate = 10f;
    
        // Owner can write directly — no ServerRpc needed
        [HideInInspector]
        public NetworkVariable<float> _stamina = new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
    
        // Owner sets this when guarding — server reads it
        [HideInInspector]
        public NetworkVariable<bool> IsGuarding = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        public float Stamina => _stamina.Value;
    
        public override void OnNetworkSpawn()
        {
            if (IsOwner)
                _stamina.Value = maxStamina;
    
            _stamina.OnValueChanged += OnStaminaChanged;
        }
    
        public override void OnNetworkDespawn()
        {
            _stamina.OnValueChanged -= OnStaminaChanged;
        }
    
        private void Update()
        {
            // Regen runs on the owner (client or host)
            if (!IsOwner) return;
    
            if (_stamina.Value < maxStamina)
                _stamina.Value = Mathf.Min(_stamina.Value + staminaRegenRate * Time.deltaTime, maxStamina);
        }
    
        public bool TryUseStamina(float amount)
        {
            if (!IsOwner || _stamina.Value < amount) return false;
    
            _stamina.Value = Mathf.Max(0f, _stamina.Value - amount);
            
            return true;
        }
    
        private void OnStaminaChanged(float previous, float current)
        {
            // Still fires on all clients — use for UI updates
        }
    
}
