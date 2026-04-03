using System.Collections;
using UnityEngine;

public class Phase1State : State_B
{
    public Phase1State(BossBehaviour boss, State_B parent) : base(boss , parent){}

    private float timer;
    public override void Enter()
    {
        Debug.Log("Phase1State Enter");

        timer = 2f;
    }

    public override void Exit()
    {
        Debug.Log("Phase1State Exit");
    }

    public override void ContinuousAction()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else if (timer < 0)
        {
            boss.stateMachine.Transit(boss.movementState);
        }
    }
}