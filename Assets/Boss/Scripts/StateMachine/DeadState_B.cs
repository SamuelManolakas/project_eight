using System.Collections;
using UnityEngine;

public class DeadState_B : State_B
{
    public DeadState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}

    public override void Enter()
    {
        boss.animator.Play("Death");
        boss.StartCoroutine(DeathSequence());
        
        //Audio
        boss.bossEngineSound = 3;
        if (boss.bossAudioScriptableObject != null)
        {
            boss.bossAudioScriptableObject.PlayEngineAudioPlay(boss.audioSource, boss.bossEngineSound, boss.bossChargeCrashSound);
        }
        if (boss.bossAudioScriptableObject != null)
        {
            boss.bossAudioScriptableObject.BossDieAudioPlay(boss.audioSource);
        }
    }

    public override void Exit()
    {
        boss.StopAllCoroutines();
        
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
        boss.frontLeftExplosion.SetActive(true);
        
        yield return new WaitForSeconds(1.7f);
        boss.backRightExplosion.SetActive(true);
        
        yield return new WaitForSeconds(3.5f);
        boss.flamesRightShoulder.SetActive(true);
        boss.flamesLeftShoulder.SetActive(true);
        boss.flamesHead.SetActive(true);
    }
}