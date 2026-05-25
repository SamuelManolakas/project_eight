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
        boss.StartCoroutine(Nuke());
    }

    private IEnumerator Nuke()
    {
        yield return new WaitForSeconds(2f);
        _isRotating = false;
        boss.animator.Play("NukeIntro");
        
        yield return new WaitForSeconds(2f);
        boss.animator.Play("NukeCharge");
        
        yield return new WaitForSeconds(5f);
        boss.animator.Play("NukeExit");
        
        //nuke damage logic here
        
        yield return new WaitForSeconds(10f);
        boss.stateMachine.Transit(boss.movementState);
    }
}
