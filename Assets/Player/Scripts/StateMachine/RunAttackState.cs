using System.Collections;
using UnityEngine;

public class RunAttackState : State
{
    public RunAttackState(PlayerBehaviour player, State parent) : base(player, parent){}

    private bool _isAttacking;
    private float _attackTimer;
    private Vector3 _attackDirection;
    
    public override void Enter()
    {
        player.animator.Play("Run Attack");
        
        player.weapon.GetComponent<Collider>().enabled = true;
        player.hitBox.damage = player.primaryAttackDamage;
        
        _attackDirection = player.controller.velocity.normalized;
        
        _isAttacking = true;
        _attackTimer = 0f;
        
        player.StartCoroutine(Attack());
    }

    public override void Exit()
    {
        player.weapon.GetComponent<Collider>().enabled = false;
        
        player.StopAllCoroutines();
    }

    public override void ContinuousAction()
    {
        if (!_isAttacking) return;

        _attackTimer += Time.deltaTime;
        float progress = _attackTimer / player.dodgeDuration; // 0 → 1

        float t = Mathf.Clamp01(_attackTimer / player.dodgeDuration);

        // Ease Out Cubic: fast start, smooth stop
        float easedT = 1f - Mathf.Pow(1f - t, 3f);

        // Differentiate to get speed (derivative of ease out cubic)
        // speed = d/dt [ 1 - (1-t)^3 ] = 3(1-t)^2
        float speed = player.speed -= Time.deltaTime * easedT;

        if (t >= 0.4f)
        {
            _isAttacking = false;
            player.controller.Move(Vector3.zero);
        }
        
        player.controller.Move((_attackDirection * speed + player.velocity) * Time.deltaTime);
    }
    
    private IEnumerator Attack()
    {
        yield return new WaitForSeconds(1.7f);
        
        player.ClearHitTargets();
        player.stateMachine.Transit(player.idleState);
    }
}
