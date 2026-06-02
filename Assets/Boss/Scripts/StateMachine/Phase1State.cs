using System.Collections;
using UnityEngine;

public class Phase1State : State_B
{
    public Phase1State(BossBehaviour boss, State_B parent) : base(boss , parent){}
    
    public override void Enter()
    {
        boss.StartCoroutine(Wait());
    }

    public override void Exit()
    {
        boss.StopAllCoroutines();
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(2f);
        boss.stateMachine.Transit(boss.movementState);
    }
}