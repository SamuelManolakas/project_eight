using System.Collections;
using UnityEngine;

public class SpawnState : State
{
    public SpawnState(PlayerBehaviour player, State parent) : base(player , parent){}

    public override void Enter()
    {
        
        player.StartCoroutine(Wait());
    }

    public override void Exit()
    {
        
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1f);
        player.stateMachine.Transit(player.idleState);
    }
}
