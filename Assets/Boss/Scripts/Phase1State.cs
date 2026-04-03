using System.Collections;
using UnityEngine;

public class Phase1State : State_B
{
    public Phase1State(BossBehaviour boss, State_B parent) : base(boss , parent){}

    private float timer;
    public override void Enter()
    {
        Debug.Log("Phase1State Enter");

        boss.StartCoroutine(Wait());
    }

    public override void Exit()
    {
        Debug.Log("Phase1State Exit");
    }

    internal IEnumerator Wait()
    {
        yield return new WaitForSeconds(2f);
        boss.stateMachine.Transit(boss.movementState);
    }
    public override void ContinuousAction()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else if (timer < 0)
        {
            
        }
    }
}