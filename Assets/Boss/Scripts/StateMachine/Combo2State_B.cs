using System.Collections;
using UnityEngine;

public class Combo2State_B : State_B
{
    public Combo2State_B(BossBehaviour boss, State_B parent) : base(boss , parent){}
    
    bool charge = false;
    Vector3 chargeDirection;
    
    public override void Enter()
    {
        boss.hitBox.damage = boss.damage;
        boss.StartCoroutine(Combo());
    }

    public override void Exit()
    {
        boss.hitBox.damage = 0;
        boss.greatSwordModel.GetComponent<Collider>().enabled = false;
        
        boss.StopAllCoroutines();
    }
    
    private IEnumerator Combo()
    {
        boss.animator.Play("4HitCombo");
        
        //spawn small explosions from the cannon
        
        yield return new WaitForSeconds(3f);
        boss.hitBox.hitTargets.Clear();
        boss.greatSwordModel.GetComponent<Collider>().enabled = true;

        yield return new WaitForSeconds(2.2f);
        
        chargeDirection = boss.currentTarget.transform.position - boss.transform.position;
        chargeDirection = Vector3.ClampMagnitude(chargeDirection, 1);
        chargeDirection.y = 0;
        
        charge = true;
        boss.chargeHitBox.GetComponent<Collider>().isTrigger = true;
        
        yield return new WaitForSeconds(0.4f);
        
        charge = false;
        boss.chargeHitBox.GetComponent<Collider>().isTrigger = false;
        
        yield return new WaitForSeconds(3.3f);
        
        boss.Shoot();
        
        yield return new WaitForSeconds(2.1f);
        boss.stateMachine.Transit(boss.movementState);
    }

    public override void ContinuousAction()
    {
        Vector3 lookDirection = boss.currentTarget.transform.position - boss.transform.position;
        lookDirection = Vector3.ClampMagnitude(lookDirection, 1);
        lookDirection.y = 0;
        
        Quaternion toRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        boss.upperBody.transform.rotation = Quaternion.Slerp(boss.upperBody.transform.rotation, toRotation, Time.deltaTime * 5);

        if (charge)
        {
            boss.lowerBody.transform.rotation = Quaternion.Slerp(boss.lowerBody.transform.rotation, toRotation, Time.deltaTime * 5);
            boss.controller.Move(chargeDirection * (Time.deltaTime * 40));   
        }
    }
}