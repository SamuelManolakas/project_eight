using System.Collections;
using UnityEngine;

public class JumpAttackState : State
{
    public JumpAttackState(PlayerBehaviour player, State parent) : base(player, parent){}

    private Vector3 _jumpDirection;
    int _fireOnce = 0;
    public override void Enter()
    {
        player.animator.Play("Jump Attack",2, 0);
        player.animator.SetLayerWeight(2, 1);
        player.stamina.TryUseStamina(player.primaryAttackStaminaCost);
        
        player.shieldHitBox.damage = player.primaryAttackDamage;
        player.shield.GetComponent<Collider>().enabled = true;
        
        _jumpDirection = player.transform.forward;
    }

    public override void Exit()
    {
        player.animator.SetLayerWeight(2, 0);
        player.shield.GetComponent<Collider>().enabled = false;

        _fireOnce = 0;
    }

    public override void OnPrimaryAttack()
    {
    }

    
    public override void ContinuousAction()
    {
        if (!player.controller.isGrounded)
        {
            player.controller.Move((_jumpDirection * player.speed + (player.velocity * 2)) * Time.deltaTime);
        }
        else
        {
            if (_fireOnce < 1)
                player.StartCoroutine(Wait());
            
            player.ClearHitTargets();
        }
    }

    private IEnumerator Wait()
    {
        _fireOnce++;
        
        yield return new WaitForSeconds(0.3f);
        player.stateMachine.Transit(player.movementState);
    }
}
