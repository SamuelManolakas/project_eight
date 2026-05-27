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
        player.animator.SetLayerWeight(1, 0);
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
        
        move = Vector3.Lerp(move, move * player.speed, player.acceleration * Time.deltaTime);

        

        if (player.speed == player.sprintSpeed)
        {
            player.stamina.TryUseStamina(player.sprintStaminaCost);
        }
        
        if (move.sqrMagnitude > 0.001f)
        {
            if (player.camera._isLockedOn && player.speed != player.sprintSpeed)
            {
                Vector3 bossDirection = player.camera.lockOnTarget.position - player.transform.position;
                
                Quaternion toRotation = Quaternion.LookRotation(bossDirection.normalized, Vector3.up);
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation, toRotation, Time.deltaTime * 10f);
                
                player.animator.SetFloat("speed", 0);
                player.animator.SetLayerWeight(1, move.magnitude);
                player.animator.SetFloat("strafeX", player.moveInput.x);
                player.animator.SetFloat("strafeZ", player.moveInput.y);
            }
            else
            {
                Quaternion toRotation = Quaternion.LookRotation(move, Vector3.up);
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation, toRotation, Time.deltaTime * 10f);
                
                player.animator.SetFloat("speed", move.magnitude);
                
                player.animator.SetLayerWeight(1, 0);
            }
        }
        else if(player.controller.velocity.magnitude < 0.001f)
        {
            player.stateMachine.Transit(player.idleState);
        }
        
        if (move.sqrMagnitude < 0.01f)
        {
            move = Vector3.Lerp(
                move,
                Vector3.zero,
                player.acceleration * Time.deltaTime // ~5-8, slower than accel
            );
        }
        
        player.controller.Move((move * player.speed + player.velocity) * Time.deltaTime);
    }

    public override void OnPrimaryAttack()
    {
        if (player.swordAndShield || player.greatSword)
        {
            if (player.speed ==  player.sprintSpeed)
                player.stateMachine.Transit(player.runAttackState);
            else
            {
                player.stateMachine.Transit(player.primaryAttackState);
            }
        }
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
