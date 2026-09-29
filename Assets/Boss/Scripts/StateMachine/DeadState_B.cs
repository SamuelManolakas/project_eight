using UnityEngine;

public class DeadState_B : State_B
{
    public DeadState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}

    public override void Enter()
    {
        boss.animator.Play("Death");
        boss.deathSequence.Play(); // explosions, head launch and fallen sword (timings on BossDeathSequence)
        
        //Audio
        boss.SetEngineSound(3);
        if (boss.bossAudioScriptableObject != null)
        {
            boss.bossAudioNetworker.TriggerDieAudio();

        }
    }

    public override void Exit()
    {
        
        //Audio
        boss.bossEngineSound = 3;
        if (boss.bossAudioScriptableObject != null)
        {
            boss.bossAudioScriptableObject.PlayEngineAudioPlay(boss.audioSource, boss.bossEngineSound, boss.bossChargeCrashSound);
        }
    }
    public override void GetHit(int damage){}

    public override void ContinuousAction(){}
}