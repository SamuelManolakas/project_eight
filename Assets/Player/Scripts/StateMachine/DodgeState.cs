using System.Collections;
using UnityEngine;

public class DodgeState : State
{
    public DodgeState(PlayerBehaviour player, State parent) : base(player, parent){}

    // The dodge hands control back at 80% of dodgeDuration; the slow tail of the ease-out is skipped.
    private const float EndFraction = 0.8f;

    private bool _isDodging;
    private float _dodgeTimer;
    private Vector3 _dodgeDirection;
    private bool _isInvincible;
    public override void Enter()
    {
        HandleAnimation();
        _dodgeDirection = player.controller.velocity;
        _dodgeDirection.y = 0f;
        _dodgeDirection.Normalize();

        if (player.moveInput == Vector2.zero)
        {
            _dodgeDirection = -player.transform.forward;
        }

        player.animator.speed = 1.3f;
        
        _isDodging = true;
        _dodgeTimer = 0f;
        
        player.stamina.TryUseStamina(player.dodgeStaminaCost);
        
        StartCoroutine(IFrameWindow(0.1f, 0.4f));
        
        //Audio
        if (player.PlayerAudioScriptableObject != null)
        {
            player.PlayerAudioScriptableObject.PlayRollAudioPlay(player.audioSource);
        }
    }

    public override void Exit()
    {
        player.animator.speed = 1f;
        
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

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // Ease Out Cubic: fast start, smooth stop. Move by how far along the curve we got this
        // frame, rather than sampling its speed, so the dodge covers the same distance at any frame rate.
        float previousEased = EaseOutCubic(Mathf.Clamp01(_dodgeTimer / player.dodgeDuration));
        _dodgeTimer += dt;
        float t = Mathf.Clamp01(_dodgeTimer / player.dodgeDuration);
        float easedT = EaseOutCubic(t);

        float speed = (easedT - previousEased) * player.dodgeDistance / dt;
        player.horizontalVelocity = _dodgeDirection * speed;

        if (t >= EndFraction)
        {
            _isDodging = false;
            player.stateMachine.Transit(player.idleState); // Idle bleeds off the remaining speed
        }
    }

    private static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);

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
        if (player.playerCamera._isLockedOn && player.speed != player.sprintSpeed)
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
