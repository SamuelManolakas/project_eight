using UnityEngine;

public class DownedState : State
{
    private float _reviveProgress = 0f;
    private float _downedTimer = 0f;
    private const float DownedTimeout = 30f; // auto-die after 30s

    public DownedState(PlayerBehaviour player, State parent) : base(player, parent) {}

    public override void Enter()
    {
        _reviveProgress = 0f;
        _downedTimer = 0f;
        player.animator.Play("KnockedBack");
        player.StopAllCoroutines();

        //if (player.IsOwner)
            //HUDManager.Instance.ShowDownedUI(true);
    }

    public override void Exit()
    {
        _reviveProgress = 0f;
        //if (player.IsOwner)
            //HUDManager.Instance.ShowDownedUI(false);
    }

    public override void ContinuousAction()
    {
        if (!player.IsServer) return;

        _downedTimer += Time.deltaTime;
        if (_downedTimer >= DownedTimeout)
        {
            player.TransitToDeadStateClientRpc();
        }
    }

    public override void GetHit(int damage) {} // can't be hurt while downed
    public override void OnGuard() {}
    public override void OnDodge() {}
    public override void OnJump() {}
    public override void OnPrimaryAttack() {}
    public override void OnMove() {}
}