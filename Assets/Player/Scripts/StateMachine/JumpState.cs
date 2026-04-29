using System.Collections;
using UnityEngine;

public class JumpState : State
{
    public JumpState(PlayerBehaviour player, State parent) : base(player, parent){}

    public override void Enter()
    {
        player.animator.Play("Jump");
        player.velocity.y = Mathf.Sqrt(player.jumpHeight * -2f * player.gravity);
    }
    public override void Exit()
    {
        
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

        player.controller.Move(move * (player.speed * Time.deltaTime));
        player.animator.SetFloat("speed", move.magnitude);

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(move, Vector3.up);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, toRotation, Time.deltaTime * 10f);
        }

        if (player.controller.isGrounded)
        {
            player.stateMachine.Transit(player.idleState);
        }
    }

    public override void OnAttack()
    {
        player.stateMachine.Transit(player.jumpAttackState);
    }
}
