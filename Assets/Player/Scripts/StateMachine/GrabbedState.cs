using System.Collections;
using UnityEngine;

public class GrabbedState : State
{
    public GrabbedState(PlayerBehaviour player, State parent) : base(player , parent){}

    bool canMove;
    public override void Enter()
    {
        player.animator.Play("LayingDown");
        player.StartCoroutine(Wait());
        canMove = false;
    }

    public override void Exit()
    {
        canMove = false;
        
        player.StopAllCoroutines();
    }

    public override void ContinuousAction()
    {
        
    }

    public override void OnMove()
    {
        if (canMove)
            player.StartCoroutine(GetUp());
    }

    private IEnumerator GetUp()
    {
        player.animator.Play("Get Up");
        yield return new WaitForSeconds(2f);
        player.stateMachine.Transit(player.movementState);
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(2.5f);
        //Temporary damage solution
        if (player.health.Health >= 0)
        {
            player.health.TakeDamage((player.health.maxHealth / 3) * 2);
        }
        //knock back from the explosion
        
        canMove = true;
    }
}
