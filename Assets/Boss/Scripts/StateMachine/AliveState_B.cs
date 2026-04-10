using Unity.Netcode;
using UnityEngine;

public class AliveState_B : State_B
{
    public AliveState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}
    public override void Enter(){}
    public override void Exit(){}
    public override void GetHit(int damage)
    {
        boss.currentHealth.Value -= damage;
        boss.slider.value = boss.currentHealth.Value;
        
        Debug.Log(boss.currentHealth.Value);

        if (boss.currentHealth.Value <= boss.maxHealth / 2)
        {
            boss.stateMachine.Transit(boss.phase2State);
        }

        if (boss.currentHealth.Value <= 0)
        {
            boss.stateMachine.Transit(boss.deadState);
        }
    }
}