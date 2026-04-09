using System.Collections;
using UnityEngine;

public class AttackState : State
{
    public AttackState(PlayerBehaviour player, State parent) : base(player , parent){}

    public override void Enter()
    {
        if (player.IsLocalPlayer)
        {
            player.animator.SetTrigger("attack");
        }

        player.hitBox.damage = player.damage;
        player.StartCoroutine(HitBoxActivation());
    }

    public override void Exit()
    {
        player.greatSwordModel.GetComponent<Collider>().enabled = false;
    }

    private IEnumerator HitBoxActivation()
    {
        yield return new WaitForSeconds(0.4f);
        player.greatSwordModel.GetComponent<Collider>().enabled = true;
        player.StartCoroutine(Wait());
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1.2f);
        player.stateMachine.Transit(player.idleState);
    }
}
