using System.Collections;
using UnityEngine;

public class GrabbedState : State
{
    public GrabbedState(PlayerBehaviour player, State parent) : base(player , parent){}

    public override void Enter()
    {
        //Play grabbed/stunned animation here
        player.StartCoroutine(Wait());
    }
    public override void Exit(){}

    public override void ContinuousAction()
    {
        
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(2.5f);
        //knock back from the explosion
        player.stateMachine.Transit(player.idleState);
    }
}
