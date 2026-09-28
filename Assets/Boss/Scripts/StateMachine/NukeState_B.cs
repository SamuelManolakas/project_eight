using System.Collections;
using UnityEngine;

public class NukeState_B : State_B
{
    public NukeState_B(BossBehaviour boss, State_B parent) : base(boss , parent){}

    
    private bool _hasReachedPosition = false;
    private bool _isRotating = false;
    public override void Enter()
    {
        boss.animator.Play("Idle/Run");
    }

    public override void Exit()
    {
        _hasReachedPosition = false;
       
    }

    public override void ContinuousAction()
    {
        if (!boss.currentTarget)
        {
            return;
        }

        if (_isRotating)
        {
            Vector3 faceDirection = -boss.nukePosition.forward;
            boss.TurnToward(boss.lowerBody.transform, faceDirection, 2f);
            boss.TurnToward(boss.upperBody.transform, faceDirection, 3f);
        }

        if (_hasReachedPosition) return;

        MoveToPosition();

        if (Vector3.Distance(boss.transform.position, boss.nukePosition.position) <= 5f)
        {
            _hasReachedPosition = true;
            _isRotating =  true;
            OnPositionReached();
        }
    }

    private void MoveToPosition()
    {
        // Sprints to the nuke position; the caller stops this once within 5m
        Vector3 move = boss.FlatDirectionTo(boss.nukePosition.position);
        boss.MoveToward(boss.nukePosition.position, boss.speed + 10f, 0f);
        boss.animator.SetFloat(AnimatorParams.Speed, move.magnitude);
        boss.FaceDirection(move);
    }

    private void OnPositionReached()
    {
        StartCoroutine(NukeSequence());
        
        //Audio
        if (boss.bossAudioScriptableObject != null)
        {
            boss.bossAudioNetworker.TriggerNukeAudio();

        }
        boss.SetEngineSound(0);
    }

    private IEnumerator NukeSequence()
    {
        yield return new WaitForSeconds(2f);
        _isRotating = false;
        boss.animator.Play("NukeIntro");
        boss.SetTelegraphVFXClientRpc(true);

        yield return new WaitForSeconds(2f);
        boss.animator.Play("NukeCharge");

        yield return new WaitForSeconds(4f);
        boss.animator.Play("NukeExit");

        boss.SetTelegraphVFXClientRpc(false);
        boss.SetNukeVFXClientRpc(true);

        ExecuteNuke();

        yield return new WaitForSeconds(1f);
        boss.SetNukeVFXClientRpc(false);

        yield return new WaitForSeconds(9f);
        boss.stateMachine.Transit(boss.movementState);
    }

    private void ExecuteNuke()
    {
        Collider[] hits = Physics.OverlapSphere(boss.transform.position, boss.nukeRadius, boss.targetLayerMask);

        foreach (Collider hit in hits)
        {
            if (HasLineOfSight(hit.transform))
            {
                // Player is exposed — deal damage
                if (hit.TryGetComponent<HurtBox>(out var health))
                    health.GetHit(boss.nukeDamage, 1);

                Debug.Log($"{hit.name} was hit by the nuke!");
            }
            else
            {
                Debug.Log($"{hit.name} is safe behind cover!");
            }
        }
    }
    
    private bool HasLineOfSight(Transform target)
    {
        Vector3 origin    = boss.transform.position;
        Vector3 direction = (target.position - origin).normalized;
        float   distance  = Vector3.Distance(origin, target.position);

        // If the ray hits something on the occlusion layer before reaching
        // the player, they are behind cover
        return !Physics.Raycast(origin, direction, distance, boss.occlusionLayerMask);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawSphere(boss.transform.position, boss.nukeRadius);
    }
}
