using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Everything that happens after the boss dies, on one adjustable timeline.
/// All times are seconds after death (the Death animation is 14 seconds long).
/// Runs on the server; visuals reach clients through RPCs and the networked head.
/// </summary>
[RequireComponent(typeof(BossBehaviour))]
public class BossDeathSequence : NetworkBehaviour
{
    [Header("Explosions and fire")]
    [SerializeField] private float frontLeftExplosionTime = 2.3f;
    [SerializeField] private float backRightExplosionTime = 4.0f;
    [SerializeField] private float flamesTime = 7.5f;

    [Header("Head")]
    [Tooltip("The head on the boss model. Hidden for everyone when the head launches.")]
    [SerializeField] private GameObject head;
    [Tooltip("Networked head prefab with a BossHeadProjectile. Must be in the Network Prefabs list.")]
    [SerializeField] private BossHeadProjectile headProjectilePrefab;
    [SerializeField] private float headLaunchTime = 12f;
    [Tooltip("Where the head lands if no player is left standing (in front of the boss, in metres).")]
    [SerializeField] private float fallbackLandingDistance = 6f;

    [Header("Fallen sword")]
    [Tooltip("The sword's hitbox turns on at this time and instantly kills anyone who touches it.")]
    [SerializeField] private float swordLethalStartTime = 9f;
    [Tooltip("The sword's hitbox turns off again at this time.")]
    [SerializeField] private float swordLethalEndTime = 14f;

    private BossBehaviour _boss;
    private bool _started;

    private void Awake() => _boss = GetComponent<BossBehaviour>();

    /// <summary>Server only. Called by DeadState_B when the boss dies.</summary>
    public void Play()
    {
        if (!IsServer || _started) return;
        _started = true;

        StartCoroutine(At(frontLeftExplosionTime, () => _boss.SetDeathVFXClientRpc(true, 1)));
        StartCoroutine(At(backRightExplosionTime, () => _boss.SetDeathVFXClientRpc(true, 2)));
        StartCoroutine(At(flamesTime, () => _boss.SetDeathVFXClientRpc(true, 3)));
        StartCoroutine(At(headLaunchTime, LaunchHead));
        StartCoroutine(At(swordLethalStartTime, () => SetSwordLethal(true)));
        StartCoroutine(At(swordLethalEndTime, () => SetSwordLethal(false)));
    }

    private static IEnumerator At(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        action();
    }

    // ── Head ────────────────────────────────────────────────────────────────────

    private void LaunchHead()
    {
        Vector3 start = head != null ? head.transform.position : transform.position + Vector3.up * 5f;
        Quaternion rotation = head != null ? head.transform.rotation : transform.rotation;
        HideHeadClientRpc();

        if (headProjectilePrefab == null)
        {
            Debug.LogWarning($"{nameof(BossDeathSequence)}: no head projectile prefab assigned.", this);
            return;
        }

        GameObject target = PickRandomStandingPlayer();
        Vector3 fallback = transform.position + _boss.lowerBody.transform.forward * fallbackLandingDistance;

        BossHeadProjectile projectile = Instantiate(headProjectilePrefab, start, rotation);
        projectile.Launch(target != null ? target.transform : null, fallback);
        projectile.NetworkObject.Spawn();
    }

    // The boss's target list only holds players who are still standing (downed/carried ones are removed)
    private GameObject PickRandomStandingPlayer()
    {
        var candidates = new List<GameObject>();
        foreach (GameObject player in _boss.players)
            if (player != null) candidates.Add(player);

        return candidates.Count > 0 ? candidates[UnityEngine.Random.Range(0, candidates.Count)] : null;
    }

    [ClientRpc]
    private void HideHeadClientRpc()
    {
        if (head != null) head.SetActive(false);
    }

    // ── Sword ───────────────────────────────────────────────────────────────────

    private void SetSwordLethal(bool lethal)
    {
        _boss.hitBox.hitTargets.Clear();
        _boss.hitBox.instantKill = lethal;
        _boss.swordCollider.enabled = lethal;
    }
}
