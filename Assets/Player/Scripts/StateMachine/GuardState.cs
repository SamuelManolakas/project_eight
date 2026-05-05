using System.Collections;
using UnityEngine;

public class GuardState : State
{
    public GuardState(PlayerBehaviour player, State parent) : base(player , parent){}

    public override void Enter()
    {
        player.animator.Play("Guard");
    }

    public override void Exit()
    {
        
    }

    public override void GetHit(int damage)
    {
        player.stamina.Value -= 4f;
        if (player.stamina.Value <= 0)
        {
            player.currentHealth.Value -= damage / 2;
        }
    }
}
