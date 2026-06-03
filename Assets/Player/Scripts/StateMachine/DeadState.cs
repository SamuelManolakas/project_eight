using System.Collections;
using UnityEngine;

public class DeadState : State
{
    public DeadState(PlayerBehaviour player, State parent) : base(player , parent){}

    public override void Enter()
    {
        player.animator.Play("KnockedBack");
        player.StopAllCoroutines();
    }

    public override void Exit()
    {
        player.StopAllCoroutines();
    }
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
    public override void OnPrimaryAttack()
    {
    }
    public override void OnMove()
    {
    }
}
