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
        Debug.Log("I got hit while guarding!");
        
        //if (player.currentStamina.Value <= 0)
        //{
        //    player.currentHealth.Value -= damage;
        //    player.TransitToStunnedStateClientRpc(player.transform.position);
        //}
    }
}
