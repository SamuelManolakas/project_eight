using System.Collections;
using UnityEngine;

public class DodgeState : State
{
    public DodgeState(PlayerBehaviour player, State parent) : base(player, parent){}

    private Vector3 _dodgeDirection;
    public override void Enter()
    {
        HandleAnimation();
        _dodgeDirection = player.controller.velocity.normalized;

        if (player.moveInput == Vector2.zero)
        {
            _dodgeDirection = -player.transform.forward;
        }
        
        player.StartCoroutine(Wait());
    }

    private void HandleAnimation()
    {
        if (player.camera._isLockedOn && player.speed != player.sprintSpeed)
        {
            if (player.moveInput.x > 0.5f)
            {
                player.animator.Play("Dodge Right");
            }
            else if (player.moveInput.x < -0.5f)
            {
                player.animator.Play("Dodge Left");
            }
            else if (player.moveInput.y > 0.5f)
            {
                player.animator.Play("Dodge Roll Forward");
            }
            else 
            {
                player.animator.Play("Dodge Roll Backward");
            }
        }
        else if (player.moveInput == Vector2.zero)
        {
            player.animator.Play("Dodge Roll Backward");
        }
        else
        {
            player.animator.Play("Dodge Roll Forward");
        }
    }

    public override void Exit()
    {
        
    }

    public override void GetHit(int damage)
    {
        
    }

    public override void ContinuousAction()
    {
        player.controller.Move((_dodgeDirection * player.dodgeDistance + player.velocity) * Time.deltaTime);
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.6f);
        player.stateMachine.Transit(player.idleState);
    }

    public override void OnJump()
    {
    }
}
