using System;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class BossBehaviour : Enemy
{
    [Header("Variables")]
    public int maxHealth;
    [HideInInspector] public int currentHealth;
    public float speed;
    public float gravity;
    
    [Header("Components")]
    public Animator animator;
    public AnimationEvents m_animationEvents;
    
    [HideInInspector]
    public Rigidbody rigidbody;
    [HideInInspector]
    public StateMachine_B stateMachine = null;
    [HideInInspector]
    public CharacterController controller;
    [HideInInspector] 
    public Vector3 velocity;
    
    public RootState_B rootState = null;
    public AliveState_B aliveState = null;
    public DeadState_B deadState = null;
    public SpawnState_B spawnState = null;
    public Phase1State phase1State = null;
    public Phase2State phase2State = null;
    public MovementState_B movementState = null;
    public AttackState_B attackState = null;
    
    public void Awake(){
        rootState = new RootState_B(this, null);
        aliveState = new AliveState_B(this, rootState);
        deadState = new DeadState_B(this, rootState);
        spawnState = new SpawnState_B(this, rootState);
        phase1State = new Phase1State(this, aliveState);
        phase2State = new Phase2State(this, aliveState);
        movementState = new MovementState_B(this, aliveState);
        attackState = new AttackState_B(this, aliveState);
        
        stateMachine = new StateMachine_B();
        stateMachine.InitializeMachine(spawnState);
        
        rigidbody = GetComponent<Rigidbody>();
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private float _testTimer;
    private void Update()
    {
        ContinuousAction();
        
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
        _testTimer += Time.deltaTime;
        if (_testTimer > 10f)
        {
            _testTimer = 0f;
            SwitchTarget();
        }
    }

    private void SwitchTarget()
    {
        if (players.Count > 0)
        {
            if (currentTarget == players[0] && players.Count > 1)
            {
                currentTarget = players[1];
            }
            else
            {
                currentTarget = players[0];
            }
        }
    }
    
    private void ContinuousAction(){stateMachine.currentState.ContinuousAction();}
}