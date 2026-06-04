using System.Collections;
using System.Collections.Generic;
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
       
        boss.StopAllCoroutines();
    }

    public override void ContinuousAction()
    {
        if (!boss.currentTarget)
        {
            return;
        }

        if (_isRotating)
        {
            Quaternion toRotation = Quaternion.LookRotation(-boss.nukePosition.forward, Vector3.up);
            boss.lowerBody.transform.rotation = Quaternion.Slerp(boss.lowerBody.transform.rotation, toRotation, Time.deltaTime * 2);
            boss.upperBody.transform.rotation = Quaternion.Slerp(boss.upperBody.transform.rotation, toRotation, Time.deltaTime * 3);
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
        Vector3 move = boss.nukePosition.position - boss.transform.position;
        move = Vector3.ClampMagnitude(move, 1);
        move.y = 0;
        
        Vector3 finalMove = move * (boss.speed + 10f);
        
        boss.controller.Move(finalMove * Time.deltaTime);
        boss.animator.SetFloat("speed", move.magnitude);
        
        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(move, Vector3.up);
            boss.lowerBody.transform.rotation = Quaternion.Slerp(boss.lowerBody.transform.rotation, toRotation, Time.deltaTime);
            boss.upperBody.transform.rotation = Quaternion.Slerp(boss.upperBody.transform.rotation, toRotation, Time.deltaTime * 3);
        }
    }

    private void OnPositionReached()
    {
        boss.StartCoroutine(NukeSequence());
    }

    private IEnumerator NukeSequence()
    {
        yield return new WaitForSeconds(2f);
        _isRotating = false;
        boss.animator.Play("NukeIntro");
        if (boss.telegraphVFX) boss.telegraphVFX.SetActive(true);
        
        yield return new WaitForSeconds(2f);
        boss.animator.Play("NukeCharge");
        
        yield return new WaitForSeconds(5f);
        boss.animator.Play("NukeExit");
        
        if (boss.telegraphVFX) boss.telegraphVFX.SetActive(false);
        if (boss.nukeVFX)      boss.nukeVFX.SetActive(true);
        
        ExecuteNuke();
        
        yield return new WaitForSeconds(1f);
        if (boss.nukeVFX)      boss.nukeVFX.SetActive(false);
        
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
