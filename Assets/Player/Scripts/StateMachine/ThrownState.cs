using UnityEngine;

public class ThrownState : State
{
    private bool _hasReportedLanding;

    public ThrownState(PlayerBehaviour player, State parent) : base(player, parent) {}

    public override void Enter()
    {
        _hasReportedLanding = false;
        player.animator.Play("KnockedBack");
        player.RemovePlayerFromBossList();
    }

    public override void Exit()
    {
        player.AddPlayerFromBossList(); // DownedState.Enter() removes it again — harmless
    }

    public override void ContinuousAction()
    {
        player.controller.Move((player.horizontalVelocity + player.velocity) * Time.deltaTime);
        player.horizontalVelocity = Vector3.MoveTowards(player.horizontalVelocity, Vector3.zero, player.deceleration * Time.deltaTime);

        if (!_hasReportedLanding && player.controller.isGrounded && player.velocity.y <= 0f)
        {
            _hasReportedLanding = true;
            player.NotifyThrowLandedServerRpc();
        }
    }

    public override void GetHit(int damage) {}
    public override void OnGuard() {}
    public override void OnDodge() {}
    public override void OnJump() {}
    public override void OnPrimaryAttack() {}
    public override void OnMove() {}
}