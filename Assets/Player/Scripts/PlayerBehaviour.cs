using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerBehaviour : NetworkBehaviour, IExtractionCarryable
{
    [Header("Weapon Choice")]
    public bool swordAndShield;
    public bool bolter;
    public bool greatSword;
    
    [Header("Movement")]
    public float speed;
    public float sprintSpeed;
    public float acceleration;
    public float deceleration;
    public float jumpHeight;
    public float gravity;

    [Header("Dodge")] 
    public float dodgeDuration;
    public float dodgeDistance;
    
    [Header("Damage")]
    public int primaryAttackDamage;
    public int secondaryAttackDamage;
    public float fireRate;
    
    [Header("Stamina")]
    public float primaryAttackStaminaCost;
    public float secondaryAttackStaminaCost;
    public float dodgeStaminaCost;
    public float sprintStaminaCost;

    [Header("Heal and Ammo")] 
    public int maxHealConsumableAmount;
    private int _healConsumableAmount;
    public int healConsumableAmount
    {
        get => _healConsumableAmount;
        set
        {
            _healConsumableAmount = Mathf.Clamp(value, 0, maxHealConsumableAmount);
            OnHealConsumableChanged?.Invoke(_healConsumableAmount, maxHealConsumableAmount);
        }
    }
    public event System.Action<int, int> OnHealConsumableChanged;
    
    public int healAmount;
    public int maxAmmo;
    private int _ammo;
    public int ammo
    {
        get => _ammo;
        set
        {
            _ammo = Mathf.Clamp(value, 0, maxAmmo);
            OnAmmoChanged?.Invoke(_ammo, maxAmmo);
        }
    }
    
    public event System.Action<int, int> OnAmmoChanged;
    
    [Header("Carry")]
    public Transform carryPoint;              // empty child on the carrier's rig, e.g. over the shoulder
    public float carryDetectionRange = 2.5f;
    public LayerMask playerLayerMask;
    public float carrySpeedMultiplier = 0.6f;
    public float throwForce = 6f;
    public float throwUpwardForce = 3f;

    [HideInInspector] public PlayerBehaviour carriedPlayer;   // set on the carrier
    [HideInInspector] public PlayerBehaviour carrierPlayer;   // set on the carried player

    public CarryState carryState = null;
    public CarriedState carriedState = null;

    [Header("Sound")] 
    public int isMoving;
    
    [Header("Components")]
    public Animator animator;
    public GameObject weapon;
    public GameObject shield;
    public GameObject hudPrefab;
    public AvatarAudio PlayerAudioScriptableObject;
    public GameObject audioSource;
    public NetworkTransform networkTransform;
    
    [HideInInspector] 
    public Transform cameraTransform;
    [HideInInspector] 
    public CharacterController controller;
    [HideInInspector] 
    public Vector2 moveInput;
    [HideInInspector]
    public Vector3 velocity; // only .y is used — vertical speed, owned by ApplyMovement
    [HideInInspector]
    public Vector3 horizontalVelocity; // set by the current state each frame
    [HideInInspector]
    public float gravityScale = 1f;
    [HideInInspector] 
    public float initialSpeed;
    [HideInInspector] 
    public HitBox hitBox;
    [HideInInspector] 
    public HitBox shieldHitBox;
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
    public DownedState downedState = null;
    public ThrownState thrownState = null;
    

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
        downedState = new DownedState(this, aliveState);
        carryState = new CarryState(this, aliveState);
        carriedState = new CarriedState(this, aliveState);
        thrownState = new ThrownState(this, aliveState);
        
        stateMachine = new StateMachine();
        stateMachine.InitializeMachine(spawnState);
        
        controller = GetComponent<CharacterController>();
        if (Camera.main != null) // NEW — guard, in case no active MainCamera exists yet at this instant
            cameraTransform = Camera.main.transform;
        initialSpeed = speed;
        stamina = GetComponent<PlayerStamina>();
        health = GetComponent<PlayerHealth>();
        playerShoot = GetComponent<PlayerShoot>();
    }

    private void Start()
    {
        if (weapon == null)
        {
            Debug.LogError("Weapon is not assigned in the Inspector!", this);
            return;
        }

        if (swordAndShield || greatSword)
        {
            hitBox = weapon.GetComponent<HitBox>();
            
            if (hitBox == null)
            {
                Debug.LogError("No HitBox component found on weapon: " + weapon.name, this);
                return;
            }
        }

        NetworkObject networkObject = GetComponent<NetworkObject>();
        if (networkObject == null)
        {
            Debug.LogError("No NetworkObject component found on player!", this);
            return;
        }

        if (!bolter)
        {
            hitBox.attackerNetworkObjectId = networkObject.NetworkObjectId;
        }
        
        if (shield)
        {
            shieldHitBox = shield.GetComponent<HitBox>();
            if (shieldHitBox == null)
                Debug.LogWarning("No HitBox component found on shield: " + shield.name, this);
        }

        hurtBox = GetComponent<HurtBox>();
        if (hurtBox == null)
            Debug.LogError("No HurtBox component found on player!", this);
    }
    
    public void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log("Pressing Interact!");
        if (!IsOwner) return;
        
        if (!context.performed) return;

        if (stateMachine.currentState == carryState)
        {
            RequestDropCarriedPlayerServerRpc();
            return;
        }

        if (health.Health <= 0) return;
        //if (stateMachine.currentState != idleState && stateMachine.currentState != movementState) return;

        PlayerBehaviour target = FindCarryTarget();
        if (target == null) return;

        RequestPickUpTeammateServerRpc(target.NetworkObjectId);
    }

    public void OnThrow(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (!context.performed) return;
        if (stateMachine.currentState != carryState) return;

        RequestThrowCarriedPlayerServerRpc();
    }

    private PlayerBehaviour FindCarryTarget()
    {
        Vector3 origin = transform.position + Vector3.up;
        Collider[] hits = Physics.OverlapSphere(origin, carryDetectionRange, playerLayerMask);

        PlayerBehaviour best = null;
        float bestDot = 0.5f; // roughly a 60° cone in front of you

        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent(out PlayerBehaviour candidate)) continue;
            if (candidate == this) continue;
            if (candidate.stateMachine.currentState != candidate.downedState) continue;

            Vector3 toCandidate = (candidate.transform.position - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, toCandidate);
            if (dot > bestDot)
            {
                bestDot = dot;
                best = candidate;
            }
        }

        return best;
    }
    
    [Rpc(SendTo.Server)]
    private void RequestPickUpTeammateServerRpc(ulong targetNetworkObjectId)
    {
        if (stateMachine.currentState == carryState) return;
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out NetworkObject targetNetObj))
            return;
        if (!targetNetObj.TryGetComponent(out PlayerBehaviour target)) return;
        if (target == this) return;
        if (target.stateMachine.currentState != target.downedState)
        {
            Debug.LogWarning($"Carry: {target.name} isn't downed on the server (server sees: {target.stateMachine.currentState}).");
            return;
        }
        if (target.carrierPlayer != null) return; // already being carried
    
        float maxDist = carryDetectionRange * 1.5f; // generous margin for latency
        if ((target.transform.position - transform.position).sqrMagnitude > maxDist * maxDist) return;
    
        //targetNetObj.TrySetParent(carryPoint != null ? carryPoint : transform, false);
        
        bool parented = targetNetObj.TrySetParent(NetworkObject, false); // this carrier's own NetworkObject, not carryPoint
        if (!parented)
        {
            Debug.LogWarning($"Carry: failed to parent {target.name} under {name}.");
            return;
        }

        NotifyPickUpClientRpc(NetworkObjectId, targetNetworkObjectId);
    }
    
    public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject)
    {
        if (parentNetworkObject == null) return; // being un-parented (thrown/dropped/dropped-off) — nothing to offset

        Transform socket;
        if (parentNetworkObject.TryGetComponent(out PlayerBehaviour carrier))
        {
            socket = carrier.carryPoint;
        }
        else if (parentNetworkObject.TryGetComponent(out PitExtractionDevice extractor))
        {
            socket = extractor.AttachPoint;
        }
        else
        {
            return;
        }

        if (socket == null) return;

        transform.localPosition = parentNetworkObject.transform.InverseTransformPoint(socket.position);
        transform.localRotation = Quaternion.Inverse(parentNetworkObject.transform.rotation) * socket.rotation;
    }
    
    [ClientRpc]
    public void TransitToCarriedStateClientRpc()
    {
        stateMachine.Transit(carriedState);
    }

    // IExtractionCarryable — called by PitExtractionDevice, server-only
    public void OnExtractionPickup(NetworkObject device)
    {
        if (!IsServer) return;

        bool parented = NetworkObject.TrySetParent(device, false);
        if (!parented)
        {
            Debug.LogWarning($"Extraction: failed to parent {name} under {device.name}.");
            return;
        }

        TransitToCarriedStateClientRpc();
    }

    public void OnExtractionDropoff()
    {
        if (!IsServer) return;

        NetworkObject.TrySetParent((Transform)null, true); // leave them exactly where the ride left them
        TransitToDownedStateClientRpc(); // already exists — reused as-is, same as a normal pit-down
    }
    
    [ClientRpc]
    private void NotifyPickUpClientRpc(ulong carrierId, ulong carriedId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(carrierId, out NetworkObject carrierObj)) return;
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(carriedId, out NetworkObject carriedObj)) return;
    
        PlayerBehaviour carrier = carrierObj.GetComponent<PlayerBehaviour>();
        PlayerBehaviour carried = carriedObj.GetComponent<PlayerBehaviour>();
    
        carrier.carriedPlayer = carried;
        carried.carrierPlayer = carrier;
    
        carrier.stateMachine.Transit(carrier.carryState);
        carried.stateMachine.Transit(carried.carriedState);
    }
    
    [Rpc(SendTo.Server)]
    private void RequestThrowCarriedPlayerServerRpc()
    {
        if (stateMachine.currentState != carryState || carriedPlayer == null) return;
    
        PlayerBehaviour target = carriedPlayer;
        target.GetComponent<NetworkObject>().TrySetParent((Transform)null, true);
    
        Vector3 throwVelocity = transform.forward * throwForce + Vector3.up * throwUpwardForce;
        NotifyThrowClientRpc(NetworkObjectId, target.NetworkObjectId, throwVelocity);
    }
    
    [ClientRpc]
    private void NotifyThrowClientRpc(ulong carrierId, ulong carriedId, Vector3 throwVelocity)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(carrierId, out NetworkObject carrierObj)) return;
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(carriedId, out NetworkObject carriedObj)) return;

        PlayerBehaviour carrier = carrierObj.GetComponent<PlayerBehaviour>();
        PlayerBehaviour carried = carriedObj.GetComponent<PlayerBehaviour>();
    
        carried.velocity = new Vector3(0f, throwVelocity.y, 0f);
        carried.horizontalVelocity = new Vector3(throwVelocity.x, 0f, throwVelocity.z);

        carrier.stateMachine.Transit(carrier.idleState);
        carried.stateMachine.Transit(carried.thrownState);
    }
    
    [Rpc(SendTo.Server)]
    public void NotifyThrowLandedServerRpc()
    {
        if (stateMachine.currentState != thrownState) return; // ignore stray/duplicate calls
        TransitToDownedStateClientRpc();
    }
    
    [Rpc(SendTo.Server)]
    private void RequestDropCarriedPlayerServerRpc()
    {
        if (stateMachine.currentState != carryState || carriedPlayer == null) return;
    
        PlayerBehaviour target = carriedPlayer;
        target.GetComponent<NetworkObject>().TrySetParent((Transform)null, true);
    
        NotifyDropClientRpc(NetworkObjectId, target.NetworkObjectId);
    }
    
    [ClientRpc]
    private void NotifyDropClientRpc(ulong carrierId, ulong carriedId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(carrierId, out NetworkObject carrierObj)) return;
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(carriedId, out NetworkObject carriedObj)) return;
    
        carrierObj.GetComponent<PlayerBehaviour>().stateMachine.Transit(carrierObj.GetComponent<PlayerBehaviour>().idleState);
        carriedObj.GetComponent<PlayerBehaviour>().stateMachine.Transit(carriedObj.GetComponent<PlayerBehaviour>().downedState);
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
        
        if (context.performed && stamina._stamina.Value >= dodgeStaminaCost)
        {
            stateMachine.currentState.OnDodge();
        }
    }
    
    public void OnPrimaryAttack(InputAction.CallbackContext context)
    {
        if(!IsOwner) return;

        if (context.performed && stamina._stamina.Value >= primaryAttackStaminaCost)
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

        if (context.performed && stamina._stamina.Value >= secondaryAttackStaminaCost)
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
            else if (context.canceled && health.Health > 0 && stateMachine.currentState == guardState)
            {
                stateMachine.Transit(idleState);
            }
        }
    }
    
    public void OnHeal(InputAction.CallbackContext context)
    {
        if(!IsOwner) return;
        if(health.Health <= 0) return;
        
        if (context.performed && health.Health > 0 && healConsumableAmount > 0)
        {
            _healCooldown = 0.6f;
            health.Heal(healAmount);
            healConsumableAmount--;
            
            //Audio
            if (PlayerAudioScriptableObject != null)
            {
                PlayerAudioScriptableObject.PlayHealAudioPlay(audioSource);
            }
        }
    }
    
    public void OnReload(InputAction.CallbackContext context)
    {
        if(!IsOwner) return;
        if(health.Health <= 0) return;
        
        if (context.performed && bolter && stateMachine.currentState != grabbedState)
        {
            stateMachine.Transit(reloadState);
        }
    }

    private void Update()
    {
        if(!IsOwner) return;
        
        ContinuousAction();
        ApplyMovement();

        if (_healCooldown >= 0)
        {
            _healCooldown -= Time.deltaTime;
        }

        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            camera.target = transform;
        }
    }

    private void ContinuousAction(){stateMachine.currentState.ContinuousAction();}

    // The only place the player's CharacterController is moved. States set horizontalVelocity
    // (and velocity.y for jumps); gravity and grounding are handled here.
    private void ApplyMovement()
    {
        if (!controller.enabled) return; // e.g. CarriedState — position comes from the carrier

        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f; // small constant push keeps isGrounded stable on slopes/steps
        velocity.y += gravity * gravityScale * Time.deltaTime;

        Vector3 motion = horizontalVelocity;
        motion.y = velocity.y;
        controller.Move(motion * Time.deltaTime);
    }

    // Default for states that don't drive movement: horizontal speed bleeds off smoothly.
    public void BleedHorizontalVelocity()
    {
        horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, deceleration * Time.deltaTime);
        if (horizontalVelocity.sqrMagnitude < 0.0001f) horizontalVelocity = Vector3.zero;
    }

    // Move input mapped onto the camera's flattened forward/right. Magnitude is clamped to 1.
    public Vector3 GetCameraRelativeInput()
    {
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 move = camForward * moveInput.y + camRight * moveInput.x;
        return Vector3.ClampMagnitude(move, 1f);
    }
    
    public void GetHit(int damage)
    {
        if(!IsServer) return;
        
        stateMachine.currentState.GetHit(damage);
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        m_heldObjectType.OnValueChanged += HandleHeldItemChanged;
        HandleItemOnJoin();
        if (IsOwner)
        {
            Canvas sceneCanvas = FindObjectOfType<Canvas>(); // or a cached reference if you have one
            Instantiate(hudPrefab, sceneCanvas.transform, false);
        }

        if (!IsOwner) return;

        ammo = maxAmmo;
        var ammoUI = FindObjectOfType<AmmoUI>();
        ammoUI?.Bind(this); // internally hides itself if this player isn't bolter class
        
        healConsumableAmount = maxHealConsumableAmount;
        var healUI = FindObjectOfType<HealUI>();
        healUI?.Bind(this);
        
        StartCoroutine(AssignCameraWhenReady()); // replaces the direct camera assignment

        stamina._stamina.OnValueChanged += OnStaminaChanged;

        //GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().m_players.Add(gameObject);
    }

    private System.Collections.IEnumerator AssignCameraWhenReady()
    {
        ThirdPersonCamera foundCamera = null;
        float timeout = 5f;
        float elapsed = 0f;

        while (foundCamera == null && elapsed < timeout)
        {
            foundCamera = FindObjectOfType<ThirdPersonCamera>(true); // 'true' = include inactive objects, since GameplayCamera starts inactive
            if (foundCamera == null)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        if (foundCamera == null)
        {
            Debug.LogError("Could not find any ThirdPersonCamera in the scene after waiting.");
            yield break;
        }

        foundCamera.gameObject.SetActive(true); // NEW — turn on the real gameplay camera
        camera = foundCamera;
        camera.target = transform;
        cameraTransform = foundCamera.transform; // NEW — refresh the cached reference from Awake()
        if (bolter)
            camera.isRanged = true;

        WaitingCameraMarker waitingCam = FindObjectOfType<WaitingCameraMarker>(true);
        if (waitingCam != null)
            waitingCam.gameObject.SetActive(false); // NEW — turn off the spectator camera
    }
    
    private void OnStaminaChanged(float previousValue, float newValue)
    {
        if (IsOwner)
        {
            HUDManager.Instance.SetMaxStamina(stamina.maxStamina);
            HUDManager.Instance.SetStamina(stamina._stamina.Value);
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

        //GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().m_players.Remove(gameObject);
        //GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().SceneReload();
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
        if(health.Health <= 0) return;
        
        position.y = transform.position.y;
        transform.position = position;
        stateMachine.Transit(grabbedState);
    }

    [ClientRpc]
    public void TransitToDownedStateClientRpc()
    {
        stateMachine.Transit(downedState);
    }

    [ClientRpc]
    public void ReviveClientRpc()
    {
        stateMachine.Transit(idleState);
        // health is already set server-side before this fires
    }

    public void ClearHitTargets()
    {
        if (hitBox)
        {
            hitBox.hitTargets.Clear();
        }

        if (shield)
        {
            shieldHitBox.hitTargets.Clear();
        }
    }

    public void AddPlayerFromBossList()
    {
        if (!IsOwner) return;
        GameObject.FindGameObjectWithTag("Enemy")
            .GetComponent<BossBehaviour>().players.Add(gameObject);
    }
    
    public void RemovePlayerFromBossList()
    {
        if (!IsOwner) return;
        GameObject.FindGameObjectWithTag("Enemy")
            .GetComponent<BossBehaviour>().players.Remove(gameObject);
    }
}

