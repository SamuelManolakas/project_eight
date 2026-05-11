using System.Collections;
using UnityEngine;

public class JumpAttackState : State
{
    public JumpAttackState(PlayerBehaviour player, State parent) : base(player, parent){}

    private Vector3 _jumpDirection;
    public override void Enter()
    {
        player.animator.Play("Combo1a");
        _jumpDirection = player.controller.velocity;
    }

    public override void Exit()
    {
        
    }

    public override void ContinuousAction()
    {
        if (!player.controller.isGrounded)
        {
            player.controller.Move((_jumpDirection.normalized * player.speed + player.velocity) * Time.deltaTime);
        }
        else
        {
            player.stateMachine.Transit(player.movementState);
        }
    }
}
