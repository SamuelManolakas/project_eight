using UnityEngine;

public class RootState_B : State_B
{
    public RootState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}
    public override void Enter(){}
    public override void Exit(){}
    public override void GetHit(int damage){}
    public override void ContinuousAction(){}
}