using System.Collections;
using UnityEngine;

public class StunnedState_B : State_B
{
    public StunnedState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}
    
    public override void Enter()
    {
        boss.animator.Play("Stunned");
        boss.StartCoroutine(Wait());
        
        //Audio
        if (boss.bossAudioScriptableObject != null)
        {
            boss.bossAudioScriptableObject.BossStunAudioPlay(boss.audioSource);
        }
        boss.bossEngineSound = 0;
        if (boss.bossAudioScriptableObject != null)
        {
            boss.bossAudioScriptableObject.PlayEngineAudioPlay(boss.audioSource, boss.bossEngineSound, boss.bossChargeCrashSound);
        }
    }

    public override void Exit()
    {
        boss.StopAllCoroutines();
    }

    public override void ContinuousAction()
    {
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(4f);
        boss.stateMachine.Transit(boss.movementState);
    }
}
