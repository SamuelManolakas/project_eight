using System.Collections;
using UnityEngine;

public class ChargeState_B : State_B
{
    public ChargeState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}
    
    bool charge = false;
    Vector3 chargeDirection;
    
    public override void Enter()
    {
        //reminder to change the damage logic
        boss.chargeHitBox.damage = boss.damage;
        StartCoroutine(Wait());
        
        //Audio
        boss.SetEngineSound(2, 0);
    }

    public override void Exit()
    {
        boss.chargeHitBoxCollider.isTrigger = false;
        boss.chargeHitBox.damage = 0;
        boss.chargeHitBox.gameObject.layer = 7;
        

        //Audio
        boss.SetEngineSound(3, 1);
        
    }
    
    private IEnumerator Wait()
    {
        boss.animator.Play("Charge");
        
        chargeDirection = boss.FlatDirectionTo(boss.currentTarget.transform.position);
        
        yield return new WaitForSeconds(1);
        
        boss.chargeHitBoxCollider.isTrigger = true;
        boss.chargeHitBox.gameObject.layer = 9;
        charge = true;
        
        yield return new WaitForSeconds(7.667f);
        
        boss.chargeHitBox.gameObject.layer = 7;
        
        charge = false;
        if (boss.stateMachine.currentState == boss.stunnedState)
        {
            // workaround for the boss to not transit to movement state if he hits a wall early
        }
        else
        {
            boss.stateMachine.Transit(boss.movementState);
        }
    }

    public override void ContinuousAction()
    {
        boss.FaceDirection(chargeDirection); // chargeDirection is zero until the charge starts; FaceDirection skips that

        if (charge)
            boss.controller.Move(chargeDirection * (Time.deltaTime * 40));
    }
}
