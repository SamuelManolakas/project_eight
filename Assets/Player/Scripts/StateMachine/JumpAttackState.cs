using System.Collections;
using UnityEngine;

public class JumpAttackState : State
{
    public JumpAttackState(PlayerBehaviour player, State parent) : base(player, parent){}

    private Vector3 _jumpDirection;
    public override void Enter()
    {
        player.animator.Play("Combo1a");
        player.stamina.TryUseStamina(player.primaryAttackStaminaCost);
        player.hitBox.damage = player.primaryAttackDamage;
        player.weapon.GetComponent<Collider>().enabled = true;
        _jumpDirection = player.controller.velocity;
    }

    public override void Exit()
    {
        player.weapon.GetComponent<Collider>().enabled = false;
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
