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
        
        player.StopAllCoroutines();
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.1f);
        player.velocity.y = Mathf.Sqrt(player.jumpHeight * -2f * player.gravity);
        canJump = true;
    }
    
    public override void ContinuousAction()
    {
        Vector3 move = player.GetCameraRelativeInput();
        player.horizontalVelocity = move * player.speed;

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(move, Vector3.up);
            player.transform.rotation =
                Quaternion.Slerp(player.transform.rotation, toRotation, Time.deltaTime * 5f);
        }
        
        if (canJump)
        {
            // isGrounded is from last frame's Move, so also require that we're falling —
            // otherwise the frame right after takeoff would count as a landing.
            if (player.controller.isGrounded && player.velocity.y <= 0f)
            {
                player.stateMachine.Transit(player.idleState);
            }
        }
    }

    public override void OnPrimaryAttack()
    {
        if (!player.stamina.TryUseStamina(player.primaryAttackStaminaCost)) return;
        
        if (player.swordAndShield || player.greatSword)
            player.StartCoroutine(WaitToTransition());
    }

    private IEnumerator WaitToTransition()
    {
        yield return new WaitForSeconds(0.25f);
        player.stateMachine.Transit(player.jumpAttackState);
    }
}
