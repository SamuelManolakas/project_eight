using System.Collections;
using UnityEngine;

public class AttackState : State
{
    public AttackState(PlayerBehaviour player, State parent) : base(player , parent){}

    public override void Enter()
    {
        if (player.IsOwner)
        {
            player.animator.SetTrigger("attack");
        }

        player.StartCoroutine(Wait());
    }

    public override void Exit()
    {
        
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1.6f);
        player.stateMachine.Transit(player.idleState);
    }
}
