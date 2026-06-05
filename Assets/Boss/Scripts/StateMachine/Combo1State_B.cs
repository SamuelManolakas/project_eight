using System.Collections;
using UnityEngine;

public class Combo1State_B : State_B
{
    public Combo1State_B(BossBehaviour boss, State_B parent) : base(boss , parent){}
    
    public override void Enter()
    {
        boss.hitBox.damage = boss.damage;
        boss.StartCoroutine(Combo());
        
        //Audio
        if (boss.bossAudioScriptableObject != null)
        {
            boss.bossAudioScriptableObject.Boss3HcAudioPlay(boss.audioSource);
            boss.bossAudioNetworker.Trigger3HcAudio();
        }
        boss.bossEngineSound = 0;
        if (boss.bossAudioScriptableObject != null)
        {
            boss.bossAudioScriptableObject.PlayEngineAudioPlay(boss.audioSource, boss.bossEngineSound, boss.bossChargeCrashSound);
            boss.bossAudioNetworker.TriggerEngineAudio(boss.bossEngineSound, boss.bossChargeCrashSound);
        }
    }

    public override void Exit()
    {
        boss.hitBox.damage = 0;
        boss.greatSwordModel.GetComponent<Collider>().enabled = false;
        
        boss.StopAllCoroutines();
    }
    
    private IEnumerator Combo()
    {
        boss.hitBox.hitTargets.Clear();
        boss.animator.Play("3HitCombo");
        
        yield return new WaitForSeconds(1f);
        boss.greatSwordModel.GetComponent<Collider>().enabled = true;

        yield return new WaitForSeconds(1.5f);
        boss.hitBox.hitTargets.Clear();
        
        yield return new WaitForSeconds(1.3f);
        boss.hitBox.hitTargets.Clear();
        
        yield return new WaitForSeconds(2.7f);
        boss.stateMachine.Transit(boss.movementState);
    }

    public override void ContinuousAction()
    {
        Vector3 lookDirection = boss.currentTarget.transform.position - boss.transform.position;
        lookDirection = Vector3.ClampMagnitude(lookDirection, 1);
        lookDirection.y = 0;
        
        Quaternion toRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        boss.upperBody.transform.rotation = Quaternion.Slerp(boss.upperBody.transform.rotation, toRotation, Time.deltaTime);
    }
}
