using UnityEngine;

public class AliveState : State
{
    public AliveState(PlayerBehaviour player, State parent) : base(player , parent){}
    public override void Enter(){}
    public override void Exit(){}

    public override void GetHit(int damage)
    {
        if (!player.IsServer)
        {
            return;
        }

        if (player.currentHealth.Value >= 0)
        {
            player.currentHealth.Value -= damage;
        }
        else
        {
            player.stateMachine.Transit(player.deadState);
        }
    }
}
