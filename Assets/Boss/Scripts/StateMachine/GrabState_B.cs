using System.Collections;
using UnityEngine;

public class GrabState_B : State_B
{
    public GrabState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}

    private bool _shouldMove;
    public override void Enter()
    {
        _shouldMove = true;
        boss.animator.Play("Grab");
        boss.StartCoroutine(Wait());
    }

    public override void Exit()
    {
        boss.StopAllCoroutines();
    }
    
    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1);
        boss.grabCollider.enabled = true;
        yield return new WaitForSeconds(2);
        _shouldMove = false;
        boss.grabCollider.enabled = false;
        yield return new WaitForSeconds(2.958f);
        boss.stateMachine.Transit(boss.movementState);
    }
    
    public override void ContinuousAction()
    {
        if (!boss.currentTarget)
        {
            return;
        }
        if (!_shouldMove) return;
        
        Vector3 move = boss.currentTarget.transform.position - boss.transform.position;
        move = Vector3.ClampMagnitude(move, 1);
        move.y = 0;
        
        Vector3 finalMove = move * boss.speed;

        if (Vector3.Distance(boss.transform.position, boss.currentTarget.transform.position) > 8f)
        {
            boss.controller.Move(finalMove * (Time.deltaTime * 3));
        }

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(move, Vector3.up);
            boss.lowerBody.transform.rotation = Quaternion.Slerp(boss.lowerBody.transform.rotation, toRotation, Time.deltaTime);
            boss.upperBody.transform.rotation = Quaternion.Slerp(boss.upperBody.transform.rotation, toRotation, Time.deltaTime * 3);
        }
    }
}
