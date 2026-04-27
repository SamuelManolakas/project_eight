using System.Collections;
using UnityEngine;

public class ChargeState_B : State_B
{
    public ChargeState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}
    
    public override void Enter()
    {
        boss.hitBox.damage = boss.damage;
        boss.StartCoroutine(Wait());
    }

    public override void Exit()
    {
        boss.greatSwordModel.GetComponent<Collider>().enabled = false;
    }
    
    private IEnumerator Wait()
    {
        boss.animator.Play("Charge");
        yield return new WaitForSeconds(5.667f);
        
        boss.stateMachine.Transit(boss.movementState);
    }
}
