using UnityEngine;

public class DeadState : State
{
    public DeadState(PlayerBehaviour player, State parent) : base(player , parent){}
    public override void Enter(){}
    public override void Exit(){}
    public override void GetHit(int damage){}
}
