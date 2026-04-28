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
        boss.animator.Play("3HitCombo");
        yield return new WaitForSeconds(5.5f);
        boss.hitBox.damage = 0;
        yield return new WaitForSeconds(1f);
        boss.stateMachine.Transit(boss.movementState);
    }

    public override void ContinuousAction()
    {
        Vector3 lookDirection = boss.currentTarget.transform.position - boss.transform.position;
        lookDirection = Vector3.ClampMagnitude(lookDirection, 1);
        lookDirection.y = 0;
        
        Quaternion toRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        boss.upperBody.transform.rotation = Quaternion.Slerp(boss.upperBody.transform.rotation, toRotation, Time.deltaTime);
        
    }
}
