using UnityEngine;

public class MovementState : State
{
    public MovementState(PlayerBehaviour player, State parent) : base(player , parent){}
    
    public override void Enter()
    {
        player.animator.Play("Run");
    }

    public override void Exit()
    {
        player.animator.SetFloat("speed", 0);
    }

    public override void ContinuousAction()
    {
        Vector3 camForward = player.cameraTransform.forward;
        Vector3 camRight = player.cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 move = camForward * player.moveInput.y + camRight * player.moveInput.x;

        player.controller.Move((move * player.speed + player.velocity) * Time.deltaTime);
        player.animator.SetFloat("speed", move.magnitude);

        if (player.speed == player.sprintSpeed)
        {
            player.stamina.TryUseStamina(player.sprintStaminaCost);
        }

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(move, Vector3.up);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, toRotation, Time.deltaTime * 10f);
        }
        else
        {
            player.stateMachine.Transit(player.idleState);
        }
    }

    public override void OnPrimaryAttack()
    {
        if(player.swordAndShield || player.greatSword)
            player.stateMachine.Transit(player.primaryAttackState);
    }

    public override void OnJump()
    {
        player.stateMachine.Transit(player.jumpState);
    }
    
    public override void OnDodge()
    {
        player.stateMachine.Transit(player.dodgeState);
    }

    public override void OnSprint()
    {
        if (player.speed == player.sprintSpeed)
        {
            player.speed = player.initialSpeed;
        }
        else
        {
            player.speed = player.sprintSpeed;
        }
    }

    public override void OnGuard()
    {
        if (player.swordAndShield)
            player.stateMachine.Transit(player.guardState);
    }

    public override void OnSecondaryAttack()
    {
        player.stateMachine.Transit(player.secondaryAttackState);
    }
}
