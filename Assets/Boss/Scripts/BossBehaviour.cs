using System;
using System.Collections.Generic;
using System.Linq;
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
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public float speed;
    public int damage;
    public float gravity;
    public float attackCooldown;

    [Header("Aggro Settings")]
    [Tooltip("How often (seconds) the boss considers a random target switch.")]
    public float aggroEvaluationInterval = 8f;
    [Tooltip("0–1 chance per interval that the boss switches away from the top damage dealer.")]
    [Range(0f, 1f)]
    public float randomSwitchChance = 0.25f;
    [Tooltip("How long (seconds) a random taunt target is held before re-evaluating.")]
    public float randomTauntDuration = 4f;

    [Header("Stagger Settings")]
    [Tooltip("How much damage must be dealt within the window to trigger a stagger.")]
    public int staggerThreshold = 150;
    [Tooltip("The rolling time window (seconds) in which burst damage is measured.")]
    public float staggerWindow = 3f;
    [Tooltip("Minimum time (seconds) between staggers, so it can't chain-stagger infinitely.")]
    public float staggerCooldown = 6f;

    [Header("Components")]
    public Animator animator;
    public AnimationEvents m_animationEvents;
    public GameObject greatSwordModel;
    public Slider slider;
    public GameObject upperBody;
    public GameObject lowerBody;
    public Collider grabCollider;
    public Transform firePoint;
    public ChargeHitBox chargeHitBox;
    public GameObject bulletPrefab;
    public GameObject canonExplosionPrefab;
    public GameObject flamePrefab;
    public GameObject chestFlamer;
    public Transform nukePosition;
    //Audio
    public BossAudio bossAudioScriptableObject;
    public GameObject audioSource;

    [Header("Nuke Attack")]
    public float nukeRadius;
    public int nukeDamage;
    public LayerMask targetLayerMask;
    public LayerMask occlusionLayerMask;

    [Header("VFX")]
    public GameObject telegraphVFX;
    public GameObject nukeVFX;
    public GameObject grabExplosionVFX;
    
    [Header("Death VFX")]
    public GameObject frontLeftExplosion;
    public GameObject backRightExplosion;
    public GameObject flamesLeftShoulder;
    public GameObject flamesRightShoulder;
    public GameObject flamesHead;

    [HideInInspector] public Rigidbody _rigidbody;
    [HideInInspector] public StateMachine_B stateMachine = null;
    [HideInInspector] public CharacterController controller;
    [HideInInspector] public Vector3 velocity;
    [HideInInspector] public HitBox hitBox;
    [HideInInspector] public HurtBox hurtBox;
    //Audio
    [HideInInspector] public int bossEngineSound;
    [HideInInspector] public int bossChargeCrashSound;

    public RootState_B rootState        = null;
    public AliveState_B aliveState      = null;
    public DeadState_B deadState        = null;
    public SpawnState_B spawnState      = null;
    public Phase1State phase1State      = null;
    public Phase2State phase2State      = null;
    public MovementState_B movementState   = null;
    public Combo1State_B combo1State    = null;
    public Combo2State_B combo2State    = null;
    public GrabState_B grabState        = null;
    public ChargeState_B chargeState    = null;
    public StunnedState_B stunnedState  = null;
    public ShootState_B shootState      = null;
    public StrikeState_B strikeState    = null;
    public SweepState_B sweepState      = null;
    public ChestFlamerState_B chestFlamerState = null;
    public SpinAttackState_B SpinAttackState   = null;
    public NukeState_B NukeState               = null;

    // ── Aggro System ─────────────────────────────────────────────────────────────
    private Dictionary<ulong, int> _damageTotals = new Dictionary<ulong, int>();
    private float _randomTauntTimer  = 0f;
    private float _aggroEvalTimer    = 0f;

    // ── Stagger System ───────────────────────────────────────────────────────────
    // Each entry is the timestamp of a hit and how much damage it dealt.
    // Old entries are expired as they fall outside the rolling window.
    private struct DamageEvent { public float time; public int amount; }
    private Queue<DamageEvent> _staggerDamageWindow = new Queue<DamageEvent>();
    private int   _windowDamageAccumulator = 0;    // Running sum of unexpired events
    private float _staggerCooldownTimer    = 0f;   // > 0 while on cooldown

    public void Awake()
    {
        rootState        = new RootState_B(this, null);
        aliveState       = new AliveState_B(this, rootState);
        deadState        = new DeadState_B(this, rootState);
        spawnState       = new SpawnState_B(this, rootState);
        phase1State      = new Phase1State(this, aliveState);
        phase2State      = new Phase2State(this, aliveState);
        movementState    = new MovementState_B(this, aliveState);
        combo1State      = new Combo1State_B(this, aliveState);
        combo2State      = new Combo2State_B(this, aliveState);
        grabState        = new GrabState_B(this, aliveState);
        chargeState      = new ChargeState_B(this, aliveState);
        stunnedState     = new StunnedState_B(this, aliveState);
        shootState       = new ShootState_B(this, aliveState);
        strikeState      = new StrikeState_B(this, aliveState);
        sweepState       = new SweepState_B(this, aliveState);
        chestFlamerState = new ChestFlamerState_B(this, aliveState);
        SpinAttackState  = new SpinAttackState_B(this, aliveState);
        NukeState        = new NukeState_B(this, aliveState);

        stateMachine = new StateMachine_B();
        stateMachine.InitializeMachine(spawnState);

        _rigidbody = GetComponent<Rigidbody>();
        controller = GetComponent<CharacterController>();

        chargeHitBox.OnHitWall += TransitionToStunnedState;
    }

    public override void OnDestroy()
    {
        chargeHitBox.OnHitWall -= TransitionToStunnedState;
    }

    private void Start()
    {
        currentHealth.Value = maxHealth;
        hitBox  = greatSwordModel.GetComponent<HitBox>();
        hurtBox = GetComponent<HurtBox>();
        
        //Audio
        bossEngineSound = 0;
        if (bossAudioScriptableObject != null)
        {
            bossAudioScriptableObject.PlayEngineAudioPlay(audioSource, bossEngineSound, bossChargeCrashSound);
        }
        
    }

    public override void OnNetworkSpawn()
    {
        slider.maxValue = maxHealth;
        slider.value    = maxHealth;
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

    private void Update()
    {
        if (!IsServer) return;
        if (currentHealth.Value <= 0) return;

        ContinuousAction();

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        TickAggroSystem(Time.deltaTime);
        TickStaggerSystem(Time.deltaTime);
    }

    // ── Aggro System ─────────────────────────────────────────────────────────────

    private void RegisterDamage(ulong attackerNetworkObjectId, int dmg)
    {
        if (!_damageTotals.ContainsKey(attackerNetworkObjectId))
            _damageTotals[attackerNetworkObjectId] = 0;

        _damageTotals[attackerNetworkObjectId] += dmg;
    }

    private GameObject GetTopThreatTarget()
    {
        if (players.Count == 0) return null;

        var validPlayers = players.Where(p => p != null).ToList();
        if (validPlayers.Count == 0) return null;

        if (_damageTotals.Count == 0)
            return GetClosestPlayer(validPlayers);

        GameObject topTarget = null;
        int topDamage = -1;

        foreach (var player in validPlayers)
        {
            var netObj = player.GetComponent<NetworkObject>();
            if (netObj == null) continue;

            _damageTotals.TryGetValue(netObj.NetworkObjectId, out int dealt);
            if (dealt > topDamage)
            {
                topDamage = dealt;
                topTarget = player;
            }
        }

        return topTarget != null ? topTarget : GetClosestPlayer(validPlayers);
    }

    private GameObject GetRandomNonTopTarget(GameObject topTarget)
    {
        var alternatives = players
            .Where(p => p != null && p != topTarget)
            .ToList();

        if (alternatives.Count == 0) return null;
        return alternatives[UnityEngine.Random.Range(0, alternatives.Count)];
    }

    private GameObject GetClosestPlayer(List<GameObject> validPlayers)
    {
        return validPlayers
            .OrderBy(p => Vector3.Distance(transform.position, p.transform.position))
            .FirstOrDefault();
    }

    private void TickAggroSystem(float deltaTime)
    {
        if (players.Count == 0) return;

        if (_randomTauntTimer > 0f)
        {
            _randomTauntTimer -= deltaTime;
            if (_randomTauntTimer <= 0f)
                currentTarget = GetTopThreatTarget();
            return;
        }

        _aggroEvalTimer -= deltaTime;
        if (_aggroEvalTimer > 0f) return;

        _aggroEvalTimer = aggroEvaluationInterval;
        GameObject topTarget = GetTopThreatTarget();

        if (players.Count > 1 && UnityEngine.Random.value < randomSwitchChance)
        {
            GameObject randomTarget = GetRandomNonTopTarget(topTarget);
            if (randomTarget != null)
            {
                currentTarget     = randomTarget;
                _randomTauntTimer = randomTauntDuration;
                return;
            }
        }

        currentTarget = topTarget;
    }

    // ── Stagger System ───────────────────────────────────────────────────────────

    /// <summary>
    /// Records an incoming hit into the rolling damage window, then checks
    /// whether accumulated burst damage exceeds the stagger threshold.
    /// </summary>
    private void RegisterStaggerDamage(int dmg)
    {
        // Add the new event.
        _staggerDamageWindow.Enqueue(new DamageEvent { time = Time.time, amount = dmg });
        _windowDamageAccumulator += dmg;

        // Expire events that have fallen outside the rolling window.
        ExpireOldStaggerEvents();

        // Only stagger if not already on cooldown and threshold is exceeded.
        if (_staggerCooldownTimer > 0f) return;
        if (_windowDamageAccumulator >= staggerThreshold)
            TriggerStagger();
    }

    /// <summary>
    /// Removes damage events older than staggerWindow and subtracts them
    /// from the accumulator, keeping the sum accurate without iterating everything.
    /// </summary>
    private void ExpireOldStaggerEvents()
    {
        float expiryTime = Time.time - staggerWindow;
        while (_staggerDamageWindow.Count > 0 && _staggerDamageWindow.Peek().time <= expiryTime)
        {
            _windowDamageAccumulator -= _staggerDamageWindow.Dequeue().amount;
        }
    }

    private void TriggerStagger()
    {
        // Reset the window so back-to-back hits don't immediately re-stagger.
        _staggerDamageWindow.Clear();
        _windowDamageAccumulator = 0;
        _staggerCooldownTimer    = staggerCooldown;

        stateMachine.Transit(stunnedState);
    }

    /// <summary>
    /// Ticked every frame to expire old damage events passively and drain
    /// the cooldown timer, even when no hits are incoming.
    /// </summary>
    private void TickStaggerSystem(float deltaTime)
    {
        if (_staggerCooldownTimer > 0f)
            _staggerCooldownTimer -= deltaTime;

        // Keep the window clean even between hits.
        if (_staggerDamageWindow.Count > 0)
            ExpireOldStaggerEvents();
    }

    // ─────────────────────────────────────────────────────────────────────────────

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void GetHitRpc(int dmg, ulong attackerNetworkObjectId)
    {
        RegisterDamage(attackerNetworkObjectId, dmg);
        RegisterStaggerDamage(dmg);             // ← feeds the stagger window
        stateMachine.currentState.GetHit(dmg);
    }

    private void ContinuousAction() { stateMachine.currentState.ContinuousAction(); }

    private void TransitionToStunnedState(Collider other)
    {
        stateMachine.Transit(stunnedState);
    }

    public void Shoot() => RequestShootServerRpc();

    [ServerRpc]
    private void RequestShootServerRpc()
    {
        Vector3 dir = (currentTarget.transform.position - firePoint.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(dir));
        bullet.GetComponent<Bullet_B>().Initialize(dir);
        bullet.GetComponent<NetworkObject>().Spawn();
    }
    
    public void CanonExplosions() => RequestCanonExplosionsServerRpc();

    [ServerRpc]
    private void RequestCanonExplosionsServerRpc()
    {
        Vector3 dir = (currentTarget.transform.position - firePoint.position).normalized;
        GameObject explosion = Instantiate(canonExplosionPrefab, firePoint.position, Quaternion.LookRotation(dir));
        explosion.GetComponent<NetworkObject>().Spawn();
    }

    public void Flamer(Vector3 direction, float timer) => RequestFlamerServerRpc(direction, timer);

    [ServerRpc]
    private void RequestFlamerServerRpc(Vector3 directionToPlayer, float timer)
    {
        GameObject bullet = Instantiate(flamePrefab, chestFlamer.transform.position,
            Quaternion.LookRotation(chestFlamer.transform.forward));
        bullet.GetComponent<Flame_B>().Initialize(directionToPlayer, timer);
        bullet.GetComponent<Flame_B>().damage = damage;
        bullet.GetComponent<NetworkObject>().Spawn();
    }

    public void HandFlamer(float timer) => RequestHandFlamerServerRpc(timer);

    [ServerRpc]
    private void RequestHandFlamerServerRpc(float timer)
    {
        GameObject bullet = Instantiate(flamePrefab, firePoint.position,
            Quaternion.LookRotation(firePoint.transform.forward));
        bullet.GetComponent<Flame_B>().Initialize(firePoint.transform.forward, timer);
        bullet.GetComponent<Flame_B>().damage = damage;
        bullet.GetComponent<NetworkObject>().Spawn();
    }
}