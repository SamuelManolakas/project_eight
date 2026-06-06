using System.Collections;
using UnityEngine;

public class ReloadState : State
{
    public ReloadState(PlayerBehaviour player, State parent) : base(player , parent){}

    public override void Enter()
    {
        player.animator.Play("Reload");

        player.StartCoroutine(Wait());
    }

    public override void Exit()
    {
        player.StopAllCoroutines();
    }

    public override void ContinuousAction()
    {
        player.controller.Move(player.velocity * Time.deltaTime);
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.75f);
        
        player.ammo = player.maxAmmo;
        player.stateMachine.Transit(player.idleState);
    }
}
