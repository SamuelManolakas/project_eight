using System.Collections;
using UnityEngine;

public class RunAttackState : State
{
    public RunAttackState(PlayerBehaviour player, State parent) : base(player, parent){}

    private bool _isAttacking;
    private float _attackTimer;
    private Vector3 _attackDirection;
    private float _attackSpeed;

    public override void Enter()
    {
        player.animator.Play("Run Attack");
        
        player.weapon.GetComponent<Collider>().enabled = true;
        player.hitBox.damage = player.primaryAttackDamage;
        
        _attackDirection = player.controller.velocity;
        _attackDirection.y = 0f;
        _attackDirection.Normalize();
        _attackSpeed = player.speed;

        _isAttacking = true;
        _attackTimer = 0f;
        
        player.stamina.TryUseStamina(player.primaryAttackStaminaCost);
        
        player.StartCoroutine(Attack());
    }

    public override void Exit()
    {
        player.weapon.GetComponent<Collider>().enabled = false;
        
        player.StopAllCoroutines();
    }

    public override void ContinuousAction()
    {
        if (!_isAttacking)
        {
            player.BleedHorizontalVelocity();
            return;
        }

        _attackTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_attackTimer / player.dodgeDuration);

        player.horizontalVelocity = _attackDirection * _attackSpeed;

        if (t >= 0.4f)
            _isAttacking = false;
    }
    
    private IEnumerator Attack()
    {
        yield return new WaitForSeconds(1.7f);
        
        player.ClearHitTargets();
        player.stateMachine.Transit(player.idleState);
    }
}
