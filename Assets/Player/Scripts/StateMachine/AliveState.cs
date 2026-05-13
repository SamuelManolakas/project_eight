using UnityEngine;

public class AliveState : State
{
    public AliveState(PlayerBehaviour player, State parent) : base(player , parent){}
    public override void Enter(){}
    public override void Exit(){}

    public override void GetHit(int damage)
    {
        if (player.health.Health >= 0)
        {
            player.health.TakeDamage(damage);
        }
    }
}
