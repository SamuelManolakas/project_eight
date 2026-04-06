using System.Collections;
using UnityEngine;

public class JumpAttackState : State
{
    public JumpAttackState(PlayerBehaviour player, State parent) : base(player, parent){}

    private Vector3 _jumpDirection;
    public override void Enter()
    {
        player.animator.SetTrigger("attack");
        _jumpDirection = player.controller.velocity;
        
        player.StartCoroutine(Wait());
    }

    public override void Exit()
    {
        
    }

    public override void ContinuousAction()
    {
        if (!player.controller.isGrounded)
        {
            player.controller.Move(_jumpDirection.normalized * (player.speed * Time.deltaTime));
        }
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1.6f);
        player.stateMachine.Transit(player.idleState);
    }
}
