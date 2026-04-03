using System.Collections;
using UnityEngine;

public class SpawnState_B : State_B
{
    public SpawnState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}

    private float timer;
    public override void Enter()
    {
        Debug.Log("SpawnState Enter");
        
        timer = 2f;
    }

    public override void Exit()
    {
        Debug.Log("SpawnState Exit");
    }

    public override void ContinuousAction()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else if (timer < 0)
        {
            boss.stateMachine.Transit(boss.phase1State);
        }
    }    
}
