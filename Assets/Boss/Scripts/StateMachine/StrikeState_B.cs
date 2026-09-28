using System.Collections;
using UnityEngine;

public class StrikeState_B : State_B
{
    public StrikeState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}

    public override void Enter()
    {
        boss.hitBox.damage = boss.damage;
        boss.animator.Play("Strike");
        StartCoroutine(Wait());
        boss.swordCollider.enabled = true;
        
        //Audio
        if (boss.bossAudioScriptableObject != null)
        {
            boss.bossAudioNetworker.TriggerStrikeAudio();

        }
        boss.SetEngineSound(0);
    }

    public override void Exit()
    {
        boss.hitBox.damage = 0;
        boss.swordCollider.enabled = false;
        
    }
    
    public override void ContinuousAction()
    {
        if (!boss.currentTarget)
        {
            return;
        }
        Vector3 direction = boss.currentTarget.transform.position - boss.transform.position;
        direction = Vector3.ClampMagnitude(direction, 1);
        direction.y = 0;
        
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
            boss.upperBody.transform.rotation = Quaternion.Slerp(boss.upperBody.transform.rotation, toRotation, Time.deltaTime * 3);
        }
    }
    
    private IEnumerator Wait()
    {
        boss.hitBox.hitTargets.Clear();
        
        yield return new WaitForSeconds(3.5f);
        boss.stateMachine.Transit(boss.movementState);
    }
}
