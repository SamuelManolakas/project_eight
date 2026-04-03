using UnityEngine;

public class MovementState_B : State_B
{
    public MovementState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}

    public override void Enter()
    {
        Debug.Log("MovementState Enter");
    }

    public override void Exit()
    {
        Debug.Log("MovementState Exit");
    }
    public override void ContinuousAction()
    {
        Vector3 move = boss.player.transform.position - boss.transform.position;
        move = Vector3.ClampMagnitude(move, 1);

        Vector3 finalMove = move * boss.speed;
        
        if (Vector2.Distance(boss.transform.position, boss.player.transform.position) > 2f)
        {
            boss.controller.Move(finalMove * Time.deltaTime);
            boss.animator.SetFloat("speed", move.magnitude);
        }
        else
        {
            boss.animator.SetFloat("speed", 0);
        }
        
        

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(move, Vector3.up);
            boss.transform.rotation = Quaternion.Slerp(boss.transform.rotation, toRotation, Time.deltaTime * 10f);
        }
    }
}
