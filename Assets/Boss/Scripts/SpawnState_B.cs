using System.Collections;
using UnityEngine;

public class SpawnState_B : State_B
{
    public SpawnState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}
    
    public override void Enter()
    {
        Debug.Log("SpawnState Enter");
        
        boss.StartCoroutine(Wait());
    }

    public override void Exit()
    {
        Debug.Log("SpawnState Exit");
    }
    
    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(2f);
        boss.stateMachine.Transit(boss.phase1State);
    }
}