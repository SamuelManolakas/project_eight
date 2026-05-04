using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ShootState_B : State_B
{
    public ShootState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}

    public override void Enter()
    {
        boss.animator.Play("Shoot");
        boss.StartCoroutine(Wait());
    }

    public override void Exit()
    {
    }
    
    public override void ContinuousAction()
    {
        if (!boss.currentTarget)
        {
            return;
        }
        Vector3 direction = boss.currentTarget.transform.position - boss.transform.position;
        direction = Vector3.ClampMagnitude(direction, 1);
        direction.y = 0;
        
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
            boss.upperBody.transform.rotation = Quaternion.Slerp(boss.upperBody.transform.rotation, toRotation, Time.deltaTime * 3);
        }
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1.5f);
        boss.Shoot();
        yield return new WaitForSeconds(2.5f);
        boss.stateMachine.Transit(boss.movementState);
    }
}
