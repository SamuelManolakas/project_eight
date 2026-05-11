using System.Collections;
using UnityEngine;

public class AttackState : State
{
    public AttackState(PlayerBehaviour player, State parent) : base(player , parent){}
    
    public override void Enter()
    {
        if (player.IsOwner)
        {
            player.hitBox.damage = player.damage;
            player.StartCoroutine(Combo());
        }
    }

    public override void Exit()
    {
        player.greatSwordModel.GetComponent<Collider>().enabled = false;
    }

    private IEnumerator Combo()
    {
        yield return new WaitForSeconds(0.2f);
        
        
        player.attackBuffer = false;
        player.greatSwordModel.GetComponent<Collider>().enabled = true;
        player.animator.Play("Combo1a");
        
        yield return new WaitForSeconds(1f);
        if (player.attackBuffer)
        {
            player.animator.Play("Combo1b");
            player.attackBuffer  = false;
            
            yield return new WaitForSeconds(1f);
            if (player.attackBuffer)
            {
                player.animator.Play("Combo1c");
                player.attackBuffer  = false;
                
                yield return new WaitForSeconds(1f);
                player.stateMachine.Transit(player.idleState);
            }
            else
            {
                player.stateMachine.Transit(player.idleState);
            }
        }
        else
        {
            player.stateMachine.Transit(player.idleState);
        }
    }
}
