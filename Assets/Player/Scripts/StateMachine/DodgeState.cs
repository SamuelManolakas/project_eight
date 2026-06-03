using System.Collections;
using UnityEngine;

public class DodgeState : State
{
    public DodgeState(PlayerBehaviour player, State parent) : base(player, parent){}

    private bool _isDodging;
    private float _dodgeTimer;
    private Vector3 _dodgeDirection;
    private bool _isInvincible;
    public override void Enter()
    {
        HandleAnimation();
        _dodgeDirection = player.controller.velocity.normalized;

        if (player.moveInput == Vector2.zero)
        {
            _dodgeDirection = -player.transform.forward;
        }

        player.animator.speed = 1.3f;
        
        _isDodging = true;
        _dodgeTimer = 0f;
        
        player.stamina.ConsumeStaminaServer(player.dodgeStaminaCost);
        
        player.StartCoroutine(IFrameWindow(0.1f, 0.4f));
        
        //Audio
        if (player.PlayerAudioScriptableObject != null)
        {
            player.PlayerAudioScriptableObject.PlayRollAudioPlay(player.audioSource);
        }
    }

    public override void Exit()
    {
        player.animator.speed = 1f;
        
        player.StopAllCoroutines();
    }

    public override void GetHit(int damage)
    {
        if (_isInvincible) return;
        
        if (player.health.Health >= 0)
        {
            player.health.TakeDamage(damage);
        }
    }

    public override void ContinuousAction()
    {
        if (!_isDodging) return;

        _dodgeTimer += Time.deltaTime;
        float progress = _dodgeTimer / player.dodgeDuration; // 0 → 1

        float t = Mathf.Clamp01(_dodgeTimer / player.dodgeDuration);

        // Ease Out Cubic: fast start, smooth stop
        float easedT = 1f - Mathf.Pow(1f - t, 3f);

        // Differentiate to get speed (derivative of ease out cubic)
        // speed = d/dt [ 1 - (1-t)^3 ] = 3(1-t)^2
        float speed = 3f * Mathf.Pow(1f - t, 2f) * (player.dodgeDistance / player.dodgeDuration);

        if (t >= player.dodgeDuration - 0.2f)
        {
            _isDodging = false;
            player.controller.Move(Vector3.zero);
            player.stateMachine.Transit(player.idleState);
        }
        
        player.controller.Move((_dodgeDirection * speed + player.velocity) * Time.deltaTime);
    }

    private IEnumerator IFrameWindow(float startFraction, float endFraction)
    {
        yield return new WaitForSeconds(player.dodgeDuration * startFraction);
        _isInvincible = true;
        yield return new WaitForSeconds(player.dodgeDuration * (endFraction - startFraction));
        _isInvincible = false;
    }

    public override void OnJump()
    {
    }
    
    private void HandleAnimation()
    {
        if (player.camera._isLockedOn && player.speed != player.sprintSpeed)
        {
            if (player.moveInput.x > 0.5f)
            {
                player.animator.Play("Dodge Right");
            }
            else if (player.moveInput.x < -0.5f)
            {
                player.animator.Play("Dodge Left");
            }
            else if (player.moveInput.y > 0.5f)
            {
                player.animator.Play("Dodge Roll Forward");
            }
            else 
            {
                player.animator.Play("Dodge Roll Backward");
            }
        }
        else if (player.moveInput == Vector2.zero)
        {
            player.animator.Play("Dodge Roll Backward");
        }
        else
        {
            player.animator.Play("Dodge Roll Forward");
        }
    }
}
