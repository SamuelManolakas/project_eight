using System.Collections;
using UnityEngine;

public class HeavyAttackState : State
{
    public HeavyAttackState(PlayerBehaviour player, State parent) : base(player , parent){}

    public override void Enter()
    {
        if (player.swordAndShield)
        {
            player.hitBox.damage = player.damage * 2;
            player.StartCoroutine(Wait());
        }
        else if(player.bolter)
        {
            player.StartCoroutine(Aim());
        }
        
    }

    public override void Exit()
    {
    }

    private IEnumerator Aim()
    {
        player.animator.Play("Aim");
        
        yield return new WaitForSeconds(0.5f);
        
        player.stateMachine.Transit(player.idleState);
    }

    private IEnumerator Wait()
    {
        player.stamina.TryUseStamina(20);
        player.animator.Play("HeavyAttack");
        
        yield return new WaitForSeconds(1.75f);
        player.stateMachine.Transit(player.idleState);
    }
}
