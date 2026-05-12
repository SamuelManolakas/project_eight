using UnityEngine;

public class RootState : State{
    public RootState(PlayerBehaviour player, State parent) : base(player , parent){}
    public override void Enter(){}
    public override void Exit(){}
    public override void GetHit(int damage){}
    public override void ContinuousAction(){}
    public override void OnAttack(){}
    public override void OnMove(){}
    public override void OnJump(){}
    public override void OnDodge(){}
    public override void OnSprint(){}
    public override void OnGuard(){}
    public override void OnHeavyAttack(){}
}