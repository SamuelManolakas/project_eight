using UnityEngine;

public class CarriedState : State
{
    public CarriedState(PlayerBehaviour player, State parent) : base(player, parent) {}

    public override void Enter()
    {
        player.animator.Play("Carried");
        player.controller.enabled = false; // keep this — stops the capsule collider from pushing against things while parented
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