using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerBehaviour : NetworkBehaviour
{
    [Header("Weapon Choice")]
    public bool swordAndShield;
    public bool bolter;
    public bool greatSword;
    
    [Header("Movement")]
    public float speed;
    public float sprintSpeed;
    public float acceleration;
    public float jumpHeight;
    public float gravity;

    [Header("Dodge")] 
    public float dodgeDuration;
    public float dodgeDistance;
    
    [Header("Combat")]
    public int primaryAttackDamage;
    public int secondaryAttackDamage;
    public int healConsumableAmount;
    public int healAmount;
    public int maxAmmo;
    [HideInInspector] public int ammo;
    public float fireRate;
    public float primaryAttackStaminaCost;
    public float secondaryAttackStaminaCost;
    public float dodgeStaminaCost;
    public float sprintStaminaCost;
    
    [Header("Components")]
    public Animator animator;
    //[SerializeField]
    //private AnimationEvents m_animationEvents;
    [SerializeField]
    private InteractionDetector m_interactionDetector;
    public GameObject weapon;
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
    [HideInInspector]
    public PlayerHealth health;
    [HideInInspector]
    public PlayerStamina stamina;
    [HideInInspector]
    public PlayerShoot playerShoot;
    [HideInInspector]
    public ThirdPersonCamera camera;
    
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
    public PrimaryAttackState primaryAttackState = null;
    public JumpAttackState jumpAttackState = null;
    public GrabbedState grabbedState = null;
    public GuardState guardState = null;
    public SecondaryAttackState secondaryAttackState = null;
    public ReloadState reloadState = null;
    public RunAttackState runAttackState = null;

    private float _healCooldown;
    
    public void Awake(){
        rootState = new RootState(this, null);
        aliveState = new AliveState(this, rootState);
        deadState = new DeadState(this, rootState);
        spawnState = new SpawnState(this, rootState);
        idleState = new IdleState(this, aliveState);
        movementState = new MovementState(this, aliveState);
        dodgeState = new DodgeState(this, aliveState);
        jumpState = new JumpState(this, aliveState);
        primaryAttackState = new PrimaryAttackState(this, aliveState);
        jumpAttackState = new JumpAttackState(this, aliveState);
        grabbedState = new GrabbedState(this, aliveState);
        guardState = new GuardState(this, aliveState);
        secondaryAttackState = new SecondaryAttackState(this, aliveState);
        reloadState = new ReloadState(this, aliveState);
        runAttackState = new RunAttackState(this, aliveState);
        
        stateMachine = new StateMachine();
        stateMachine.InitializeMachine(spawnState);
        
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
        initialSpeed = speed;
        stamina = GetComponent<PlayerStamina>();
        health = GetComponent<PlayerHealth>();
        playerShoot = GetComponent<PlayerShoot>();
    }

    private void Start()
    {
        hitBox = weapon.GetComponent<HitBox>();
        hurtBox = GetComponent<HurtBox>();
        ammo = maxAmmo;
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        if(!IsOwner) return;
        moveInput = context.ReadValue<Vector2>();
        stateMachine.currentState.OnMove();
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if(!IsOwner) return;
        
        if (context.performed && stamina.TryUseStamina(dodgeStaminaCost))
        {
            stateMachine.currentState.OnDodge();
        }
    }
    
    public void OnPrimaryAttack(InputAction.CallbackContext context)
    {
        if(!IsOwner) return;

        if (context.performed)
        {
            if (stateMachine.currentState != primaryAttackState)
            {
                stateMachine.currentState.OnPrimaryAttack();
            }
            attackBuffer = true;
        }
    }

    public void OnSecondaryAttack(InputAction.CallbackContext context)
    {
        if(!IsOwner) return;

        if (context.performed)
        {
            if (stateMachine.currentState != secondaryAttackState)
            {
                stateMachine.currentState.OnSecondaryAttack();
            }
        }
        else if (context.canceled && stateMachine.currentState == secondaryAttackState)
        {
            if (bolter)
            {
                secondaryAttackState.ExitAim();
            }
        }
    }
    
    public void OnJump(InputAction.CallbackContext context)
    {
        if(!IsOwner) return;
        if (context.performed && controller.isGrounded)
        {
            stateMachine.currentState.OnJump();   
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if(context.performed)
            stateMachine.currentState.OnSprint();
    }

    public void OnGuard(InputAction.CallbackContext context)
    {
        if(!IsOwner) return;
        if (swordAndShield || greatSword)
        {
            if (context.performed && controller.isGrounded)
                stateMachine.currentState.OnGuard();
            else if (context.canceled && health.Health > 0)
            {
                stateMachine.Transit(idleState);
            }
        }
    }
    
    public void OnHeal(InputAction.CallbackContext context)
    {
        if(!IsOwner) return;
        if (context.performed && health.Health > 0 && healConsumableAmount > 0)
        {
            _healCooldown = 0.6f;
            health.Heal(healAmount);
            healConsumableAmount--;
        }
    }
    
    public void OnReload(InputAction.CallbackContext context)
    {
        if(!IsOwner) return;
        
        if (context.performed && bolter)
        {
            stateMachine.Transit(reloadState);
        }
    }

    private void Update()
    {
        if(!IsOwner) return;
        
        velocity.y += gravity * Time.deltaTime;
        ContinuousAction();

        if (_healCooldown >= 0)
        {
            _healCooldown -= Time.deltaTime;
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

        if (!IsOwner) return;
        
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ThirdPersonCamera>();
        camera.target = transform;
        if (bolter)
        {
            camera.isRanged = true;
        }
        stamina._stamina.OnValueChanged += OnStaminaChanged;
    }
    
    private void OnStaminaChanged(float previousValue, float newValue)
    {
        if (IsOwner)
        {
            HUDManager.Instance.SetMaxStamina(stamina.maxStamina);
            HUDManager.Instance.SetStamina(stamina._stamina.Value);
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
        weapon.SetActive(newValue == ObjectType.Axe);
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
        
        stamina._stamina.OnValueChanged -= OnStaminaChanged;

        SceneManager.LoadScene(0);
    }
    
    [Rpc(SendTo.Server)]
    private void RequestDropServerRpc()
    {
        DropCurrentItem();
    }

    [ClientRpc]
    public void TransitToDeadStateClientRpc()
    {
        stateMachine.Transit(deadState);
    }
    
    [ClientRpc]
    public void TransitToStunnedStateClientRpc(Vector3 position)
    {
        position.y = transform.position.y;
        transform.position = position;
        stateMachine.Transit(grabbedState);
    }

    public void ClearHitTargets()
    {
        hitBox.hitTargets.Clear();
    }
}

