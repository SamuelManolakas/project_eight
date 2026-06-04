using UnityEngine;

public class DeadState_B : State_B
{
    public DeadState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}

    public override void Enter()
    {
        boss.animator.Play("Death");
            
        //Audio
        if (boss.bossAudioScriptableObject != null)
        {
            boss.bossAudioScriptableObject.BossDieAudioPlay(boss.audioSource);
        }
    }

    public override void Exit()
    {
        boss.StopAllCoroutines();
    }
    public override void GetHit(int damage){}

    public override void ContinuousAction(){}
}