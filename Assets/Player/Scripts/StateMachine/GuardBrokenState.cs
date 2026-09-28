using System.Collections;
using UnityEngine;

/// <summary>
/// Knocked down after a guard break: KnockedBack, then LayingDown (like GrabbedState), but no damage.
/// Once lying down, the player can get back up after 1 second by moving.
/// </summary>
public class GuardBrokenState : State
{
    public GuardBrokenState(PlayerBehaviour player, State parent) : base(player, parent){}

    private const float KnockdownDuration = 1f;

    private bool _canGetUp;
    private bool _isGettingUp;

    public override void Enter()
    {
        _canGetUp = false;
        _isGettingUp = false;
        // The animator controllers move KnockedBack → LayingDown on their own once the knockback finishes.
        player.animator.Play("KnockedBack");
        StartCoroutine(Knockdown());
    }

    public override void Exit()
    {
        _canGetUp = false;
        _isGettingUp = false;
    }

    public override void ContinuousAction()
    {
        player.BleedHorizontalVelocity();

        // OnMove only fires when input changes, so also get up if a direction is already held.
        if (player.moveInput.sqrMagnitude > 0.01f)
            TryGetUp();
    }

    public override void OnMove() => TryGetUp();

    private void TryGetUp()
    {
        if (!_canGetUp || _isGettingUp) return;

        _isGettingUp = true;
        StartCoroutine(GetUp());
    }

    private IEnumerator Knockdown()
    {
        // Wait until the player is actually lying down (with a safety timeout in case the
        // controller's transition changes), then start the 1-second recovery.
        float timeout = 3f;
        while (!player.animator.GetCurrentAnimatorStateInfo(0).IsName("LayingDown") && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(KnockdownDuration);
        _canGetUp = true;
    }

    private IEnumerator GetUp()
    {
        player.animator.Play("Get Up");
        yield return new WaitForSeconds(2f);
        player.stateMachine.Transit(player.movementState);
    }
}
