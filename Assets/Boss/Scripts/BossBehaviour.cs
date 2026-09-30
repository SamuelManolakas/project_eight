using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
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

    [Header("Player Count Scaling")]
    [Tooltip("Values above are balanced for 1 player. Each extra player adds this fraction of the base value (0.75 = +75% per extra player).")]
    public float healthPerExtraPlayer = 0.75f;
    [Tooltip("Applies to all boss damage: melee, charge, flamers, nuke, bullets and cannon explosions.")]
    public float damagePerExtraPlayer = 0.2f;
    [Tooltip("More players deal more burst damage, so the stagger threshold rises with them.")]
    public float staggerThresholdPerExtraPlayer = 0.75f;

    // Scaled max health, synced so every client's health bar uses the same max.
    public NetworkVariable<int> scaledMaxHealth = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Components")]
    public Animator animator;
    public GameObject greatSwordModel;
    public Slider slider;
    public GameObject upperBody;
    public GameObject lowerBody;
    public Collider grabCollider;
    public Transform firePoint;
    public ChargeHitBox chargeHitBox;
    public GameObject bulletPrefab;
    [FormerlySerializedAs("canonExplosionPrefab")] // keeps the prefab's existing assignment after the rename
    public GameObject cannonExplosionPrefab;
    public GameObject flamePrefab;
    public GameObject chestFlamer;
    public Transform nukePosition;
    //Audio
    public BossAudio bossAudioScriptableObject;
    public GameObject audioSource;
    public BossAudioNetworker bossAudioNetworker;

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
    [HideInInspector] public HierarchicalStateMachine<State_B> stateMachine = null;
    [HideInInspector] public CharacterController controller;
    [HideInInspector] public Vector3 velocity;
    [HideInInspector] public HitBox hitBox;
    [HideInInspector] public HurtBox hurtBox;
    [HideInInspector] public Collider swordCollider;       // cached: attack states toggle these on every attack
    [HideInInspector] public Collider chargeHitBoxCollider;
    [HideInInspector] public BossDeathSequence deathSequence;
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

    // ── Player Count Scaling ─────────────────────────────────────────────────────
    // Inspector values captured before scaling, so scaling always starts from the 1-player balance.
    private int _baseMaxHealth, _baseDamage, _baseNukeDamage, _baseStaggerThreshold;
    private float _damageMultiplier = 1f;

    private bool _isGrounded;

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

        stateMachine = new HierarchicalStateMachine<State_B>();
        stateMachine.InitializeMachine(spawnState);

        _rigidbody = GetComponent<Rigidbody>();
        swordCollider        = greatSwordModel.GetComponent<Collider>();
        chargeHitBoxCollider = chargeHitBox.GetComponent<Collider>();
        deathSequence        = GetComponent<BossDeathSequence>();
        controller = GetComponent<CharacterController>();

        chargeHitBox.OnHitWall += TransitionToStunnedState;

        _baseMaxHealth        = maxHealth;
        _baseDamage           = damage;
        _baseNukeDamage       = nukeDamage;
        _baseStaggerThreshold = staggerThreshold;
    }

    public override void OnDestroy()
    {
        chargeHitBox.OnHitWall -= TransitionToStunnedState;

        // The engine loop lives on the BossAudio asset, which outlives this scene: stop it here
        // (runs on every machine when the boss is destroyed, e.g. leaving the game)
        if (bossAudioScriptableObject != null)
            bossAudioScriptableObject.StopEngineAudio();

        base.OnDestroy();
    }

    private void Start()
    {
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
        if (IsServer) // only the server is allowed to write currentHealth
        {
            ApplyPlayerCountScaling(NetworkManager.ConnectedClientsIds.Count);
            currentHealth.Value = maxHealth;
        }

        UpdateHealthBar();
        currentHealth.OnValueChanged += OnHealthChanged;
        scaledMaxHealth.OnValueChanged += OnMaxHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= OnHealthChanged;
        scaledMaxHealth.OnValueChanged -= OnMaxHealthChanged;
    }

    private void OnHealthChanged(int previousValue, int newValue) => UpdateHealthBar();

    private void OnMaxHealthChanged(int previousValue, int newValue) => UpdateHealthBar();

    // Sets the max first, then the value. On clients the health and max-health values can arrive in either
    // order; setting the value while the slider's max is still its default (1) clamps it, so both are
    // always re-applied together.
    private void UpdateHealthBar()
    {
        if (scaledMaxHealth.Value > 0)
            maxHealth = scaledMaxHealth.Value; // keeps maxHealth-based checks (e.g. the half-health nuke) in sync on clients too

        slider.maxValue = maxHealth;
        slider.value = currentHealth.Value;
    }

    // ── Player Count Scaling ─────────────────────────────────────────────────────

    /// <summary>
    /// Server only. Scales health, damage and stagger resistance from the 1-player inspector values.
    /// Called once when the boss spawns; everyone is already connected from the lobby by then.
    /// </summary>
    private void ApplyPlayerCountScaling(int playerCount)
    {
        int extraPlayers = Mathf.Max(0, playerCount - 1);
        _damageMultiplier = 1f + damagePerExtraPlayer * extraPlayers;

        maxHealth        = Mathf.RoundToInt(_baseMaxHealth * (1f + healthPerExtraPlayer * extraPlayers));
        damage           = ScaleDamage(_baseDamage);
        nukeDamage       = ScaleDamage(_baseNukeDamage);
        staggerThreshold = Mathf.RoundToInt(_baseStaggerThreshold * (1f + staggerThresholdPerExtraPlayer * extraPlayers));
        scaledMaxHealth.Value = maxHealth;

        Debug.Log($"Boss scaled for {playerCount} player(s): health {maxHealth}, damage {damage}, " +
                  $"nuke {nukeDamage}, stagger threshold {staggerThreshold}");
    }

    private int ScaleDamage(int baseAmount) => Mathf.RoundToInt(baseAmount * _damageMultiplier);

    private void Update()
    {
        if (!IsServer) return;
        if (currentHealth.Value <= 0) return;

        ContinuousAction();

        // Hold a small downward speed while grounded instead of letting gravity build up forever.
        // Grounding comes from this gravity move's own result: the states' horizontal Move calls
        // run first and report isGrounded = false even when the boss is standing on the floor.
        if (_isGrounded && velocity.y < 0f)
            velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        CollisionFlags flags = controller.Move(velocity * Time.deltaTime);
        _isGrounded = (flags & CollisionFlags.Below) != 0;

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

    /// <summary>Server only. Makes a player targetable again (e.g. after being revived).</summary>
    public void AddTarget(GameObject player)
    {
        if (!players.Contains(player))
            players.Add(player);
    }

    /// <summary>
    /// Server only. Stops targeting a player (downed, carried, dead). If the boss was chasing them,
    /// it switches right away instead of waiting for the next aggro evaluation.
    /// </summary>
    public void RemoveTarget(GameObject player)
    {
        players.Remove(player);

        if (currentTarget != player) return;
        _randomTauntTimer = 0f;
        GameObject next = GetTopThreatTarget();
        if (next != null) currentTarget = next; // nobody left: keep the old target so states don't hit a null
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
        // Ignore hits once dead: otherwise stagger damage could still Transit the dead boss into StunnedState
        if (currentHealth.Value <= 0) return;

        RegisterDamage(attackerNetworkObjectId, dmg);
        RegisterStaggerDamage(dmg);             // ← feeds the stagger window
        stateMachine.currentState.GetHit(dmg);
    }

    private void ContinuousAction() { stateMachine.currentState.ContinuousAction(); }

    private void TransitionToStunnedState(Collider other)
    {
        stateMachine.Transit(stunnedState);
    }

    // ── Movement helpers (used by the boss states) ──────────────────────────────

    /// <summary>The boss stops walking toward its target once it's this close.</summary>
    public const float StoppingDistance = 8f;

    /// <summary>Flat direction toward a point, length 1 (shorter when the point is closer than 1m).</summary>
    public Vector3 FlatDirectionTo(Vector3 point)
    {
        Vector3 direction = Vector3.ClampMagnitude(point - transform.position, 1f);
        direction.y = 0f;
        return direction;
    }

    /// <summary>Walks toward a point unless already within stopDistance. Returns true if it moved.</summary>
    public bool MoveToward(Vector3 point, float moveSpeed, float stopDistance)
    {
        if (Vector3.Distance(transform.position, point) <= stopDistance) return false;

        controller.Move(FlatDirectionTo(point) * (moveSpeed * Time.deltaTime));
        return true;
    }

    /// <summary>Turns the legs and torso toward a flat direction (the torso turns faster).</summary>
    public void FaceDirection(Vector3 flatDirection)
    {
        TurnToward(lowerBody.transform, flatDirection, 1f);
        TurnToward(upperBody.transform, flatDirection, 3f);
    }

    /// <summary>Smoothly turns one body part (e.g. just the torso during an attack) toward a flat direction.</summary>
    public void TurnToward(Transform part, Vector3 flatDirection, float turnRate)
    {
        if (flatDirection.sqrMagnitude <= 0.001f) return; // no direction yet: LookRotation(zero) would log a warning

        Quaternion toRotation = Quaternion.LookRotation(flatDirection, Vector3.up);
        part.rotation = Quaternion.Slerp(part.rotation, toRotation, Smoothing.Factor(turnRate));
    }

    /// <summary>Server only. Switches the engine sound for everyone (the networker's RPC also plays it on the host).</summary>
    public void SetEngineSound(int engineState) => SetEngineSound(engineState, bossChargeCrashSound);

    public void SetEngineSound(int engineState, int chargeCrash)
    {
        bossEngineSound = engineState;
        bossChargeCrashSound = chargeCrash;
        if (bossAudioScriptableObject != null)
            bossAudioNetworker.TriggerEngineAudio(bossEngineSound, bossChargeCrashSound);
    }

    public void Shoot() => RequestShootServerRpc();

    [ServerRpc]
    private void RequestShootServerRpc()
    {
        Vector3 dir = (currentTarget.transform.position - firePoint.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(dir));
        Bullet_B bulletB = bullet.GetComponent<Bullet_B>();
        bulletB.Initialize(dir);
        bulletB.damage = ScaleDamage(bulletB.damage); // prefab value is the 1-player damage
        bullet.GetComponent<NetworkObject>().Spawn();
    }
    
    public void CannonExplosion() => RequestCannonExplosionServerRpc();

    [ServerRpc]
    private void RequestCannonExplosionServerRpc()
    {
        Vector3 dir = (currentTarget.transform.position - firePoint.position).normalized;
        GameObject explosion = Instantiate(cannonExplosionPrefab, firePoint.position, Quaternion.LookRotation(dir));
        CannonExplosion_B explosionB = explosion.GetComponent<CannonExplosion_B>();
        explosionB.damage = ScaleDamage(explosionB.damage); // prefab value is the 1-player damage
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
    
    [ClientRpc]
    public void SetTelegraphVFXClientRpc(bool active)
    {
        if (telegraphVFX) telegraphVFX.SetActive(active);
    }

    [ClientRpc]
    public void SetNukeVFXClientRpc(bool active)
    {
        if (nukeVFX) nukeVFX.SetActive(active);
    }

    [ClientRpc]
    public void SetGrabExplosionVFXClientRpc(bool active)
    {
        if (grabExplosionVFX) grabExplosionVFX.SetActive(active);
    }
    
    [ClientRpc]
    public void SetDeathVFXClientRpc(bool active, int value)
    {
        if (value == 1)
        {
            frontLeftExplosion.SetActive(true);
        }
        else if (value == 2)
        {
            backRightExplosion.SetActive(true);
        }
        else if (value == 3)
        {
            flamesRightShoulder.SetActive(true);
            flamesLeftShoulder.SetActive(true);
            flamesHead.SetActive(true);
        }
    }
}