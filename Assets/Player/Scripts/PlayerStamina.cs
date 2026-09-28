using Unity.Netcode;
using UnityEngine;

public class PlayerStamina : NetworkBehaviour
{
     [Header("Stamina Settings")]
        public float maxStamina = 100f;
        public float staminaRegenRate = 10f;
        public float staminaRegenDelay = 1f; // Delay before regen starts
        
        private float _regenDelayTimer = 0f;
    
        // Owner can write directly — no ServerRpc needed.
        // Read = Owner: only the owner (HUD, input checks) and the server (guard) need it. It changes
        // every frame while regenerating, so this stops it being sent to every other client.
        [HideInInspector]
        public NetworkVariable<float> _stamina = new NetworkVariable<float>(
            100f,
            NetworkVariableReadPermission.Owner,
            NetworkVariableWritePermission.Owner
        );

        // Owner sets this when guarding — server reads it
        [HideInInspector]
        public NetworkVariable<bool> IsGuarding = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Owner,
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
            if (!IsOwner) return;

            // Count down the delay timer
            if (_regenDelayTimer > 0f)
            {
                _regenDelayTimer -= Time.deltaTime;
                return;
            }

            if (_stamina.Value < maxStamina)
                _stamina.Value = Mathf.Min(_stamina.Value + staminaRegenRate * Time.deltaTime, maxStamina);
        }
    
        public bool TryUseStamina(float amount)
        {
            if (!IsOwner || _stamina.Value < amount) return false;

            _stamina.Value = Mathf.Max(0f, _stamina.Value - amount);
            _regenDelayTimer = staminaRegenDelay; // Reset the delay on use
            return true;
        }

        /// <summary>
        /// Server → owner. Stamina is written by its owner, so when the server needs to spend it
        /// (e.g. a blocked hit while guarding) it asks the owner. Runs locally when the owner is the host.
        /// </summary>
        [Rpc(SendTo.Owner)]
        public void DrainStaminaRpc(float amount)
        {
            _stamina.Value = Mathf.Max(0f, _stamina.Value - amount);
            _regenDelayTimer = staminaRegenDelay;
        }
    
        private void OnStaminaChanged(float previous, float current)
        {
            // Fires on the owner and server only (read permission is Owner)
        }
    
}
