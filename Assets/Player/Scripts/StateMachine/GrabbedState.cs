using System.Collections;
using UnityEngine;

public class GrabbedState : State
{
    public GrabbedState(PlayerBehaviour player, State parent) : base(player , parent){}

    bool grabbed = false;
    public override void Enter()
    {
        player.animator.Play("LayingDown");
        player.StartCoroutine(Wait());
        grabbed = false;
    }

    public override void Exit()
    {
        grabbed = false;
    }

    public override void ContinuousAction()
    {
        
    }

    public override void OnMove()
    {
        if (grabbed == true)
            player.stateMachine.Transit(player.movementState);
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(2.5f);
        //Temporary damage solution
        player.GetHit(10);
        //knock back from the explosion
        
        grabbed = true;
    }
}
