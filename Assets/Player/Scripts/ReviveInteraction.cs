using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReviveInteraction : NetworkBehaviour
{
    [Header("Settings")]
    public float reviveRadius = 2f;
    public float reviveDuration = 3f;

    private PlayerBehaviour _self;
    private float _holdTime = 0f;
    private PlayerBehaviour _currentTarget = null;
    private bool _isHolding = false;

    private void Awake() => _self = GetComponent<PlayerBehaviour>();

    public void OnRevive(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        if (context.performed)
            _isHolding = true;
        else if (context.canceled)
        {
            _isHolding = false;
            _holdTime = 0f;
            _currentTarget = null;
            //HUDManager.Instance.SetReviveProgress(0f);
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (!_isHolding)
        {
            PlayerBehaviour target = FindDownedPlayerInRange();
            //HUDManager.Instance.ShowRevivePrompt(target != null);
            return;
        }

        PlayerBehaviour currentTarget = FindDownedPlayerInRange();

        if (currentTarget == null)
        {
            _holdTime = 0f;
            _currentTarget = null;
            //HUDManager.Instance.ShowRevivePrompt(false);
            //HUDManager.Instance.SetReviveProgress(0f);
            return;
        }

        //HUDManager.Instance.ShowRevivePrompt(true);

        if (_currentTarget != currentTarget)
        {
            _holdTime = 0f;
            _currentTarget = currentTarget;
        }

        _holdTime += Time.deltaTime;
        //HUDManager.Instance.SetReviveProgress(_holdTime / reviveDuration);

        if (_holdTime >= reviveDuration)
        {
            _holdTime = 0f;
            _isHolding = false;
            //HUDManager.Instance.SetReviveProgress(0f);
            RequestReviveServerRpc(_currentTarget.GetComponent<NetworkObject>().NetworkObjectId);
            _currentTarget = null;
        }
    }

    private PlayerBehaviour FindDownedPlayerInRange()
    {
        foreach (var obj in NetworkManager.Singleton.SpawnManager.SpawnedObjects.Values)
        {
            if (obj.gameObject == gameObject) continue;

            var other = obj.GetComponent<PlayerBehaviour>();
            if (other == null) continue;
            if (other.stateMachine.currentState != other.downedState) continue;

            if (Vector3.Distance(transform.position, other.transform.position) <= reviveRadius)
                return other;
        }
        return null;
    }

    [ServerRpc]
    private void RequestReviveServerRpc(ulong targetNetworkObjectId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects
                .TryGetValue(targetNetworkObjectId, out NetworkObject target)) return;

        var targetHealth = target.GetComponent<PlayerHealth>();
        var targetPlayer = target.GetComponent<PlayerBehaviour>();

        if (targetPlayer.stateMachine.currentState != targetPlayer.downedState) return;

        targetHealth.Revive(30);
        targetPlayer.ReviveClientRpc();
    }
}