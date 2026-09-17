using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Server-authoritative hover device that flies to a downed player, picks them up,
/// carries them along an "upside-down L" path (straight up above the pit, then across
/// and slightly down) to a fixed drop-off point, then despawns.
///
/// Setup requirements on the prefab:
///  - NetworkObject component
///  - NetworkTransform component, set to Server Authoritative
///  - Registered in your NetworkManager's Network Prefabs list
///  - A child Transform assigned to attachPoint, marking where the player visually sits
/// </summary>
[RequireComponent(typeof(NetworkObject))]
public class PitExtractionDevice : NetworkBehaviour
{
    [Header("Timing (adjustable)")]
    [Tooltip("Time to fly from this device's spawn point to the downed player.")]
    [SerializeField] private float approachDuration = 1.5f;

    [Tooltip("Time from pickup to drop-off. Stays constant no matter how far apart the pit and drop point are.")]
    [SerializeField] private float carryDuration = 3f;

    [Header("Path shape")]
    [Tooltip("How far above the drop-off height the device rises before crossing over to it.")]
    [SerializeField] private float cruiseHeightAboveDrop = 4f;

    [Header("References")]
    [Tooltip("Local socket the player is visually attached to while carried. Falls back to this transform if left empty.")]
    [SerializeField] private Transform attachPoint;

    private Vector3 _pickupPosition;
    private Vector3 _dropPosition;
    private NetworkObject _passenger;
    private IExtractionCarryable _carryable;

    /// <summary>
    /// Call this on the server immediately after NetworkObject.Spawn(). Kicks off the whole sequence.
    /// </summary>
    public void ServerInitialize(Vector3 pickupPosition, Vector3 dropPosition, NetworkObject passenger)
    {
        if (!IsServer) return;

        _pickupPosition = pickupPosition;
        _dropPosition = dropPosition;
        _passenger = passenger;
        _carryable = passenger != null ? passenger.GetComponent<IExtractionCarryable>() : null;

        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        Transform anchor = attachPoint != null ? attachPoint : transform;

        // Phase 1 — hover down from this device's spawn point to the downed player.
        yield return MoveAlongWaypoints(new[] { transform.position, _pickupPosition }, approachDuration, null);

        // Pickup.
        _carryable?.OnExtractionPickup(NetworkObject, anchor);

        // Phase 2 — "upside-down L": straight up above the pit, then across (and slightly
        // down, since cruise height is only a little above the drop point) to the destination.
        float cruiseY = Mathf.Max(_pickupPosition.y, _dropPosition.y + cruiseHeightAboveDrop);
        Vector3 corner = new Vector3(_pickupPosition.x, cruiseY, _pickupPosition.z);
        Vector3[] carryPath = { _pickupPosition, corner, _dropPosition };

        yield return MoveAlongWaypoints(carryPath, carryDuration, () => _carryable?.OnExtractionCarryUpdate(anchor.position));

        // Drop-off.
        _carryable?.OnExtractionDropoff(_dropPosition);

        // Only the server may despawn a NetworkObject.
        NetworkObject.Despawn();
    }

    /// <summary>
    /// Moves this transform along a polyline in exactly `duration` seconds, regardless of the
    /// polyline's total length — speed scales with distance so total travel time stays fixed.
    /// </summary>
    private IEnumerator MoveAlongWaypoints(Vector3[] waypoints, float duration, System.Action onStep)
    {
        int segmentCount = waypoints.Length - 1;
        var segmentLengths = new float[segmentCount];
        float totalLength = 0f;

        for (int i = 0; i < segmentCount; i++)
        {
            segmentLengths[i] = Vector3.Distance(waypoints[i], waypoints[i + 1]);
            totalLength += segmentLengths[i];
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float traveled = Mathf.Clamp01(elapsed / duration) * totalLength;

            Vector3 pos = waypoints[waypoints.Length - 1];
            float accumulated = 0f;

            for (int i = 0; i < segmentCount; i++)
            {
                bool isLastSegment = i == segmentCount - 1;
                if (traveled <= accumulated + segmentLengths[i] || isLastSegment)
                {
                    float segT = segmentLengths[i] > 0f ? (traveled - accumulated) / segmentLengths[i] : 1f;
                    pos = Vector3.Lerp(waypoints[i], waypoints[i + 1], Mathf.Clamp01(segT));
                    break;
                }
                accumulated += segmentLengths[i];
            }

            transform.position = pos;
            onStep?.Invoke();
            yield return null;
        }

        transform.position = waypoints[waypoints.Length - 1];
        onStep?.Invoke();
    }
}