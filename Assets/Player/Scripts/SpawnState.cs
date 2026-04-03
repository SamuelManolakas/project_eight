using System.Collections;
using UnityEngine;

public class SpawnState : State
{
    public SpawnState(PlayerBehaviour player, State parent) : base(player , parent){}

    public override void Enter()
    {
        Debug.Log("Entered SpawnState");
        player.StartCoroutine(Wait());
    }

    public override void Exit()
    {
        Debug.Log("Exit SpawnState");
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1f);
        player.stateMachine.Transit(player.idleState);
    }
}
