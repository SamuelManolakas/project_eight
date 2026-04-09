using UnityEngine;

public class DeadState_B : State_B
{
    public DeadState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}
    public override void Enter(){}
    public override void Exit(){}
    public override void GetHit(int damage){}
}