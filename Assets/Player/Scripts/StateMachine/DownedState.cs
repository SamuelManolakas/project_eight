using UnityEngine;

public class DownedState : State
{


    public DownedState(PlayerBehaviour player, State parent) : base(player, parent) {}

    public override void Enter()
    {
        player.animator.Play("KnockedBack");
        player.StopAllCoroutines();

        //if (player.IsOwner)
            //HUDManager.Instance.ShowDownedUI(true);
            
        player.RemovePlayerFromBossList();
    }

    public override void Exit()
    {
        player.AddPlayerFromBossList();
        
        //if (player.IsOwner)
            //HUDManager.Instance.ShowDownedUI(false);
    }

    public override void GetHit(int damage) {} // can't be hurt while downed
    public override void OnGuard() {}
    public override void OnDodge() {}
    public override void OnJump() {}
    public override void OnPrimaryAttack() {}
    public override void OnMove() {}
}