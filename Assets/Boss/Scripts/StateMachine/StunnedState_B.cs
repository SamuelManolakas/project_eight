using System.Collections;
using UnityEngine;

public class StunnedState_B : State_B
{
    public StunnedState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}
    
    public override void Enter()
    {
        Debug.Log("StunnedState_B Enter");
        boss.StartCoroutine(Wait());
    }

    public override void Exit()
    {
        Debug.Log("StunnedState_B Exit");
    }

    public override void ContinuousAction()
    {
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(4.5f);
        boss.stateMachine.Transit(boss.movementState);
    }
}
