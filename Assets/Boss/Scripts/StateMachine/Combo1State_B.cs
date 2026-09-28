using System.Collections;
using UnityEngine;

public class Combo1State_B : State_B
{
    public Combo1State_B(BossBehaviour boss, State_B parent) : base(boss , parent){}
    
    public override void Enter()
    {
        boss.hitBox.damage = boss.damage;
        StartCoroutine(Combo());
        
        //Audio
        if (boss.bossAudioScriptableObject != null)
        {
            boss.bossAudioNetworker.Trigger3HcAudio();
        }
        boss.SetEngineSound(0);
    }

    public override void Exit()
    {
        boss.hitBox.damage = 0;
        boss.swordCollider.enabled = false;
        
    }
    
    private IEnumerator Combo()
    {
        boss.hitBox.hitTargets.Clear();
        boss.animator.Play("3HitCombo");
        
        yield return new WaitForSeconds(1f);
        boss.swordCollider.enabled = true;

        yield return new WaitForSeconds(1.5f);
        boss.hitBox.hitTargets.Clear();
        
        yield return new WaitForSeconds(1.3f);
        boss.hitBox.hitTargets.Clear();
        
        yield return new WaitForSeconds(2.7f);
        boss.stateMachine.Transit(boss.movementState);
    }

    public override void ContinuousAction()
    {
        Vector3 lookDirection = boss.FlatDirectionTo(boss.currentTarget.transform.position);
        boss.TurnToward(boss.upperBody.transform, lookDirection, 1f); // torso tracks the target during the combo
    }
}
