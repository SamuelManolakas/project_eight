using System.Collections;
using UnityEngine;

public class AttackState_B : State_B
{
    public AttackState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}

    public override void Enter()
    {
        boss.animator.SetTrigger("attack");
        boss.hitBox.damage = boss.damage;
        boss.StartCoroutine(HitBoxActivation());
    }

    public override void Exit()
    {
        boss.animator.SetFloat("speed", 0);
        boss.greatSwordModel.GetComponent<Collider>().enabled = false;
    }

    private IEnumerator HitBoxActivation()
    {
        yield return new WaitForSeconds(0.4f);
        boss.greatSwordModel.GetComponent<Collider>().enabled = true;
        boss.StartCoroutine(Wait());
    }
    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1.6f);
        boss.stateMachine.Transit(boss.movementState);
    }
}
