using System;
using Unity.Netcode;
using UnityEngine;

public class BossBehaviour : NetworkBehaviour
{
    [Header("Variables")]
    public int maxHealth;
    [HideInInspector] public int currentHealth;
    public float speed;
    
    [Header("Components")]
    public Animator animator;
    public AnimationEvents m_animationEvents;
    
    [HideInInspector]
    public Rigidbody rigidbody;
    [HideInInspector]
    public PlayerBehaviour player;
    [HideInInspector]
    public StateMachine_B stateMachine = null;
    [HideInInspector]
    public CharacterController controller;
    
    public RootState_B rootState = null;
    public AliveState_B aliveState = null;
    public DeadState_B deadState = null;
    public SpawnState_B spawnState = null;
    public Phase1State phase1State = null;
    public Phase2State phase2State = null;
    public MovementState_B movementState = null;
    
    public void Awake(){
        rootState = new RootState_B(this, null);
        aliveState = new AliveState_B(this, rootState);
        deadState = new DeadState_B(this, rootState);
        spawnState = new SpawnState_B(this, rootState);
        phase1State = new Phase1State(this, aliveState);
        phase2State = new Phase2State(this, aliveState);
        movementState = new MovementState_B(this, aliveState);
        
        stateMachine = new StateMachine_B();
        stateMachine.InitializeMachine(spawnState);
        
        rigidbody = GetComponent<Rigidbody>();
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        player = GameObject.FindWithTag("Player").GetComponent<PlayerBehaviour>();
    }
    
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    private void Update()
    {
        ContinuousAction();
    }
    
    private void ContinuousAction(){stateMachine.currentState.ContinuousAction();}
}