using UnityEngine;

public class MovementState : State
{
    public MovementState(PlayerBehaviour player, State parent) : base(player , parent){}
    
    public override void Enter()
    {
        player.animator.Play("Run");

        player.isMoving = 1;
        
        if (player.PlayerAudioScriptableObject != null)
        {
            player.PlayerAudioScriptableObject.PlayFootstepAudioPlay(player.audioSource, player.isMoving);
        }
    }

    public override void Exit()
    {
        player.animator.SetFloat("speed", 0);
        player.animator.SetLayerWeight(1, 0);
        player.horizontalVelocity = Vector3.zero;
        
        player.isMoving = 0;
        
        if (player.PlayerAudioScriptableObject != null)
        {
            player.PlayerAudioScriptableObject.PlayFootstepAudioPlay(player.audioSource, player.isMoving);
        }
        
        player.StopAllCoroutines();
    }

    public override void ContinuousAction()
    {
        Vector3 camForward = player.cameraTransform.forward;
        Vector3 camRight = player.cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        //camForward.Normalize();
        //camRight.Normalize();
        //Vector3 move = camForward * player.moveInput.y + camRight * player.moveInput.x;
        //
        //move = Vector3.Lerp(move, move * player.speed, player.acceleration * Time.deltaTime);
        //
        if (player.speed == player.sprintSpeed)
        {
            player.stamina.TryUseStamina(player.sprintStaminaCost);

            if (player.stamina._stamina.Value <= 2)
            {
                player.speed = player.initialSpeed;
            }
        }
        
        // Target velocity this frame based on input
        Vector3 targetVelocity = (camForward * player.moveInput.y + camRight * player.moveInput.x);
        if (targetVelocity.magnitude > 1f) targetVelocity.Normalize();
        targetVelocity *= player.speed;

        // Lerp the STORED velocity toward the target — this is what gives you smooth accel/decel
        float lerpRate = targetVelocity.sqrMagnitude > 0.001f
            ? player.acceleration
            : player.deceleration; // slower bleed-off rate for the "skid" feel

        player.horizontalVelocity = Vector3.Lerp(
            player.horizontalVelocity,
            targetVelocity,
            lerpRate * Time.deltaTime
        );
        
        if (player.horizontalVelocity.sqrMagnitude > 0.001f)
        {
            if (player.camera._isLockedOn && player.speed != player.sprintSpeed)
            {
                Vector3 bossDirection = player.camera.lockOnTarget.position - player.transform.position;
                bossDirection.y = 0;
                
                Quaternion toRotation = Quaternion.LookRotation(bossDirection.normalized, Vector3.up);
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation, toRotation, Time.deltaTime * 10f);
                
                player.animator.SetFloat("speed", 0);
                player.animator.SetLayerWeight(1, player.horizontalVelocity.magnitude / player.speed);
                player.animator.SetFloat("strafeX", player.moveInput.x);
                player.animator.SetFloat("strafeZ", player.moveInput.y);
            }
            else
            {
                Quaternion toRotation = Quaternion.LookRotation(player.horizontalVelocity.normalized, Vector3.up);
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation, toRotation, Time.deltaTime * 10f);
                
                player.animator.SetFloat("speed", player.horizontalVelocity.magnitude / player.speed);
                
                player.animator.SetLayerWeight(1, 0);
            }
        }
        else
        {
            player.stateMachine.Transit(player.idleState);
        }
        
        player.controller.Move((player.horizontalVelocity + player.velocity) * Time.deltaTime);
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
        if (player.swordAndShield || player.greatSword)
            player.stateMachine.Transit(player.guardState);
    }

    public override void OnSecondaryAttack()
    {
        player.stateMachine.Transit(player.secondaryAttackState);
    }
}
