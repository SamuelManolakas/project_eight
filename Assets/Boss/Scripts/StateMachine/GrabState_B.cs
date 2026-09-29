using System.Collections;
using UnityEngine;

public class GrabState_B : State_B
{
    public GrabState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}

    private bool _shouldMove;
    public override void Enter()
    {
        _shouldMove = true;
        boss.animator.Play("Grab");
        StartCoroutine(Wait());
        
        //Audio
        if (boss.bossAudioScriptableObject != null)
        {
            boss.bossAudioNetworker.TriggerGrabAudio();

        }
        boss.SetEngineSound(0);
    }

    // Clean up if the grab is interrupted partway (e.g. the boss gets staggered)
    public override void Exit()
    {
        boss.grabCollider.enabled = false;
        if (_explosionShown)
        {
            boss.SetGrabExplosionVFXClientRpc(false);
            _explosionShown = false;
        }
    }

    private bool _explosionShown;

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1.5f);
        boss.grabCollider.enabled = true;
        yield return new WaitForSeconds(0.5f);
        _shouldMove = false;
        boss.grabCollider.enabled = false;

        yield return new WaitForSeconds(2.3f);
        boss.SetGrabExplosionVFXClientRpc(true); // RPC so clients see it too (this coroutine only runs on the server)
        _explosionShown = true;

        yield return new WaitForSeconds(0.5f);
        boss.SetGrabExplosionVFXClientRpc(false);
        _explosionShown = false;
        
        yield return new WaitForSeconds(1.158f);
        boss.stateMachine.Transit(boss.movementState);
    }
    
    public override void ContinuousAction()
    {
        if (!boss.currentTarget)
        {
            return;
        }
        if (!_shouldMove) return;
        
        Vector3 targetPosition = boss.currentTarget.transform.position;
        boss.MoveToward(targetPosition, boss.speed * 3f, BossBehaviour.StoppingDistance); // lunges in at triple speed
        boss.FaceDirection(boss.FlatDirectionTo(targetPosition));
    }
}
