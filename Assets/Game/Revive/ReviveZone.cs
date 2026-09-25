using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Area that revives downed players left inside it. Teammates carry a downed player here and
/// drop them off (or throw them in, or a pit extraction device drops them here); while they
/// lie Downed inside the zone, revive progress fills and they get back up once it completes.
///
/// Setup: put this on a scene GameObject with a Collider that defines the area (Box, Sphere,
/// Capsule or convex Mesh). Mark the collider as a trigger so it doesn't block movement.
/// No NetworkObject needed — the logic only runs on the server.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ReviveZone : MonoBehaviour
{
    [Tooltip("Seconds a downed player must lie in the zone to be revived.")]
    [SerializeField] private float reviveDuration = 3f;

    [Tooltip("Health the player gets back when revived.")]
    [SerializeField] private int healthOnRevive = 30;

    [Tooltip("If true, progress is kept when a player is picked up and brought back. If false, it restarts.")]
    [SerializeField] private bool keepProgressWhenRemoved = false;

    private Collider _area;
    private readonly List<PlayerHealth> _revivingPlayers = new List<PlayerHealth>();

    private void Awake() => _area = GetComponent<Collider>();

    private void Update()
    {
        NetworkManager network = NetworkManager.Singleton;
        if (network == null || !network.IsServer) return;

        foreach (NetworkClient client in network.ConnectedClientsList)
        {
            if (client.PlayerObject == null) continue;
            if (!client.PlayerObject.TryGetComponent(out PlayerBehaviour player)) continue;

            PlayerHealth health = player.health;
            bool isReviving = player.stateMachine.currentState == player.downedState && IsInside(player.transform.position);

            if (isReviving)
                TickRevive(player, health);
            else if (_revivingPlayers.Remove(health) && !keepProgressWhenRemoved)
                health.ReviveProgress.Value = 0f;
        }
    }

    private void TickRevive(PlayerBehaviour player, PlayerHealth health)
    {
        if (!_revivingPlayers.Contains(health)) _revivingPlayers.Add(health);

        health.ReviveProgress.Value = Mathf.Min(1f, health.ReviveProgress.Value + Time.deltaTime / reviveDuration);
        if (health.ReviveProgress.Value < 1f) return;

        _revivingPlayers.Remove(health);
        health.Revive(healthOnRevive);
        player.ReviveClientRpc();
    }

    private bool IsInside(Vector3 position)
    {
        // ClosestPoint returns the point itself when it's inside the collider.
        return (_area.ClosestPoint(position) - position).sqrMagnitude < 0.0001f;
    }
}
