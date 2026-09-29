using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// The boss's head after it's launched on death. Flies straight up, then arcs down onto its target,
/// following them until <see cref="lockOffBeforeLanding"/> seconds before it lands so they can escape.
/// Players within <see cref="killRadius"/> of the landing point are killed (downed).
///
/// Server-driven: put it on a prefab with a NetworkObject and a server-authoritative NetworkTransform
/// (sync position and rotation) so every client sees the same flight.
/// </summary>
public class BossHeadProjectile : NetworkBehaviour
{
    [Header("Flight")]
    [Tooltip("How high above its start the head rises before arcing toward the target.")]
    [SerializeField] private float riseHeight = 15f;
    [SerializeField] private float riseDuration = 1.2f;
    [Tooltip("Time from the top of the rise to landing.")]
    [SerializeField] private float fallDuration = 2.5f;
    [Tooltip("Stops following the target this many seconds before landing, so they can get away.")]
    [SerializeField] private float lockOffBeforeLanding = 1f;
    [Tooltip("Tumble while flying (degrees per second per axis).")]
    [SerializeField] private Vector3 spinDegreesPerSecond = new Vector3(360f, 120f, 0f);

    [Header("Impact")]
    [SerializeField] private float killRadius = 2.5f;
    [Tooltip("The head stops when it hits these layers. Left empty, it uses the \"Floor\" layer.")]
    [SerializeField] private LayerMask floorLayers;
    [Tooltip("Distance from the head's pivot to its underside, so it rests on the floor instead of sinking in.")]
    [SerializeField] private float groundOffset = 0.5f;
    [SerializeField] private LayerMask playerLayers = ~0;
    [Tooltip("Seconds the head stays on the ground before despawning. Negative = it stays.")]
    [SerializeField] private float despawnAfterLanding = -1f;
    [Tooltip("Safety net: if it falls this far below where it started without touching the floor, it lands there.")]
    [SerializeField] private float maxFallBelowStart = 50f;

    private Transform _target;
    private Vector3 _landingPoint;
    private Vector3 _start;
    private Vector3 _apex;
    private float _timer;
    private bool _landed;

    private void Awake()
    {
        if (floorLayers == 0)
            floorLayers = LayerMask.GetMask("Floor");
    }

    /// <summary>
    /// Server, before Spawn. With no target (nobody left standing) it lands on fallbackLandingPoint.
    /// </summary>
    public void Launch(Transform target, Vector3 fallbackLandingPoint)
    {
        _target = target;
        _landingPoint = target != null ? target.position : fallbackLandingPoint;
        _start = transform.position;
        _apex = _start + Vector3.up * riseHeight;
        _timer = 0f;
        _landed = false;
    }

    private void Update()
    {
        if (!IsServer || !IsSpawned || _landed) return;

        _timer += Time.deltaTime;
        transform.Rotate(spinDegreesPerSecond * Time.deltaTime, Space.Self);

        // Phase 1: straight up, slowing to a stop at the top
        if (_timer < riseDuration)
        {
            float u = _timer / riseDuration;
            transform.position = Vector3.Lerp(_start, _apex, 1f - (1f - u) * (1f - u));
            return;
        }

        // Phase 2: arc down toward the target, following them until shortly before impact
        float fallTime = _timer - riseDuration;
        if (_target != null && fallTime < fallDuration - lockOffBeforeLanding)
            _landingPoint = _target.position;

        Vector3 previous = transform.position;
        Vector3 next;
        if (fallTime < fallDuration)
        {
            float s = fallTime / fallDuration;
            Vector3 landing = _landingPoint + Vector3.up * groundOffset;
            next = Vector3.Lerp(_apex, landing, s);            // horizontal: steady
            next.y = Mathf.Lerp(_apex.y, landing.y, s * s);    // vertical: accelerates like a fall
        }
        else
        {
            // Reached the planned landing without touching the floor: keep falling straight down
            // at the speed the arc ended with until it does.
            float endSpeed = 2f * Mathf.Max(1f, _apex.y - _landingPoint.y) / fallDuration;
            next = previous + Vector3.down * (endSpeed * Time.deltaTime);
        }

        // Stop on the floor: check the path moved this frame (plus the head's own size below its pivot)
        Vector3 step = next - previous;
        float distance = step.magnitude;
        if (distance > 0f &&
            Physics.Raycast(previous, step / distance, out RaycastHit hit, distance + groundOffset, floorLayers, QueryTriggerInteraction.Ignore))
        {
            transform.position = hit.point + hit.normal * groundOffset;
            Land(hit.point);
            return;
        }

        transform.position = next;

        if (next.y < _start.y - maxFallBelowStart)
            Land(next); // never found a floor; land anyway rather than falling forever
    }

    private void Land(Vector3 impactPoint)
    {
        _landed = true;
        _landingPoint = impactPoint;

        // Kill everyone near the impact (once each, even if they have several colliders)
        Collider[] hits = Physics.OverlapSphere(impactPoint, killRadius, playerLayers, QueryTriggerInteraction.Collide);
        var killed = new HashSet<PlayerHealth>();
        foreach (Collider hit in hits)
        {
            PlayerHealth health = hit.GetComponentInParent<PlayerHealth>();
            if (health != null && killed.Add(health))
                health.Kill();
        }

        if (despawnAfterLanding >= 0f)
            StartCoroutine(DespawnAfter(despawnAfterLanding));
    }

    private IEnumerator DespawnAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn(true);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Application.isPlaying ? _landingPoint : transform.position, killRadius);
    }
}
