using System.Collections;
using UnityEngine;

public class CarryState : State
{
    public CarryState(PlayerBehaviour player, State parent) : base(player, parent) {}

    private const string CarryLayerName = "Carry";
    private const float JumpTakeoffDelay = 0.1f; // same wind-up as JumpState, to match the Jump animation

    private int _carryLayer = -1;
    private bool _isJumping;
    private bool _hasTakenOff;

    public override void Enter()
    {
        _isJumping = false;
        _hasTakenOff = false;

        // Legs: the base layer walks/idles by itself from the "speed" parameter set below
        player.animator.Play("Idle", 0, 0f);

        // Left arm: the "Carry" layer (masked to the left arm) holds the carry pose on top.
        // Looked up by name because the controllers have different numbers of layers.
        _carryLayer = player.animator.GetLayerIndex(CarryLayerName);
        if (_carryLayer >= 0)
        {
            player.animator.Play("Carry", _carryLayer, 0f);
            player.animator.SetLayerWeight(_carryLayer, 1f);
        }
    }

    public override void Exit()
    {
        player.DropCarriedPlayerIfStillHeld(); // interrupted (downed, grabbed...) instead of throwing
        player.carriedPlayer = null;

        if (_carryLayer >= 0)
            player.animator.SetLayerWeight(_carryLayer, 0f);
    }

    public override void ContinuousAction()
    {
        Vector3 input = player.GetCameraRelativeInput();
        if (input.sqrMagnitude > 0.001f)
            player.UpdateSprintStamina(); // sprinting while carrying costs stamina like normal sprinting

        float moveSpeed = player.speed * player.carrySpeedMultiplier;
        Vector3 targetVelocity = input * moveSpeed;

        float lerpRate = targetVelocity.sqrMagnitude > 0.001f
            ? player.acceleration
            : player.deceleration;

        player.horizontalVelocity = Vector3.Lerp(player.horizontalVelocity, targetVelocity, Smoothing.Factor(lerpRate));

        if (player.horizontalVelocity.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(player.horizontalVelocity.normalized, Vector3.up);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, toRotation, Smoothing.Factor(10f));
            player.animator.SetFloat(AnimatorParams.Speed, player.horizontalVelocity.magnitude / moveSpeed);
        }
        else
        {
            player.animator.SetFloat(AnimatorParams.Speed, 0f);
        }

        // Landed: hand the legs back to the Idle/Run animations. isGrounded is from last frame's
        // Move, so also require falling, otherwise the frame after takeoff would count as landing.
        if (_hasTakenOff && player.controller.isGrounded && player.velocity.y <= 0f)
        {
            _isJumping = false;
            _hasTakenOff = false;
            player.animator.Play("Idle", 0, 0f);
        }
    }

    // Jumping happens inside this state: switching to JumpState would leave CarryState, which drops
    // the carried player. PlayerBehaviour only calls this while grounded.
    public override void OnJump()
    {
        if (_isJumping) return;

        _isJumping = true;
        player.animator.Play("Jump", 0, 0f); // base layer only; the carry layer keeps the left arm in place
        StartCoroutine(JumpTakeoff());
    }

    private IEnumerator JumpTakeoff()
    {
        yield return new WaitForSeconds(JumpTakeoffDelay);
        player.velocity.y = Mathf.Sqrt(player.jumpHeight * -2f * player.gravity);
        _hasTakenOff = true;
    }

    public override void OnSprint() => player.ToggleSprint();

    public override void OnPrimaryAttack() {}
    public override void OnSecondaryAttack() {}
    public override void OnGuard() {}
    public override void OnDodge() {}
    // OnMove intentionally not overridden: the base layer's Idle/Run transitions follow "speed".
}
