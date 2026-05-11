using System.Collections;
using UnityEngine;

public class GuardState : State
{
    public GuardState(PlayerBehaviour player, State parent) : base(player , parent){}

    public override void Enter()
    {
        player.animator.Play("Guard");

        if (!player.IsOwner) return;
        
        player.stamina.IsGuarding.Value = true;
    }

    public override void Exit()
    {
        if (!player.IsOwner) return;
        
        player.stamina.IsGuarding.Value = false;
    }
}
