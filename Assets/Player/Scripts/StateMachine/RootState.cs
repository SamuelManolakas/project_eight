using UnityEngine;

public class RootState : State{
    public RootState(PlayerBehaviour player, State parent) : base(player , parent){}
    public override void Enter(){}
    public override void Exit(){}
    public override void GetHit(int damage){}
    public override void ContinuousAction(){ player.BleedHorizontalVelocity(); } // default for states that don't drive movement
    public override void OnPrimaryAttack(){}
    public override void OnMove(){}
    public override void OnJump(){}
    public override void OnDodge(){}
    public override void OnSprint(){}
    public override void OnGuard(){}
    public override void OnSecondaryAttack(){}
}