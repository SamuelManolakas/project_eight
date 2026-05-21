using System.Collections;
using UnityEngine;

public class StrikeState_B : State_B
{
    public StrikeState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}

    public override void Enter()
    {
        boss.hitBox.damage = boss.damage;
        boss.animator.Play("Strike");
        boss.StartCoroutine(Wait());
        boss.greatSwordModel.GetComponent<Collider>().enabled = true;
    }

    public override void Exit()
    {
        boss.hitBox.damage = 0;
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
        boss.hitBox.hitTargets.Clear();
        
        yield return new WaitForSeconds(3.5f);
        boss.stateMachine.Transit(boss.movementState);
    }
}
