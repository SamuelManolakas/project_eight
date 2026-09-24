using UnityEngine;

public class CarriedState : State
{
    public CarriedState(PlayerBehaviour player, State parent) : base(player, parent) {}

    public override void Enter()
    {
        player.animator.Play("Carried");
        player.controller.enabled = false; // keep this — stops the capsule collider from pushing against things while parented
        // Clear movement on enter, not exit: the throw sets velocity right before leaving this state.
        player.velocity = Vector3.zero;
        player.horizontalVelocity = Vector3.zero;
        player.RemovePlayerFromBossList();
    }

    public override void Exit()
    {
        player.controller.enabled = true;
        player.carrierPlayer = null;
    }

    public override void GetHit(int damage) {}
    public override void OnGuard() {}
    public override void OnDodge() {}
    public override void OnJump() {}
    public override void OnPrimaryAttack() {}
    public override void OnMove() {}
}