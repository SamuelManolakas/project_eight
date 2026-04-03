using UnityEngine;

public class IdleState : State
{
    public IdleState(PlayerBehaviour player, State parent) : base(player , parent){}

    public override void Enter()
    {
        Debug.Log("Entered IdleState");
    }

    public override void Exit()
    {
        Debug.Log("Exit IdleState");
    }

    public override void ContinuousAction()
    {
        if (player.moveInput.sqrMagnitude > 0.001f)
        {
            player.stateMachine.Transit(player.movementState);
        }
    }
}
