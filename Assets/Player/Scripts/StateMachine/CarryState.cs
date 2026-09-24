using UnityEngine;

public class CarryState : State
{
    public CarryState(PlayerBehaviour player, State parent) : base(player, parent) {}

    public override void Enter()
    {
        player.animator.Play("CarryIdle");
        player.speed = player.initialSpeed; // don't carry over an active sprint speed
    }

    public override void Exit()
    {
        player.carriedPlayer = null;
    }

    public override void ContinuousAction()
    {
        float moveSpeed = player.speed * player.carrySpeedMultiplier;
        Vector3 targetVelocity = player.GetCameraRelativeInput() * moveSpeed;

        float lerpRate = targetVelocity.sqrMagnitude > 0.001f
            ? player.acceleration
            : player.deceleration;

        player.horizontalVelocity = Vector3.Lerp(player.horizontalVelocity, targetVelocity, lerpRate * Time.deltaTime);

        if (player.horizontalVelocity.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(player.horizontalVelocity.normalized, Vector3.up);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, toRotation, Time.deltaTime * 10f);
            player.animator.SetFloat("speed", player.horizontalVelocity.magnitude / moveSpeed);
        }
        else
        {
            player.animator.SetFloat("speed", 0f);
        }
    }

    public override void OnPrimaryAttack() {}
    public override void OnSecondaryAttack() {}
    public override void OnGuard() {}
    public override void OnDodge() {}
    public override void OnJump() {}
    public override void OnSprint() {} // no sprinting while carrying
    // OnMove intentionally not overridden — no idle/move split, just blends via "speed" above.
}