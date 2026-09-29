using UnityEngine;

public class CarryState : State
{
    public CarryState(PlayerBehaviour player, State parent) : base(player, parent) {}

    private const string CarryLayerName = "Carry";
    private int _carryLayer = -1;

    public override void Enter()
    {
        player.speed = player.initialSpeed; // don't carry over an active sprint speed

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
        float moveSpeed = player.speed * player.carrySpeedMultiplier;
        Vector3 targetVelocity = player.GetCameraRelativeInput() * moveSpeed;

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
    }

    public override void OnPrimaryAttack() {}
    public override void OnSecondaryAttack() {}
    public override void OnGuard() {}
    public override void OnDodge() {}
    public override void OnJump() {}
    public override void OnSprint() {} // no sprinting while carrying
    // OnMove intentionally not overridden: the base layer's Idle/Run transitions follow "speed".
}