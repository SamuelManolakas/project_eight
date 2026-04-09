using System.Collections;
using UnityEngine;

public class AttackState_B : State_B
{
    public AttackState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}

    public override void Enter()
    {
        float random = UnityEngine.Random.Range(0,3);
        if (random == 0)
        {
            boss.animator.SetTrigger("attack");
        }
        else
        {
            boss.animator.SetFloat("speed", 1);
        }
        
        boss.StartCoroutine(Wait());
    }

    public override void Exit()
    {
        boss.animator.SetFloat("speed", 0);
        base.Exit();
    }

    public override void ContinuousAction()
    {
        base.ContinuousAction();
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1.6f);
        boss.stateMachine.Transit(boss.movementState);
    }
}
