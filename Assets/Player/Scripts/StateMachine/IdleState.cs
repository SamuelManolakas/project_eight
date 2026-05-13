using UnityEngine;

public class IdleState : State
{
    public IdleState(PlayerBehaviour player, State parent) : base(player , parent){}

    public override void Enter()
    {
        player.animator.Play("Idle");
    }

    public override void Exit()
    {
        
    }

    public override void ContinuousAction()
    {
        if (player.moveInput.sqrMagnitude > 0.01f)
        {
            player.stateMachine.Transit(player.movementState);
        }
        else if (player.moveInput.sqrMagnitude < 0.01f)
        {
            player.speed = player.initialSpeed;
        }
    }

    public override void OnAttack()
    {
        if(player.swordAndShield || player.greatSword)
            player.stateMachine.Transit(player.attackState);
    }

    public override void OnHeavyAttack()
    {
        player.stateMachine.Transit(player.heavyAttackState);
    }

    public override void OnJump()
    {
        player.stateMachine.Transit(player.jumpState);
    }

    public override void OnDodge()
    {
        player.stateMachine.Transit(player.dodgeState);
    }
    
    public override void OnGuard()
    {
        if (player.swordAndShield)
            player.stateMachine.Transit(player.guardState);
    }
}
