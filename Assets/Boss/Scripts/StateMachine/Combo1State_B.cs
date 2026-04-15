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
        boss.animator.speed = 1;
        boss.animator.SetTrigger("Combo1a");
        yield return new WaitForSeconds(1.6f);
        boss.animator.speed = 0.5f;
        boss.animator.Play("Combo1b");
        yield return new WaitForSeconds(1.6f);
        boss.animator.speed = 3;
        boss.animator.Play("Combo1c");
        //yield return new WaitForSeconds(1.6f);
        
        boss.stateMachine.Transit(boss.movementState);
    }
}
