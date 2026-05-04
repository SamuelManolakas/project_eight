using System.Collections;
using UnityEngine;

public class SweepState_B : State_B
{
    public SweepState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}

    public override void Enter()
    {
        boss.animator.Play("Sweep");
        boss.StartCoroutine(Wait());
        boss.greatSwordModel.GetComponent<Collider>().enabled = true;
    }

    public override void Exit()
    {
        boss.greatSwordModel.GetComponent<Collider>().enabled = false;
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
        yield return new WaitForSeconds(4f);
        boss.stateMachine.Transit(boss.movementState);
    }
}
