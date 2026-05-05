using System.Collections;
using UnityEngine;

public class DeadState : State
{
    public DeadState(PlayerBehaviour player, State parent) : base(player , parent){}

    public override void Enter()
    {
        player.animator.Play("KockedBack");
        player.StartCoroutine(Wait());
    }
    public override void Exit(){}
    public override void GetHit(int damage){}
    public override void ContinuousAction()
    {
    }
    public override void OnGuard()
    {
    }
    public override void OnDodge()
    {
    }
    public override void OnJump()
    {
    }
    public override void OnAttack()
    {
    }
    public override void OnMove()
    {
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.5f);
        player.animator.Play("LayingDown");
    }
}
