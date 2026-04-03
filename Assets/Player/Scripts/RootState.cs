using UnityEngine;

public class RootState : State{
    public RootState(PlayerBehaviour player, State parent) : base(player , parent){}
    public override void Enter(){}
    public override void Exit(){}
    public override void GetHit(int damage){}
    public override void ContinuousAction(){}
}