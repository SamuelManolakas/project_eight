using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ChargeState_B : State_B
{
    public ChargeState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}
    
    bool charge = false;
    Vector3 chargeDirection;
    
    public override void Enter()
    {
        //reminder to change the damage logic
        boss.chargeHitBox.GetComponent<ChargeHitBox>().damage = boss.damage;
        boss.StartCoroutine(Wait());
    }

    public override void Exit()
    {
        boss.chargeHitBox.GetComponent<Collider>().isTrigger = false;
    }
    
    private IEnumerator Wait()
    {
        boss.animator.Play("Charge");
        
        chargeDirection = boss.currentTarget.transform.position - boss.transform.position;
        chargeDirection = Vector3.ClampMagnitude(chargeDirection, 1);
        chargeDirection.y = 0;
        
        yield return new WaitForSeconds(1);
        
        boss.chargeHitBox.GetComponent<Collider>().isTrigger = true;
        charge = true;
        
        yield return new WaitForSeconds(7.667f);
        
        charge = false;
        if (boss.stateMachine.currentState == boss.stunnedState)
        {
            
        }
        else
        {
            boss.stateMachine.Transit(boss.movementState);
        }
    }

    public override void ContinuousAction()
    {
        Quaternion toRotation = Quaternion.LookRotation(chargeDirection, Vector3.up);
        boss.lowerBody.transform.rotation = Quaternion.Slerp(boss.lowerBody.transform.rotation, toRotation, Time.deltaTime);
        boss.upperBody.transform.rotation = Quaternion.Slerp(boss.upperBody.transform.rotation, toRotation, Time.deltaTime * 3);
        
        if (charge)
            boss.controller.Move(chargeDirection * (Time.deltaTime * 40));
    }
}
