using System.Collections;
using UnityEngine;

public class AttackState : State
{
    public AttackState(PlayerBehaviour player, State parent) : base(player , parent){}
    
    public override void Enter()
    {
        if (player.IsOwner)
        {
            if (player.swordAndShield)
            {
                player.hitBox.damage = player.damage;
                player.StartCoroutine(SwordAndShieldCombo());
            }
        }
    }

    public override void Exit()
    {
        if (player.swordAndShield)
            player.greatSwordModel.GetComponent<Collider>().enabled = false;
    }

    public override void OnAttack()
    {
        
    }

    private IEnumerator SwordAndShieldCombo()
    {
        player.stamina.TryUseStamina(15);
        
        yield return new WaitForSeconds(0.2f);
        
        player.attackBuffer = false;
        player.greatSwordModel.GetComponent<Collider>().enabled = true;
        player.animator.Play("Combo1a");
        
        yield return new WaitForSeconds(1f);
        if (player.attackBuffer)
        {
            player.stamina.TryUseStamina(15);
            player.animator.Play("Combo1b");
            player.attackBuffer  = false;
            
            yield return new WaitForSeconds(1f);
            if (player.attackBuffer)
            {
                player.stamina.TryUseStamina(15);
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
