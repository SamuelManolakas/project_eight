using System.Collections;
using UnityEngine;

public class JumpState : State
{
    public JumpState(PlayerBehaviour player, State parent) : base(player, parent){}

    private bool canJump;
    public override void Enter()
    {
        canJump = false;
        player.animator.Play("Jump");

        player.StartCoroutine(Wait());
    }
    public override void Exit()
    {
        canJump = false;
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.1f);
        player.velocity.y = Mathf.Sqrt(player.jumpHeight * -2f * player.gravity);
        canJump = true;
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

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(move, Vector3.up);
            player.transform.rotation =
                Quaternion.Slerp(player.transform.rotation, toRotation, Time.deltaTime * 5f);
        }
        
        if (canJump)
        {
            

            if (player.controller.isGrounded)
            {
                player.stateMachine.Transit(player.idleState);
            }
        }
    }

    public override void OnPrimaryAttack()
    {
        if (player.swordAndShield || player.greatSword)
            player.StartCoroutine(WaitToTransition());

    }

    private IEnumerator WaitToTransition()
    {
        yield return new WaitForSeconds(0.25f);
        player.stateMachine.Transit(player.jumpAttackState);
    }
}
