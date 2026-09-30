using UnityEngine;

public class AliveState_B : State_B
{
    public AliveState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}
    public override void Enter(){}

    public override void Exit() {}
    public override void GetHit(int damage)
    {
        boss.currentHealth.Value -= damage; // the health bar updates from currentHealth's change event

        if (boss.currentHealth.Value <= 0)
        {
            boss.stateMachine.Transit(boss.deadState);
        }
        
        //Audio
        if (boss.bossAudioScriptableObject != null)
        {
            boss.bossAudioNetworker.TriggerDamageAudio();
        }
    }
}