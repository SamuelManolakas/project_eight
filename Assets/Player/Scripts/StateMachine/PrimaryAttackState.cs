using System.Collections;
using UnityEngine;

public class PrimaryAttackState : State
{
    public PrimaryAttackState(PlayerBehaviour player, State parent) : base(player , parent){}
    
    public override void Enter()
    {
        if (player.IsOwner)
        {
            if (player.swordAndShield)
            {
                player.hitBox.damage = player.primaryAttackDamage;
                StartCoroutine(SwordAndShieldCombo());
            }
            else if(player.greatSword)
            {
                player.hitBox.damage = player.primaryAttackDamage;
                StartCoroutine(GreatSwordCombo());
            }
        }
    }

    public override void Exit()
    {
        if (player.swordAndShield)
            player.weaponCollider.enabled = false;
        
    }

    public override void OnPrimaryAttack()
    {
        
    }

    private IEnumerator SwordAndShieldCombo()
    {
        player.stamina.TryUseStamina(player.primaryAttackStaminaCost);
        player.ClearHitTargets();
        
        yield return new WaitForSeconds(0.2f);
        
        player.attackBuffer = false;
        player.weaponCollider.enabled = true;
        player.animator.Play("Combo1a");
        
        if (player.PlayerAudioScriptableObject != null)
        {
            player.PlayerAudioScriptableObject.PlaySSLightAudioPlay(player.audioSource);
        }
        
        yield return new WaitForSeconds(1f);
        if (player.attackBuffer && player.stamina.TryUseStamina(player.primaryAttackStaminaCost))
        {
            player.ClearHitTargets();
            player.stamina.TryUseStamina(player.primaryAttackStaminaCost);
            player.animator.Play("Combo1b");
            player.attackBuffer  = false;
            
            //Audio
            if (player.PlayerAudioScriptableObject != null)
            {
                player.PlayerAudioScriptableObject.PlaySSLightAudioPlay(player.audioSource);
            }
            
            yield return new WaitForSeconds(1f);
            if (player.attackBuffer && player.stamina.TryUseStamina(player.primaryAttackStaminaCost))
            {
                player.ClearHitTargets();
                player.stamina.TryUseStamina(player.primaryAttackStaminaCost);
                player.animator.Play("Combo1c");
                player.attackBuffer  = false;
                
                //Audio
                if (player.PlayerAudioScriptableObject != null)
                {
                    player.PlayerAudioScriptableObject.PlaySSLightAudioPlay(player.audioSource);
                }

                StartAttackLunge(1f);
                
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

    private IEnumerator GreatSwordCombo()
    {
        player.stamina.TryUseStamina(player.primaryAttackStaminaCost);
        player.ClearHitTargets();
        
        yield return new WaitForSeconds(0.2f);
        
        player.attackBuffer = false;
        player.weaponCollider.enabled = true;
        player.animator.Play("Combo1a");

        //Audio
        if (player.PlayerAudioScriptableObject != null)
        {
            player.PlayerAudioScriptableObject.PlayGSLightAudioPlay(player.audioSource);
        }
        
        yield return new WaitForSeconds(1.8f);
        
        player.stateMachine.Transit(player.idleState);
    }
    
    private Vector3 _attackImpulse;
    private float _attackImpulseTimer;

    private float lungeDuration = 0.5f;
   

    void StartAttackLunge(float lungeForce)
    {
        _attackImpulse = player.transform.forward * lungeForce;
        _attackImpulseTimer = 0f;
    }

    Vector3 GetAttackImpulse()
    {
        float dt = Time.deltaTime;
        if (_attackImpulseTimer >= lungeDuration || dt <= 0f) return Vector3.zero;

        // Only count the part of this frame that's inside the lunge, so the distance is the same at any frame rate.
        float activeTime = Mathf.Min(dt, lungeDuration - _attackImpulseTimer);
        _attackImpulseTimer += dt;
        return _attackImpulse * (activeTime / dt);
    }

    public override void ContinuousAction()
    {
        Vector3 impulse = GetAttackImpulse();
        if (impulse != Vector3.zero)
            player.horizontalVelocity = impulse * player.speed;
        else
            player.BleedHorizontalVelocity();
    }
}
