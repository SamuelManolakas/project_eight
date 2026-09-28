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
        
        player.weaponCollider.enabled = true;
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
        player.weaponCollider.enabled = false;
        
        player.StopAllCoroutines();
    }

    public override void ContinuousAction()
    {
        if (!_isAttacking)
        {
            player.BleedHorizontalVelocity();
            return;
        }

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // Lunge lasts 40% of dodgeDuration. Only move for the part of this frame that's inside that
        // window, so a long (low-FPS) last frame doesn't carry the player further.
        float lungeDuration = 0.4f * player.dodgeDuration;
        float activeTime = Mathf.Min(dt, lungeDuration - _attackTimer);
        _attackTimer += dt;

        player.horizontalVelocity = _attackDirection * (_attackSpeed * activeTime / dt);

        if (_attackTimer >= lungeDuration)
            _isAttacking = false;
    }
    
    private IEnumerator Attack()
    {
        yield return new WaitForSeconds(1.7f);
        
        player.ClearHitTargets();
        player.stateMachine.Transit(player.idleState);
    }
}
