using Unity.Netcode;
using UnityEngine;

public abstract class PickableBase : NetworkBehaviour, IInteractable
{
    protected NetworkVariable<bool> m_isAVariable =
        new(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private SelectionOutline m_outline;
    [SerializeField] private ObjectType m_objectType;
    
    public bool CanBePickedUp => m_isAVariable.Value;
    public ObjectType ObjectType => m_objectType;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        m_isAVariable.OnValueChanged += OnAvailabilityChanged;
        ApplyAvailabilityState(m_isAVariable.Value);
    }

    public override void OnNetworkDespawn()
    {
        m_isAVariable.OnValueChanged -= OnAvailabilityChanged;
        base.OnNetworkDespawn();
    }

    private void OnAvailabilityChanged(bool previousValue, bool newValue)
    {
        ApplyAvailabilityState(newValue);
    }

    protected abstract void ApplyAvailabilityState(bool newValue);

    public void PickUp()
    {
        if (IsServer == false)
        {
            return;
        }
        m_isAVariable.Value = false;
        OnPickedUp();
    }

    protected abstract void OnPickedUp();

    public void ToggleSelection(bool isSelected)
    {
        if(m_outline != null)
            m_outline.ToggleOutline(isSelected);
    }
}
