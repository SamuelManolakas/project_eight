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
                player.StartCoroutine(SwordAndShieldCombo());
            }
            else if(player.greatSword)
            {
                player.hitBox.damage = player.primaryAttackDamage;
                player.StartCoroutine(GreatSwordCombo());
            }
        }
    }

    public override void Exit()
    {
        if (player.swordAndShield)
            player.weapon.GetComponent<Collider>().enabled = false;
        
        player.StopAllCoroutines();
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
        player.weapon.GetComponent<Collider>().enabled = true;
        player.animator.Play("Combo1a");
        if (player.PlayerAudioScriptableObject != null)
        {
            player.PlayerAudioScriptableObject.PlaySSLightAudioPlay(player.audioSource);
        }
        
        yield return new WaitForSeconds(1f);
        if (player.attackBuffer)
        {
            player.ClearHitTargets();
            player.stamina.TryUseStamina(player.primaryAttackStaminaCost);
            player.animator.Play("Combo1b");
            player.attackBuffer  = false;
            if (player.PlayerAudioScriptableObject != null)
            {
                player.PlayerAudioScriptableObject.PlaySSLightAudioPlay(player.audioSource);
            }
            
            yield return new WaitForSeconds(1f);
            if (player.attackBuffer)
            {
                player.ClearHitTargets();
                player.stamina.TryUseStamina(player.primaryAttackStaminaCost);
                player.animator.Play("Combo1c");
                player.attackBuffer  = false;
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
        player.weapon.GetComponent<Collider>().enabled = true;
        player.animator.Play("Combo1a");

        
        yield return new WaitForSeconds(3f);
        
        Debug.Log(player.hitBox.hitTargets.Count);
        
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
        if (_attackImpulseTimer >= lungeDuration) return Vector3.zero;

        _attackImpulseTimer += Time.deltaTime;
        return _attackImpulse ;
    }

    public override void ContinuousAction()
    {
        GetAttackImpulse();
        player.controller.Move((GetAttackImpulse() * player.speed + player.velocity) * Time.deltaTime);
    }
}
