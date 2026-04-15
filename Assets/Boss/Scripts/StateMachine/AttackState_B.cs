using System.Collections;
using UnityEngine;

public class AttackState_B : State_B
{
    public AttackState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}

    public override void Enter()
    {
        boss.hitBox.damage = boss.damage;
        boss.stateMachine.Transit(boss.combo1State);
        //boss.StartCoroutine(Wait());
    }

    public override void Exit()
    {
        
    }
    
    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1.2f);
        boss.stateMachine.Transit(boss.movementState);
    }
}
