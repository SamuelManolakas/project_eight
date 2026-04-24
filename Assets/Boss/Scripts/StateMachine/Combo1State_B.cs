using System.Collections;
using UnityEngine;

public class Combo1State_B : State_B
{
    public Combo1State_B(BossBehaviour boss, State_B parent) : base(boss , parent){}
    
    public override void Enter()
    {
        boss.hitBox.damage = boss.damage;
        boss.StartCoroutine(Combo());
        boss.greatSwordModel.GetComponent<Collider>().enabled = true;
    }

    public override void Exit()
    {
        boss.greatSwordModel.GetComponent<Collider>().enabled = false;
    }
    
    private IEnumerator Combo()
    {
        boss.animator.Play("Sweep");
        yield return new WaitForSeconds(2.5f);
        boss.animator.Play("Shoot");
        yield return new WaitForSeconds(2.5f);
        boss.animator.Play("Strike");
        yield return new WaitForSeconds(2.5f);
        
        boss.stateMachine.Transit(boss.movementState);
    }
}
