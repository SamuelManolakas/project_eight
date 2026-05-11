using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyController : NetworkBehaviour
{
    [SerializeField]
    private InteractionDetector m_interactionDetector;
    [SerializeField]
    private Animator m_animator;
    [SerializeField]
    private AnimationEvents m_animationEvents;
    
    [SerializeField]
    private GameObject _greatSwordModel;
    
    private NetworkVariable<ulong> m_heldNetworkObjectId = new(ulong.MaxValue);
    private NetworkVariable<ObjectType> m_heldObjectType = new(ObjectType.None);
    
    public float speed;
    public float jumpHeight = 2f;
    public float gravity = -9.8f;
    
    private CharacterController controller;
    private GameObject player;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        m_interactionDetector.Initialize(IsOwner);
        m_heldObjectType.OnValueChanged += HandleHeldItemChanged;
        HandleItemOnJoin();
        if (IsOwner)
        {
            m_animationEvents.OnInteract += HandleInteractAction;
            m_animationEvents.OnChop += HandleChopAction;
        }
        player = GameObject.FindWithTag("Player");
    }

    private void HandleChopAction()
    {
        if(m_heldObjectType.Value is ObjectType.Axe or ObjectType.PickAxe)
        {
            if(m_interactionDetector.ClosestInteractable is ResourceNode)
            {
                RequestResourceNodeInteractionServerRpc(
                    m_interactionDetector.ClosestInteractable.NetworkObject.NetworkObjectId);
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestResourceNodeInteractionServerRpc(ulong networkObjectId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects
               .TryGetValue(networkObjectId, out NetworkObject target))
            return;

        if (!target.TryGetComponent(out ResourceNode node))
            return;

        node.Harvest(m_heldObjectType.Value);
    }

    private void HandleItemOnJoin()
    {
        if(m_heldObjectType.Value != ObjectType.None)
        {
            HandleHeldItemChanged(ObjectType.None, m_heldObjectType.Value);
        }
    }

    private void HandleHeldItemChanged(ObjectType previousValue, ObjectType newValue)
    {
        _greatSwordModel.SetActive(newValue == ObjectType.Axe);
    }

    private void HandleInteractAction()
    {
        if(m_interactionDetector.ClosestInteractable is PickableBase)
        {
            RequestPickUpServerRpc(
                m_interactionDetector.ClosestInteractable.NetworkObject.NetworkObjectId);
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestPickUpServerRpc(ulong networkObjectId)
    {
        if(!NetworkManager.SpawnManager.SpawnedObjects
            .TryGetValue(networkObjectId, out NetworkObject target))
        {
            return;
        }
        if(!target.TryGetComponent(out PickableBase pickableItem))
        {
            return;
        }
        if(!pickableItem.CanBePickedUp)
        {
            return;
        }
        if(m_heldObjectType.Value != ObjectType.None)
        {
            DropCurrentItem();
        }
        if(pickableItem is PickableTool)
        {
            m_heldNetworkObjectId.Value = networkObjectId;
        }


        m_heldObjectType.Value = pickableItem.ObjectType;
        pickableItem.PickUp();
    }

    private void DropCurrentItem()
    {
        if(IsServer == false)
        {
            return;
        }
        if(m_heldObjectType.Value == ObjectType.None)
        {
            m_heldNetworkObjectId.Value = ulong.MaxValue;
            return;
        }
        if(m_heldObjectType.Value is ObjectType.Axe or ObjectType.PickAxe)
        {
            if(NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(
                m_heldNetworkObjectId.Value, out NetworkObject target))
            {
                if(target.TryGetComponent(out PickableTool pickableItem))
                {
                    pickableItem.Drop(transform.position);
                }
            }
        }
        m_heldObjectType.Value = ObjectType.None;
        m_heldNetworkObjectId.Value = ulong.MaxValue;
    }

    public override void OnNetworkDespawn()
    {
        m_heldObjectType.OnValueChanged -= HandleHeldItemChanged;
        if (IsOwner)
        {
            RequestDropServerRpc();
            m_animationEvents.OnInteract -= HandleInteractAction;
            m_animationEvents.OnChop -= HandleChopAction;
        }
        base.OnNetworkDespawn();
    }

    [Rpc(SendTo.Server)]
    private void RequestDropServerRpc()
    {
        DropCurrentItem();
    }

    private void Update()
    {
        if(IsOwner == false)
        {
            return;
        }
        

        Vector3 move = player.transform.position - transform.position;
        move = Vector3.ClampMagnitude(move, 1);

        Vector3 finalMove = move * speed;
        
        controller.Move(finalMove * Time.deltaTime);
        m_animator.SetFloat("speed", move.magnitude);

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 10f);
        }
    }
}