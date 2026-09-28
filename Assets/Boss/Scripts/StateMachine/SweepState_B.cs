using System.Collections;
using UnityEngine;

public class SweepState_B : State_B
{
    public SweepState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}

    public override void Enter()
    {
        boss.hitBox.damage = boss.damage;
        boss.animator.Play("Sweep");
        StartCoroutine(Wait());
        boss.swordCollider.enabled = true;
        
        //Audio
        if (boss.bossAudioScriptableObject != null)
        {
            boss.bossAudioNetworker.TriggerSweepAudio();
        }
        boss.SetEngineSound(0);
    }

    public override void Exit()
    {
        boss.hitBox.damage = 0;
        boss.swordCollider.enabled = false;
        
    }
    
    public override void ContinuousAction()
    {
        if (!boss.currentTarget)
        {
            return;
        }
        Vector3 direction = boss.FlatDirectionTo(boss.currentTarget.transform.position);
        boss.TurnToward(boss.upperBody.transform, direction, 3f); // torso tracks the target during the attack
    }
    
    private IEnumerator Wait()
    {
        boss.hitBox.hitTargets.Clear();
        
        yield return new WaitForSeconds(4);
        boss.stateMachine.Transit(boss.movementState);
    }
}
