using UnityEngine;

public class IdleState : State
{
    public IdleState(PlayerBehaviour player, State parent) : base(player , parent){}

    public override void Enter()
    {
      
    }

    public override void Exit()
    {
        
    }

    public override void ContinuousAction()
    {
        if (player.moveInput.sqrMagnitude > 0.001f)
        {
            player.stateMachine.Transit(player.movementState);
        }
    }

    public override void OnAttack()
    {
        player.stateMachine.Transit(player.attackState);
    }

    public override void OnJump()
    {
        player.stateMachine.Transit(player.jumpState);
    }

    public override void OnDodge()
    {
        player.stateMachine.Transit(player.dodgeState);
    }
}
