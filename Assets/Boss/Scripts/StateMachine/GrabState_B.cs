using System.Collections;
using UnityEngine;

public class GrabState_B : State_B
{
    public GrabState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}

    public override void Enter()
    {
        boss.animator.Play("Grab");
        boss.StartCoroutine(Wait());
    }

    public override void Exit()
    {
        
    }
    
    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1);
        boss.grabCollider.enabled = true;
        yield return new WaitForSeconds(2);
        boss.grabCollider.enabled = false;
        yield return new WaitForSeconds(2.958f);
        boss.stateMachine.Transit(boss.movementState);
    }
}
