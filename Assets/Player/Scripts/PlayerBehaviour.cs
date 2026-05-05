using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerBehaviour : NetworkBehaviour
{
    [Header("Variables")]
    public int maxHealth = 100;
    [HideInInspector] 
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(0, 
        NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    public float speed;
    public float sprintSpeed;
    public float jumpHeight;
    public float gravity;
    public float dodgeDistance;
    public int damage;
    
    public NetworkVariable<int> health;
    
    [Header("Components")]
    public Animator animator;
    //[SerializeField]
    //private AnimationEvents m_animationEvents;
    [SerializeField]
    private InteractionDetector m_interactionDetector;
    public GameObject greatSwordModel;
    public GameObject hudPrefab;
    
    [HideInInspector] 
    public Transform cameraTransform;
    [HideInInspector] 
    public CharacterController controller;
    [HideInInspector] 
    public Vector2 moveInput;
    [HideInInspector] 
    public Vector3 velocity;
    [HideInInspector] 
    public float initialSpeed;
    [HideInInspector] 
    public HitBox hitBox;
    [HideInInspector] 
    public HurtBox hurtBox;
    [HideInInspector] 
    public bool attackBuffer;
    
    private NetworkVariable<ulong> m_heldNetworkObjectId = new(ulong.MaxValue);
    private NetworkVariable<ObjectType> m_heldObjectType = new(ObjectType.None);
    
    [HideInInspector]
    public StateMachine stateMachine = null;
    
    public RootState rootState = null;
    public AliveState aliveState = null;
    public DeadState deadState = null;
    public SpawnState spawnState = null;
    public IdleState idleState = null;
    public MovementState movementState = null;
    public DodgeState dodgeState = null;
    public JumpState jumpState = null;
    public AttackState attackState = null;
    public JumpAttackState jumpAttackState = null;
    public GrabbedState grabbedState = null;
    public GuardState guardState = null;
    
    public void Awake(){
        rootState = new RootState(this, null);
        aliveState = new AliveState(this, rootState);
        deadState = new DeadState(this, rootState);
        spawnState = new SpawnState(this, rootState);
        idleState = new IdleState(this, aliveState);
        movementState = new MovementState(this, aliveState);
        dodgeState = new DodgeState(this, aliveState);
        jumpState = new JumpState(this, aliveState);
        attackState = new AttackState(this, aliveState);
        jumpAttackState = new JumpAttackState(this, attackState);
        grabbedState = new GrabbedState(this, aliveState);
        guardState = new GuardState(this, aliveState);
        
        stateMachine = new StateMachine();
        stateMachine.InitializeMachine(spawnState);
        
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
        initialSpeed = speed;
    }

    private void Start()
    {
        hitBox = greatSwordModel.GetComponent<HitBox>();
        hurtBox = GetComponent<HurtBox>();
        
        if (!IsServer)
        {
            return;
        }
        currentHealth.Value = maxHealth;
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        stateMachine.currentState.OnMove();
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if(!IsOwner) return;
        stateMachine.currentState.OnDodge();
    }
    
    public void OnAttack(InputAction.CallbackContext context)
    {
        if(!IsOwner) return;
        stateMachine.currentState.OnAttack();
        attackBuffer = true;
    }
    
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && controller.isGrounded)
        {
            stateMachine.currentState.OnJump();   
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        stateMachine.currentState.OnSprint();
    }

    public void OnGuard(InputAction.CallbackContext context)
    {
        if(!IsOwner) return;
        if (context.performed && controller.isGrounded)
            stateMachine.currentState.OnGuard();
        else if (context.canceled)
        {
            stateMachine.Transit(idleState);
        }
    }

    private void Update()
    {
        if(IsOwner == false)
        {
            return;
        }

        velocity.y += gravity * Time.deltaTime;

        ContinuousAction();
        
        //death check because death check in hit function does not work
        if (currentHealth.Value <= 0)
        {
            stateMachine.Transit(deadState);
        }
    }

    private void ContinuousAction(){stateMachine.currentState.ContinuousAction();}
    
    public void GetHit(int damage)
    {
        if(!IsServer) return;
        stateMachine.currentState.GetHit(damage);
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        m_interactionDetector.Initialize(IsOwner);
        m_heldObjectType.OnValueChanged += HandleHeldItemChanged;
        HandleItemOnJoin();
        if (IsOwner)
        {
            //m_animationEvents.OnInteract += HandleInteractAction;
            //m_animationEvents.OnAnimationDone += HandleAnimationDone;
            //m_animationEvents.OnChop += HandleChopAction;
            
            Instantiate(hudPrefab);
        }

        if (IsOwner == false)
            return;
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ThirdPersonCamera>().target = transform;
        currentHealth.OnValueChanged += OnHealthChanged;
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        if (IsOwner)
        {
            HUDManager.Instance.SetMaxHealth(maxHealth);
            HUDManager.Instance.SetHealth(currentHealth.Value);
        }
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
        greatSwordModel.SetActive(newValue == ObjectType.Axe);
    }
    
    private void HandleAnimationDone()
    {
        //m_isInteracting = false;
        //m_isChopping = false;
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
        else
        {
            //m_resourceSpawner.SpawnResource(m_heldObjectType.Value, transform.position);
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
            //m_animationEvents.OnInteract -= HandleInteractAction;
            //m_animationEvents.OnAnimationDone -= HandleAnimationDone;
            //m_animationEvents.OnChop -= HandleChopAction;
        }
        base.OnNetworkDespawn();
        
        currentHealth.OnValueChanged -= OnHealthChanged;

        SceneManager.LoadScene(0);
    }
    
    [Rpc(SendTo.Server)]
    private void RequestDropServerRpc()
    {
        DropCurrentItem();
    }

    public void TransitToStunnedState(Vector3 position)
    {
        position.y = transform.position.y;
        stateMachine.Transit(grabbedState);
        controller.Move((position - transform.position));
    }
}

