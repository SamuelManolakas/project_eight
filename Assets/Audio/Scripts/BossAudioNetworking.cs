using UnityEngine;
using Unity.Netcode; // Change this to Mirror or Photon if you are using those!

public class BossAudioNetworker : NetworkBehaviour
{
    [Header("1. The Audio Data")]
    public BossAudio bossAudioSO;

    [Header("2. The Local Body Parts (Drag in from Hierarchy)")]
    public GameObject bossRootObject;
    public GameObject feetObject;
    public GameObject punchObject;
    public GameObject rangedObject;


    // ==========================================
    // MULTI-PARAMETER SOUND (Engine)
    // ==========================================

    public void TriggerEngineAudio(int state, int crash)
    {
        if (IsServer) EngineAudioClientRpc(state, crash);
    }

    [ClientRpc]
    private void EngineAudioClientRpc(int state, int crash)
    {
        bossAudioSO.PlayEngineAudioPlay(feetObject, state, crash);
    }

    // ==========================================
    // MELEE ATTACKS (Punch Object)
    // ==========================================

    public void TriggerSweepAudio()
    {
        if (IsServer) SweepAudioClientRpc();
    }
    [ClientRpc]
    private void SweepAudioClientRpc() { bossAudioSO.BossSweepAudioPlay(punchObject); }


    public void TriggerStrikeAudio()
    {
        if (IsServer) StrikeAudioClientRpc();
    }
    [ClientRpc]
    private void StrikeAudioClientRpc() { bossAudioSO.BossStrikeAudioPlay(punchObject); }


    public void TriggerSpinAudio()
    {
        if (IsServer) SpinAudioClientRpc();
    }
    [ClientRpc]
    private void SpinAudioClientRpc() { bossAudioSO.BossSpinAudioPlay(punchObject); }


    public void TriggerGrabAudio()
    {
        if (IsServer) GrabAudioClientRpc();
    }
    [ClientRpc]
    private void GrabAudioClientRpc() { bossAudioSO.BossGrabAudioPlay(punchObject); }


    public void Trigger4HcAudio()
    {
        if (IsServer) Boss4HcAudioClientRpc();
    }
    [ClientRpc]
    private void Boss4HcAudioClientRpc() { bossAudioSO.Boss4HcAudioPlay(punchObject); }


    public void Trigger3HcAudio()
    {
        if (IsServer) Boss3HcAudioClientRpc();
    }
    [ClientRpc]
    private void Boss3HcAudioClientRpc() { bossAudioSO.Boss3HcAudioPlay(punchObject); }


    // ==========================================
    // RANGED ATTACKS & EFFECTS (Ranged Object)
    // ==========================================

    public void TriggerRangedAudio()
    {
        if (IsServer) RangedAudioClientRpc();
    }
    [ClientRpc]
    private void RangedAudioClientRpc() { bossAudioSO.BossRangedAudioPlay(rangedObject); }


    public void TriggerChestAudio()
    {
        if (IsServer) ChestAudioClientRpc();
    }
    [ClientRpc]
    private void ChestAudioClientRpc() { bossAudioSO.BossChestAudioPlay(rangedObject); }


    public void TriggerNukeAudio()
    {
        if (IsServer) NukeAudioClientRpc();
    }
    [ClientRpc]
    private void NukeAudioClientRpc() { bossAudioSO.BossNukeAudioPlay(rangedObject); }


    public void TriggerStunAudio()
    {
        if (IsServer) StunAudioClientRpc();
    }
    [ClientRpc]
    private void StunAudioClientRpc() { bossAudioSO.BossStunAudioPlay(rangedObject); }


    // ==========================================
    // GLOBAL BOSS EVENTS (Root Object)
    // ==========================================

    public void TriggerDamageAudio()
    {
        if (IsServer) DamageAudioClientRpc();
    }
    [ClientRpc]
    private void DamageAudioClientRpc() { bossAudioSO.BossDamageAudioPlay(bossRootObject); }


    public void TriggerDieAudio()
    {
        if (IsServer) DieAudioClientRpc();
    }
    [ClientRpc]
    private void DieAudioClientRpc() { bossAudioSO.BossDieAudioPlay(bossRootObject); }
}
