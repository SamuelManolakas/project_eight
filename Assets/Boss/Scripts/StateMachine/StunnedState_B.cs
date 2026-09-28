using System.Collections;
using UnityEngine;

public class StunnedState_B : State_B
{
    public StunnedState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}
    
    public override void Enter()
    {
        boss.animator.Play("Stunned");
        StartCoroutine(Wait());
        
        //Audio
        if (boss.bossAudioScriptableObject != null)
        {
            boss.bossAudioNetworker.TriggerStunAudio();

        }
        boss.SetEngineSound(0);
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
