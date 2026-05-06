using System;
using Cinemachine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BossBehaviour : Enemy
{
    [Header("Variables")]
    public int maxHealth;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(0, 
        NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    public float speed;
    public int damage;
    public float gravity;
    
    [Header("Components")]
    public Animator animator;
    public AnimationEvents m_animationEvents;
    public GameObject greatSwordModel;
    public Slider slider;
    public GameObject upperBody;
    public GameObject lowerBody;
    public Collider grabCollider;
    public ChargeHitBox chargeHitBox;
    public GameObject bullet;
    
    [HideInInspector]
    public Rigidbody rigidbody;
    [HideInInspector]
    public StateMachine_B stateMachine = null;
    [HideInInspector]
    public CharacterController controller;
    [HideInInspector] 
    public Vector3 velocity;
    [HideInInspector] 
    public HitBox hitBox;
    [HideInInspector] 
    public HurtBox hurtBox;
    
    public RootState_B rootState = null;
    public AliveState_B aliveState = null;
    public DeadState_B deadState = null;
    public SpawnState_B spawnState = null;
    public Phase1State phase1State = null;
    public Phase2State phase2State = null;
    public MovementState_B movementState = null;
    public Combo1State_B combo1State = null;
    public GrabState_B grabState = null;
    public ChargeState_B chargeState = null;
    public StunnedState_B stunnedState = null;
    public ShootState_B shootState = null;
    public StrikeState_B strikeState = null;
    public SweepState_B sweepState = null;
    
    public void Awake(){
        rootState = new RootState_B(this, null);
        aliveState = new AliveState_B(this, rootState);
        deadState = new DeadState_B(this, rootState);
        spawnState = new SpawnState_B(this, rootState);
        phase1State = new Phase1State(this, aliveState);
        phase2State = new Phase2State(this, aliveState);
        movementState = new MovementState_B(this, aliveState);
        combo1State = new Combo1State_B(this, aliveState);
        grabState = new GrabState_B(this, aliveState);
        chargeState = new ChargeState_B(this, aliveState);
        stunnedState = new StunnedState_B(this, aliveState);
        shootState = new ShootState_B(this, aliveState);
        strikeState = new StrikeState_B(this, aliveState);
        sweepState = new SweepState_B(this, aliveState);
        
        stateMachine = new StateMachine_B();
        stateMachine.InitializeMachine(spawnState);
        
        rigidbody = GetComponent<Rigidbody>();
        controller = GetComponent<CharacterController>();
        
        chargeHitBox.OnHitWall += TransitionToStunnedState;
    }

    private void OnDestroy()
    {
        chargeHitBox.OnHitWall -= TransitionToStunnedState;
    }

    private void Start()
    {
        currentHealth.Value = maxHealth;
        
        hitBox = greatSwordModel.GetComponent<HitBox>();
        hurtBox = GetComponent<HurtBox>();
    }

    public override void OnNetworkSpawn()
    {
        slider.maxValue = maxHealth;
        slider.value = maxHealth;

        
        currentHealth.OnValueChanged += OnHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        slider.value = newValue;
    }

    private float _testTimer;
    private void Update()
    {
        if (!IsServer) return;
        
        ContinuousAction();
        
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
        // switching targets test
        _testTimer += Time.deltaTime;
        if (_testTimer > 10f)
        {
            _testTimer = 0f;
            SwitchTarget();
        }
    }
    
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void GetHitRpc(int damage)
    {
        stateMachine.currentState.GetHit(damage);
    }

    private void SwitchTarget()
    {
        Vector3 closestTarget = Vector3.zero;
        
        if (players.Count > 0)
        {
            foreach (var player in players)
            {
                Vector3.Distance(transform.position, player.transform.position);
                if (Vector3.Distance(transform.position, player.transform.position) > closestTarget.magnitude)
                {
                    currentTarget = player;
                }
            }
        }
    }
    
    private void ContinuousAction(){stateMachine.currentState.ContinuousAction();}

    private void TransitionToStunnedState(Collider other)
    {
        stateMachine.Transit(stunnedState);
    }

    public void Shoot()
    {
        bullet.GetComponent<Bullet_B>().direction = currentTarget.transform.position - grabCollider.transform.position;
        Instantiate(bullet, grabCollider.transform.position, grabCollider.transform.rotation);
    }
}