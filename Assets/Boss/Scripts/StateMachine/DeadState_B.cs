using System.Collections;
using UnityEngine;

public class DeadState_B : State_B
{
    public DeadState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}

    public override void Enter()
    {
        boss.animator.Play("Death");
        StartCoroutine(DeathSequence());
        
        //Audio
        boss.SetEngineSound(3);
        if (boss.bossAudioScriptableObject != null)
        {
            boss.bossAudioNetworker.TriggerDieAudio();

        }
    }

    public override void Exit()
    {
        
        //Audio
        boss.bossEngineSound = 3;
        if (boss.bossAudioScriptableObject != null)
        {
            boss.bossAudioScriptableObject.PlayEngineAudioPlay(boss.audioSource, boss.bossEngineSound, boss.bossChargeCrashSound);
        }
    }
    public override void GetHit(int damage){}

    public override void ContinuousAction(){}

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(2.3f);
        boss.SetDeathVFXClientRpc(true, 1);
        
        yield return new WaitForSeconds(1.7f);
        boss.SetDeathVFXClientRpc(true, 2);
        
        yield return new WaitForSeconds(3.5f);
        boss.SetDeathVFXClientRpc(true, 3);
    }
}